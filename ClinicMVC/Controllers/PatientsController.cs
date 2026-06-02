using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientsController(ClinicDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Patient views their own profile; Manager/Doctor can view any
    public async Task<IActionResult> Profile(int? id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        Patient? patient;
        if (id.HasValue && (currentUser.Role == UserRole.ClinicManager || currentUser.Role == UserRole.Doctor || currentUser.Role == UserRole.Receptionist))
        {
            patient = await _db.Patients.Include(p => p.ApplicationUser).FirstOrDefaultAsync(p => p.Id == id.Value);
        }
        else
        {
            patient = await _db.Patients.Include(p => p.ApplicationUser).FirstOrDefaultAsync(p => p.ApplicationUserId == currentUser.Id);
        }

        if (patient == null) return NotFound();

        // Scope visit history: doctor sees only their own patients' records
        var visitQuery = _db.VisitRecords
            .Include(vr => vr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(vr => vr.Appointment).ThenInclude(a => a.Specialization)
            .Include(vr => vr.Prescriptions)
            .Where(vr => vr.Appointment.PatientId == patient.Id);

        if (currentUser.Role == UserRole.Doctor)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == currentUser.Id);
            if (doctor != null) visitQuery = visitQuery.Where(vr => vr.Appointment.DoctorId == doctor.Id);
        }

        var visits = await visitQuery
            .OrderByDescending(vr => vr.Appointment.AppointmentDate)
            .Select(vr => new VisitHistoryViewModel
            {
                AppointmentId = vr.AppointmentId,
                AppointmentDate = vr.Appointment.AppointmentDate,
                DoctorName = vr.Appointment.Doctor.ApplicationUser.FirstName + " " + vr.Appointment.Doctor.ApplicationUser.LastName,
                SpecializationName = vr.Appointment.Specialization.Name,
                Diagnosis = vr.Diagnosis,
                TreatmentPlan = vr.TreatmentPlan,
                DoctorNotes = vr.DoctorNotes,
                Prescriptions = vr.Prescriptions.Select(p => new PrescriptionViewModel
                {
                    Id = p.Id,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    Duration = p.Duration,
                    SpecialInstructions = p.SpecialInstructions,
                    PrescribedAt = p.PrescribedAt
                }).ToList()
            }).ToListAsync();

        var recentAppts = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .Take(5)
            .Select(a => new AppointmentRowViewModel
            {
                Id = a.Id,
                PatientName = patient.ApplicationUser.FullName,
                DoctorName = a.Doctor.ApplicationUser.FirstName + " " + a.Doctor.ApplicationUser.LastName,
                SpecializationName = a.Specialization.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status
            }).ToListAsync();

        return View(new PatientProfileViewModel
        {
            Id = patient.Id,
            FullName = patient.ApplicationUser.FullName,
            Email = patient.ApplicationUser.Email!,
            CPRNumber = patient.CPRNumber,
            PatientReferenceNumber = patient.PatientReferenceNumber,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Phone = patient.Phone,
            Address = patient.Address,
            BloodType = patient.BloodType,
            Allergies = patient.Allergies,
            VisitHistory = visits,
            RecentAppointments = recentAppts
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile(int? id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        Patient? patient;

        if (id.HasValue &&
            (currentUser.Role == UserRole.ClinicManager || currentUser.Role == UserRole.Doctor))
        {
            patient = await _db.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
        }
        else
        {
            patient = await _db.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUser.Id);
        }

        if (patient == null) return NotFound();

        var model = new PatientEditProfileViewModel
        {
            Id = patient.Id,
            Email = patient.ApplicationUser.Email ?? "",
            DateOfBirth = patient.DateOfBirth,
            Phone = patient.Phone,
            Address = patient.Address
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(PatientEditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        Patient? patient;

        if (currentUser.Role == UserRole.ClinicManager || currentUser.Role == UserRole.Doctor)
        {
            patient = await _db.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
        }
        else
        {
            patient = await _db.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUser.Id);
        }

        if (patient == null) return NotFound();

        patient.ApplicationUser.Email = model.Email;
        patient.ApplicationUser.UserName = model.Email;
        patient.ApplicationUser.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
        patient.ApplicationUser.NormalizedUserName = _userManager.NormalizeName(model.Email);

        patient.DateOfBirth = model.DateOfBirth;
        patient.Phone = model.Phone;
        patient.Address = model.Address;

        await _userManager.UpdateAsync(patient.ApplicationUser);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile updated successfully.";

        return RedirectToAction("Profile", new { id = patient.Id });
    }



    [Authorize(Roles = "ClinicManager,Receptionist,Doctor")]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Patients
            .Include(p => p.ApplicationUser)
            .Include(p => p.Appointments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.CPRNumber.Contains(search) ||
                                     (p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName).Contains(search));

        var patients = await query
            .Select(p => new PatientSummaryViewModel
            {
                Id = p.Id,
                FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                CPRNumber = p.CPRNumber,
                Phone = p.Phone,
                Gender = p.Gender,
                AppointmentCount = p.Appointments.Count,
                LastVisit = p.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => a.AppointmentDate)
                    .FirstOrDefault()
            }).ToListAsync();

        return View(new PatientListViewModel { Patients = patients, Search = search });
    }
}
