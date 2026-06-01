using System.Security.Claims;
using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Hubs;
using ClinicAPI.Models;
using ClinicAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly ClinicDbContext _db;
    private readonly IAppointmentService _apptService;
    private readonly INotificationService _notifyService;
    private readonly IHubContext<AppointmentHub> _hub;

    public AppointmentsController(ClinicDbContext db, IAppointmentService apptService,
        INotificationService notifyService, IHubContext<AppointmentHub> hub)
    {
        _db = db;
        _apptService = apptService;
        _notifyService = notifyService;
        _hub = hub;
    }

    // PUBLIC: lookup by CPR + reference number (no auth required)
    [HttpGet("lookup")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicLookup([FromQuery] string cprNumber, [FromQuery] string patientReferenceNumber)
    {
        if (string.IsNullOrWhiteSpace(cprNumber) || string.IsNullOrWhiteSpace(patientReferenceNumber))
            return BadRequest(new { message = "CPR number and patient reference number are required." });

        var patient = await _db.Patients
            .Include(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.CPRNumber == cprNumber && p.PatientReferenceNumber == patientReferenceNumber);

        if (patient == null)
            return NotFound(new { message = "No patient found with the provided details." });

        var today = DateOnly.FromDateTime(DateTime.Today);

        var upcoming = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .Where(a => a.PatientId == patient.Id &&
                        a.AppointmentDate >= today &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Missed)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(10)
            .Select(a => MapAppointmentDto(a, patient))
            .ToListAsync();

        var recentVisits = await _db.VisitRecords
            .Include(vr => vr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(vr => vr.Appointment).ThenInclude(a => a.Specialization)
            .Where(vr => vr.Appointment.PatientId == patient.Id)
            .OrderByDescending(vr => vr.Appointment.AppointmentDate)
            .Take(5)
            .Select(vr => new RecentVisitDto
            {
                AppointmentDate = vr.Appointment.AppointmentDate,
                DoctorName = vr.Appointment.Doctor.ApplicationUser.FullName,
                SpecializationName = vr.Appointment.Specialization.Name,
                Diagnosis = vr.Diagnosis,
                TreatmentPlan = vr.TreatmentPlan
            })
            .ToListAsync();

        return Ok(new PublicLookupResultDto
        {
            PatientName = patient.ApplicationUser.FullName,
            UpcomingAppointments = upcoming,
            RecentVisits = recentVisits
        });
    }

    // GET /api/appointments — filtered list (JWT required)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] int? doctorId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .AsQueryable();

        if (doctorId.HasValue) query = query.Where(a => a.DoctorId == doctorId.Value);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var s))
            query = query.Where(a => a.Status == s);
        if (from.HasValue) query = query.Where(a => a.AppointmentDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.AppointmentDate <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapAppointmentDto(a, a.Patient))
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // POST /api/appointments — create (Patient or Receptionist)
    [HttpPost]
    [Authorize(Roles = "Patient,Receptionist,ClinicManager")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var doctor = await _db.Doctors.FindAsync(dto.DoctorId);
        if (doctor == null || !doctor.IsActive) return NotFound(new { message = "Doctor not found." });

        var schedule = await _db.DoctorSchedules.FirstOrDefaultAsync(s =>
            s.DoctorId == dto.DoctorId && s.DayOfWeek == dto.AppointmentDate.DayOfWeek && s.IsActive);
        if (schedule == null) return BadRequest(new { message = "Doctor has no schedule on that day." });

        var endTime = dto.StartTime.Add(TimeSpan.FromMinutes(schedule.SlotDurationMinutes));

        var available = await _apptService.IsSlotAvailableAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, endTime);
        if (!available) return Conflict(new { message = "The selected time slot is not available." });

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            SpecializationId = dto.SpecializationId,
            AppointmentDate = dto.AppointmentDate,
            StartTime = dto.StartTime,
            EndTime = endTime,
            ReasonForVisit = dto.ReasonForVisit,
            Status = AppointmentStatus.Requested
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        // Notify patient and doctor
        var patient = await _db.Patients.Include(p => p.ApplicationUser).FirstOrDefaultAsync(p => p.Id == dto.PatientId);
        var doctorUser = await _db.Doctors.Include(d => d.ApplicationUser).FirstOrDefaultAsync(d => d.Id == dto.DoctorId);
        if (patient == null || doctorUser == null) return NotFound(new { message = "Patient or doctor not found." });

        await _notifyService.SendToManyAsync(
            [patient.ApplicationUserId, doctorUser.ApplicationUserId],
            "Appointment Booked",
            $"Appointment on {dto.AppointmentDate:dd/MM/yyyy} at {dto.StartTime:hh\\:mm} has been booked.",
            NotificationType.AppointmentBooked, appointment.Id);

        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            appointment.Id,
            appointment.Status,
            AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = appointment.StartTime.ToString(@"hh\:mm"),
            PatientName = patient.ApplicationUser.FullName,
            DoctorName = doctorUser.ApplicationUser.FullName
        });

        return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, appointment.Id);
    }

    // PUT /api/appointments/{id}/status — update status with transition validation
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var appointment = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
            .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
            .Include(a => a.Specialization)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound();

        if (!_apptService.IsValidTransition(appointment.Status, dto.Status))
            return BadRequest(new { message = $"Cannot transition from {appointment.Status} to {dto.Status}." });

        appointment.Status = dto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        if (dto.Status == AppointmentStatus.Cancelled && dto.CancellationReason != null)
            appointment.CancellationReason = dto.CancellationReason;

        // Create visit record when completing
        if (dto.Status == AppointmentStatus.Completed)
        {
            _db.VisitRecords.Add(new VisitRecord
            {
                AppointmentId = appointment.Id,
                DoctorNotes = dto.DoctorNotes ?? "",
                Diagnosis = dto.Diagnosis ?? "",
                TreatmentPlan = dto.TreatmentPlan ?? ""
            });
        }

        await _db.SaveChangesAsync();

        // Notify affected users
        var notifyMessage = $"Appointment on {appointment.AppointmentDate:dd/MM/yyyy} is now {dto.Status}.";
        await _notifyService.SendToManyAsync(
            [appointment.Patient.ApplicationUserId, appointment.Doctor.ApplicationUserId],
            "Appointment Update", notifyMessage, NotificationType.AppointmentStatusChanged, id);

        // Broadcast via SignalR
        await _hub.Clients.All.SendAsync("AppointmentStatusChanged", new
        {
            appointment.Id,
            Status = appointment.Status.ToString(),
            AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
            StartTime = appointment.StartTime.ToString(@"hh\:mm"),
            PatientName = appointment.Patient.ApplicationUser.FullName,
            DoctorName = appointment.Doctor.ApplicationUser.FullName
        });

        return NoContent();
    }

    private static AppointmentDto MapAppointmentDto(Appointment a, Patient p) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        PatientName = p.ApplicationUser.FullName,
        PatientCPR = p.CPRNumber,
        DoctorId = a.DoctorId,
        DoctorName = a.Doctor.ApplicationUser.FullName,
        SpecializationId = a.SpecializationId,
        SpecializationName = a.Specialization.Name,
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status.ToString(),
        ReasonForVisit = a.ReasonForVisit,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt
    };
}
