using ClinicAPI.Data;
using ClinicAPI.Hubs;
using ClinicAPI.Models;
using ClinicAPI.Services;
using ClinicMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppointmentService _apptService;
    private readonly INotificationService _notifyService;
    private readonly IHubContext<AppointmentHub> _hub;
    private readonly string _apiBaseUrl;

    public AppointmentsController(ClinicDbContext db, UserManager<ApplicationUser> userManager,
        IAppointmentService apptService, INotificationService notifyService,
        IHubContext<AppointmentHub> hub, IConfiguration config)
    {
        _db = db;
        _userManager = userManager;
        _apptService = apptService;
        _notifyService = notifyService;
        _hub = hub;
        _apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5095";
    }

    public async Task<IActionResult> Index(string? status, DateOnly? from, DateOnly? to, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var query = _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .AsQueryable();

        // Scope to role
        if (user.Role == UserRole.Patient)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
            if (patient != null) query = query.Where(a => a.PatientId == patient.Id);
        }
        else if (user.Role == UserRole.Doctor)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
            if (doctor != null) query = query.Where(a => a.DoctorId == doctor.Id);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var s))
            query = query.Where(a => a.Status == s);
        if (from.HasValue) query = query.Where(a => a.AppointmentDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.AppointmentDate <= to.Value);

        const int pageSize = 15;
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentRowViewModel
            {
                Id = a.Id,
                PatientName = a.Patient.ApplicationUser.FirstName + " " + a.Patient.ApplicationUser.LastName,
                PatientCPR = a.Patient.CPRNumber,
                DoctorName = a.Doctor.ApplicationUser.FirstName + " " + a.Doctor.ApplicationUser.LastName,
                SpecializationName = a.Specialization.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                ReasonForVisit = a.ReasonForVisit,
                CancellationReason = a.CancellationReason,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return View(new AppointmentListViewModel
        {
            Appointments = items,
            StatusFilter = status,
            DateFrom = from,
            DateTo = to,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            TotalCount = total
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await _db.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(x => x.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(x => x.Specialization)
            .Include(x => x.VisitRecord).ThenInclude(vr => vr!.Prescriptions)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a == null) return NotFound();

        var vm = new AppointmentDetailViewModel
        {
            Id = a.Id,
            PatientName = a.Patient.ApplicationUser.FullName,
            PatientCPR = a.Patient.CPRNumber,
            DoctorName = a.Doctor.ApplicationUser.FullName,
            SpecializationName = a.Specialization.Name,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            ReasonForVisit = a.ReasonForVisit,
            CancellationReason = a.CancellationReason,
            CreatedAt = a.CreatedAt
        };

        if (a.VisitRecord != null)
        {
            vm.VisitRecord = new VisitRecordViewModel
            {
                Id = a.VisitRecord.Id,
                DoctorNotes = a.VisitRecord.DoctorNotes,
                Diagnosis = a.VisitRecord.Diagnosis,
                TreatmentPlan = a.VisitRecord.TreatmentPlan,
                CreatedAt = a.VisitRecord.CreatedAt
            };
            vm.Prescriptions = a.VisitRecord.Prescriptions.Select(p => new PrescriptionViewModel
            {
                Id = p.Id,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Frequency = p.Frequency,
                Duration = p.Duration,
                SpecialInstructions = p.SpecialInstructions,
                PrescribedAt = p.PrescribedAt
            }).ToList();
        }

        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = "Patient,Receptionist,ClinicManager")]
    public async Task<IActionResult> Book()
    {
        var vm = new BookAppointmentViewModel
        {
            Specializations = await _db.Specializations
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync()
        };

        // Receptionist can book for any patient
        if (User.IsInRole("Receptionist") || User.IsInRole("ClinicManager"))
        {
            vm.Patients = await _db.Patients
                .Include(p => p.ApplicationUser)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName + " (" + p.CPRNumber + ")"
                }).ToListAsync();
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Patient,Receptionist,ClinicManager")]
    public async Task<IActionResult> Book(BookAppointmentViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Determine patient
        int patientId;
        if (user.Role == UserRole.Patient)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
            if (patient == null) { TempData["Error"] = "Patient profile not found."; return RedirectToAction("Index"); }
            patientId = patient.Id;
        }
        else
        {
            if (!model.PatientId.HasValue) { ModelState.AddModelError(nameof(model.PatientId), "Please select a patient."); goto reload; }
            patientId = model.PatientId.Value;
        }

        if (!ModelState.IsValid) goto reload;

        // Parse slot
        var parts = model.TimeSlot.Split('-');
        if (parts.Length != 2 || !TimeSpan.TryParse(parts[0], out var start) || !TimeSpan.TryParse(parts[1], out var end))
        {
            ModelState.AddModelError(nameof(model.TimeSlot), "Invalid time slot.");
            goto reload;
        }

        var available = await _apptService.IsSlotAvailableAsync(model.DoctorId, model.AppointmentDate, start, end);
        if (!available)
        {
            ModelState.AddModelError("", "That slot is no longer available. Please choose another.");
            goto reload;
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = model.DoctorId,
            SpecializationId = model.SpecializationId,
            AppointmentDate = model.AppointmentDate,
            StartTime = start,
            EndTime = end,
            ReasonForVisit = model.ReasonForVisit,
            Status = AppointmentStatus.Requested
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        var patientRecord = await _db.Patients.Include(p => p.ApplicationUser).FirstAsync(p => p.Id == patientId);
        var doctor = await _db.Doctors.Include(d => d.ApplicationUser).FirstAsync(d => d.Id == model.DoctorId);

        await _notifyService.SendToManyAsync(
            [patientRecord.ApplicationUserId, doctor.ApplicationUserId],
            "Appointment Booked",
            $"Appointment on {model.AppointmentDate:dd/MM/yyyy} at {start:hh\\:mm} has been booked.",
            NotificationType.AppointmentBooked, appointment.Id);

        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            appointment.Id,
            Status = appointment.Status.ToString(),
            AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = appointment.StartTime.ToString(@"hh\:mm"),
            PatientName = patientRecord.ApplicationUser.FullName,
            DoctorName = doctor.ApplicationUser.FullName
        });

        TempData["Success"] = "Appointment booked successfully.";
        return RedirectToAction("Details", new { id = appointment.Id });

        reload:
        model.Specializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync();
        model.Patients = await _db.Patients.Include(p => p.ApplicationUser)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName + " (" + p.CPRNumber + ")"
            }).ToListAsync();
        return View(model);
    }

    // AJAX: get doctors by specialization
    [HttpGet]
    public async Task<IActionResult> GetDoctorsBySpecialization(int specializationId)
    {
        var doctors = await _db.DoctorSpecializations
            .Include(ds => ds.Doctor).ThenInclude(d => d.ApplicationUser)
            .Where(ds => ds.SpecializationId == specializationId && ds.Doctor.IsActive)
            .Select(ds => new { value = ds.Doctor.Id, text = ds.Doctor.ApplicationUser.FirstName + " " + ds.Doctor.ApplicationUser.LastName })
            .ToListAsync();
        return Json(doctors);
    }

    // AJAX: get available time slots
    [HttpGet]
    public async Task<IActionResult> GetTimeSlots(int doctorId, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate)) return BadRequest();
        var slots = await _apptService.GetAvailableSlotsAsync(doctorId, parsedDate);
        return Json(slots.Select(s => new { value = $"{s.StartTime:hh\\:mm\\:ss}-{s.EndTime:hh\\:mm\\:ss}", text = s.Display }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Receptionist,ClinicManager")]
    public async Task<IActionResult> Confirm(int id)
    {
        await UpdateStatus(id, AppointmentStatus.Confirmed);
        TempData["Success"] = "Appointment confirmed.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Receptionist,ClinicManager")]
    public async Task<IActionResult> CheckIn(int id)
    {
        await UpdateStatus(id, AppointmentStatus.CheckedIn);
        TempData["Success"] = "Patient checked in.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,ClinicManager")]
    public async Task<IActionResult> StartConsultation(int id)
    {
        await UpdateStatus(id, AppointmentStatus.InProgress);
        TempData["Success"] = "Consultation started.";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Doctor,ClinicManager")]
    public async Task<IActionResult> Complete(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();
        return View(new CreateVisitRecordViewModel { AppointmentId = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,ClinicManager")]
    public async Task<IActionResult> Complete(CreateVisitRecordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var a = await _db.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(x => x.Doctor).ThenInclude(d => d.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == model.AppointmentId);
        if (a == null) return NotFound();

        if (!_apptService.IsValidTransition(a.Status, AppointmentStatus.Completed))
        {
            TempData["Error"] = "Cannot complete this appointment in its current state.";
            return RedirectToAction("Details", new { id = model.AppointmentId });
        }

        a.Status = AppointmentStatus.Completed;
        a.UpdatedAt = DateTime.UtcNow;
        _db.VisitRecords.Add(new VisitRecord
        {
            AppointmentId = a.Id,
            DoctorNotes = model.DoctorNotes,
            Diagnosis = model.Diagnosis,
            TreatmentPlan = model.TreatmentPlan
        });
        await _db.SaveChangesAsync();

        await _notifyService.SendToManyAsync(
            [a.Patient.ApplicationUserId, a.Doctor.ApplicationUserId],
            "Appointment Completed", $"Your appointment on {a.AppointmentDate:dd/MM/yyyy} has been completed.",
            NotificationType.AppointmentStatusChanged, a.Id);

        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            a.Id, Status = "Completed",
            AppointmentDate = a.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = a.StartTime.ToString(@"hh\:mm"),
            PatientName = a.Patient.ApplicationUser.FullName,
            DoctorName = a.Doctor.ApplicationUser.FullName
        });

        TempData["Success"] = "Appointment completed and visit record saved.";
        return RedirectToAction("Details", new { id = a.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Cancel(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();
        return View(new CancelAppointmentViewModel { AppointmentId = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelAppointmentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var a = await _db.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(x => x.Doctor).ThenInclude(d => d.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == model.AppointmentId);
        if (a == null) return NotFound();

        if (!_apptService.IsValidTransition(a.Status, AppointmentStatus.Cancelled))
        {
            TempData["Error"] = "This appointment cannot be cancelled in its current state.";
            return RedirectToAction("Details", new { id = model.AppointmentId });
        }

        a.Status = AppointmentStatus.Cancelled;
        a.CancellationReason = model.CancellationReason;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _notifyService.SendToManyAsync(
            [a.Patient.ApplicationUserId, a.Doctor.ApplicationUserId],
            "Appointment Cancelled", $"Appointment on {a.AppointmentDate:dd/MM/yyyy} has been cancelled. Reason: {model.CancellationReason}",
            NotificationType.AppointmentCancelled, a.Id);

        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            a.Id, Status = "Cancelled",
            AppointmentDate = a.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = a.StartTime.ToString(@"hh\:mm"),
            PatientName = a.Patient.ApplicationUser.FullName,
            DoctorName = a.Doctor.ApplicationUser.FullName
        });

        TempData["Success"] = "Appointment cancelled.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Receptionist,ClinicManager")]
    public async Task<IActionResult> MarkMissed(int id)
    {
        await UpdateStatus(id, AppointmentStatus.Missed);
        TempData["Success"] = "Appointment marked as missed.";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Doctor,ClinicManager")]
    public async Task<IActionResult> AddPrescription(int visitRecordId)
    {
        var vr = await _db.VisitRecords.FindAsync(visitRecordId);
        if (vr == null) return NotFound();
        return View(new AddPrescriptionViewModel { VisitRecordId = visitRecordId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,ClinicManager")]
    public async Task<IActionResult> AddPrescription(AddPrescriptionViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var vr = await _db.VisitRecords.FindAsync(model.VisitRecordId);
        if (vr == null) return NotFound();

        _db.Prescriptions.Add(new Prescription
        {
            VisitRecordId = model.VisitRecordId,
            MedicationName = model.MedicationName,
            Dosage = model.Dosage,
            Frequency = model.Frequency,
            Duration = model.Duration,
            SpecialInstructions = model.SpecialInstructions
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Prescription added.";
        return RedirectToAction("Details", new { id = vr.AppointmentId });
    }

    // Live waiting room view (SignalR consumer)
    [Authorize(Roles = "Receptionist,ClinicManager,Doctor")]
    public async Task<IActionResult> WaitingRoom()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var appointments = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .Where(a => a.AppointmentDate == today &&
                        a.Status != AppointmentStatus.Completed &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Missed)
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentRowViewModel
            {
                Id = a.Id,
                PatientName = a.Patient.ApplicationUser.FirstName + " " + a.Patient.ApplicationUser.LastName,
                PatientCPR = a.Patient.CPRNumber,
                DoctorName = a.Doctor.ApplicationUser.FirstName + " " + a.Doctor.ApplicationUser.LastName,
                SpecializationName = a.Specialization.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status
            }).ToListAsync();

        ViewBag.ApiBaseUrl = _apiBaseUrl;
        return View(appointments);
    }

    private async Task UpdateStatus(int id, AppointmentStatus newStatus)
    {
        var a = await _db.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(x => x.Doctor).ThenInclude(d => d.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (a == null || !_apptService.IsValidTransition(a.Status, newStatus)) return;

        a.Status = newStatus;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _notifyService.SendToManyAsync(
            [a.Patient.ApplicationUserId, a.Doctor.ApplicationUserId],
            "Appointment Update", $"Appointment on {a.AppointmentDate:dd/MM/yyyy} is now {newStatus}.",
            NotificationType.AppointmentStatusChanged, id);

        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            a.Id, Status = newStatus.ToString(),
            AppointmentDate = a.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = a.StartTime.ToString(@"hh\:mm"),
            PatientName = a.Patient.ApplicationUser.FullName,
            DoctorName = a.Doctor.ApplicationUser.FullName
        });
    }
}
