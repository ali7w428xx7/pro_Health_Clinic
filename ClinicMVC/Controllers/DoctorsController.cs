using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicAPI.Services;
using ClinicMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers;

[Authorize(Roles = "ClinicManager")]
public class DoctorsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly INotificationService _notifyService;

    public DoctorsController(ClinicDbContext db, INotificationService notifyService)
    {
        _db = db;
        _notifyService = notifyService;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.DoctorSpecializations).ThenInclude(ds => ds.Specialization)
            .Include(d => d.Appointments)
            .Select(d => new DoctorSummaryViewModel
            {
                Id = d.Id,
                FullName = d.ApplicationUser.FirstName + " " + d.ApplicationUser.LastName,
                Email = d.ApplicationUser.Email!,
                LicenseNumber = d.LicenseNumber,
                Bio = d.Bio,
                IsActive = d.IsActive,
                TotalAppointments = d.Appointments.Count,
                Specializations = d.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList()
            }).ToListAsync();

        return View(new DoctorListViewModel { Doctors = doctors });
    }

    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.DoctorSpecializations).ThenInclude(ds => ds.Specialization)
            .Include(d => d.Schedules)
            .Include(d => d.LeavePeriods)
            .Include(d => d.Appointments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null) return NotFound();

        return View(new DoctorDetailViewModel
        {
            Id = doctor.Id,
            FullName = doctor.ApplicationUser.FullName,
            Email = doctor.ApplicationUser.Email!,
            LicenseNumber = doctor.LicenseNumber,
            Bio = doctor.Bio,
            IsActive = doctor.IsActive,
            TotalAppointments = doctor.Appointments.Count,
            Specializations = doctor.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
            Schedules = doctor.Schedules.Select(s => new ScheduleViewModel
            {
                Id = s.Id, DayOfWeek = s.DayOfWeek, StartTime = s.StartTime,
                EndTime = s.EndTime, SlotDurationMinutes = s.SlotDurationMinutes, IsActive = s.IsActive
            }).ToList(),
            LeavePeriods = doctor.LeavePeriods.Select(l => new LeaveViewModel
            {
                Id = l.Id, StartDate = l.StartDate, EndDate = l.EndDate,
                Reason = l.Reason, IsApproved = l.IsApproved
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _db.Doctors
            .Include(d => d.DoctorSpecializations)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (doctor == null) return NotFound();

        return View(new EditDoctorViewModel
        {
            Id = doctor.Id,
            Bio = doctor.Bio,
            IsActive = doctor.IsActive,
            SpecializationIds = doctor.DoctorSpecializations.Select(ds => ds.SpecializationId).ToList(),
            AllSpecializations = await _db.Specializations
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditDoctorViewModel model)
    {
        model.AllSpecializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync();

        if (!ModelState.IsValid) return View(model);

        var doctor = await _db.Doctors.Include(d => d.DoctorSpecializations).FirstOrDefaultAsync(d => d.Id == model.Id);
        if (doctor == null) return NotFound();

        doctor.Bio = model.Bio;
        doctor.IsActive = model.IsActive;

        // Update specializations
        _db.DoctorSpecializations.RemoveRange(doctor.DoctorSpecializations);
        foreach (var specId in model.SpecializationIds)
            _db.DoctorSpecializations.Add(new DoctorSpecialization { DoctorId = doctor.Id, SpecializationId = specId });

        await _db.SaveChangesAsync();

        // If doctor deactivated, notify patients with upcoming appointments
        if (!model.IsActive)
        {
            var affected = await _db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
                .Where(a => a.DoctorId == doctor.Id &&
                            a.AppointmentDate >= DateOnly.FromDateTime(DateTime.Today) &&
                            a.Status == AppointmentStatus.Requested || a.Status == AppointmentStatus.Confirmed)
                .ToListAsync();

            foreach (var appt in affected)
            {
                appt.Status = AppointmentStatus.Cancelled;
                appt.CancellationReason = "Doctor is no longer available.";
            }
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Doctor profile updated.";
        return RedirectToAction("Details", new { id = model.Id });
    }

    [HttpGet]
    public IActionResult AddSchedule(int doctorId) =>
        View(new AddScheduleViewModel { DoctorId = doctorId });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSchedule(AddScheduleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
            return View(model);
        }

        _db.DoctorSchedules.Add(new DoctorSchedule
        {
            DoctorId = model.DoctorId,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            SlotDurationMinutes = model.SlotDurationMinutes,
            IsActive = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Schedule added.";
        return RedirectToAction("Details", new { id = model.DoctorId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSchedule(int id, int doctorId)
    {
        var schedule = await _db.DoctorSchedules.FindAsync(id);
        if (schedule != null) { _db.DoctorSchedules.Remove(schedule); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Schedule removed.";
        return RedirectToAction("Details", new { id = doctorId });
    }

    [HttpGet]
    public IActionResult AddLeave(int doctorId) =>
        View(new AddLeaveViewModel { DoctorId = doctorId });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLeave(AddLeaveViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
            return View(model);
        }

        _db.LeavePeriods.Add(new LeavePeriod
        {
            DoctorId = model.DoctorId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason,
            IsApproved = true
        });
        await _db.SaveChangesAsync();

        // Cancel all Requested/Confirmed appointments that fall within the leave period
        var affected = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Where(a => a.DoctorId == model.DoctorId &&
                        a.AppointmentDate >= model.StartDate &&
                        a.AppointmentDate <= model.EndDate &&
                        (a.Status == AppointmentStatus.Requested || a.Status == AppointmentStatus.Confirmed))
            .ToListAsync();

        foreach (var appt in affected)
        {
            appt.Status = AppointmentStatus.Cancelled;
            appt.CancellationReason = $"Doctor is on leave from {model.StartDate:dd/MM/yyyy} to {model.EndDate:dd/MM/yyyy}.";
            appt.UpdatedAt = DateTime.UtcNow;

            await _notifyService.SendAsync(
                appt.Patient.ApplicationUserId,
                "Appointment Cancelled",
                $"Your appointment on {appt.AppointmentDate:dd/MM/yyyy} at {appt.StartTime:hh\\:mm} has been cancelled because the doctor is on leave.",
                NotificationType.AppointmentCancelled, appt.Id);
        }

        if (affected.Count > 0)
            await _db.SaveChangesAsync();

        TempData["Success"] = affected.Count > 0
            ? $"Leave period added and {affected.Count} affected appointment(s) cancelled."
            : "Leave period added.";
        return RedirectToAction("Details", new { id = model.DoctorId });
    }

    // Specializations CRUD
    public async Task<IActionResult> Specializations()
    {
        var specs = await _db.Specializations
            .Include(s => s.DoctorSpecializations)
            .Select(s => new SpecializationViewModel
            {
                Id = s.Id, Name = s.Name, Description = s.Description,
                DoctorCount = s.DoctorSpecializations.Count
            }).ToListAsync();
        return View(specs);
    }

    [HttpGet]
    public IActionResult CreateSpecialization() => View(new CreateSpecializationViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSpecialization(CreateSpecializationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _db.Specializations.AnyAsync(s => s.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "A specialization with this name already exists.");
            return View(model);
        }
        _db.Specializations.Add(new Specialization { Name = model.Name, Description = model.Description });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Specialization created.";
        return RedirectToAction("Specializations");
    }
}
