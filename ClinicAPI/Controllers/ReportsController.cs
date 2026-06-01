using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "ClinicManager")]
public class ReportsController : ControllerBase
{
    private readonly ClinicDbContext _db;

    public ReportsController(ClinicDbContext db) => _db = db;

    [HttpGet("appointment-statistics")]
    public async Task<IActionResult> AppointmentStatistics([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var query = _db.Appointments
            .Include(a => a.Specialization)
            .AsQueryable();

        if (from.HasValue) query = query.Where(a => a.AppointmentDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.AppointmentDate <= to.Value);

        var all = await query.ToListAsync();

        var total = all.Count;
        var completed = all.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelled = all.Count(a => a.Status == AppointmentStatus.Cancelled);
        var missed = all.Count(a => a.Status == AppointmentStatus.Missed);

        var byMonth = all
            .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Total = g.Count(),
                Completed = g.Count(a => a.Status == AppointmentStatus.Completed),
                Cancelled = g.Count(a => a.Status == AppointmentStatus.Cancelled)
            }).ToList();

        var bySpec = all
            .GroupBy(a => a.Specialization.Name)
            .Select(g => new SpecializationStatDto { Name = g.Key, Total = g.Count() })
            .OrderByDescending(s => s.Total).ToList();

        return Ok(new AppointmentStatsDto
        {
            Total = total,
            Completed = completed,
            Cancelled = cancelled,
            Missed = missed,
            Requested = all.Count(a => a.Status == AppointmentStatus.Requested),
            Confirmed = all.Count(a => a.Status == AppointmentStatus.Confirmed),
            CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
            CancellationRate = total > 0 ? Math.Round((double)cancelled / total * 100, 1) : 0,
            NoShowRate = total > 0 ? Math.Round((double)missed / total * 100, 1) : 0,
            ByMonth = byMonth,
            BySpecialization = bySpec
        });
    }

    [HttpGet("doctor-utilization")]
    public async Task<IActionResult> DoctorUtilization()
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.DoctorSpecializations).ThenInclude(ds => ds.Specialization)
            .Include(d => d.Appointments)
            .Where(d => d.IsActive)
            .ToListAsync();

        var result = doctors.Select(d => new DoctorUtilizationDto
        {
            DoctorId = d.Id,
            DoctorName = d.ApplicationUser.FullName,
            Specializations = d.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
            TotalAppointments = d.Appointments.Count,
            CompletedAppointments = d.Appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = d.Appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            UtilizationRate = d.Appointments.Count > 0
                ? Math.Round((double)d.Appointments.Count(a => a.Status == AppointmentStatus.Completed) / d.Appointments.Count * 100, 1)
                : 0
        }).OrderByDescending(d => d.TotalAppointments).ToList();

        return Ok(result);
    }

    [HttpGet("cancellation-rates")]
    public async Task<IActionResult> CancellationRates()
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.Appointments)
            .Where(d => d.IsActive)
            .ToListAsync();

        var all = await _db.Appointments.ToListAsync();
        var total = all.Count;
        var cancelled = all.Count(a => a.Status == AppointmentStatus.Cancelled);
        var missed = all.Count(a => a.Status == AppointmentStatus.Missed);

        var byDoctor = doctors.Select(d => new DoctorCancellationDto
        {
            DoctorName = d.ApplicationUser.FullName,
            Total = d.Appointments.Count,
            Cancelled = d.Appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            Missed = d.Appointments.Count(a => a.Status == AppointmentStatus.Missed),
            CancellationRate = d.Appointments.Count > 0
                ? Math.Round((double)d.Appointments.Count(a => a.Status == AppointmentStatus.Cancelled) / d.Appointments.Count * 100, 1) : 0,
            NoShowRate = d.Appointments.Count > 0
                ? Math.Round((double)d.Appointments.Count(a => a.Status == AppointmentStatus.Missed) / d.Appointments.Count * 100, 1) : 0
        }).OrderByDescending(d => d.CancellationRate).ToList();

        return Ok(new CancellationRateDto
        {
            OverallCancellationRate = total > 0 ? Math.Round((double)cancelled / total * 100, 1) : 0,
            OverallNoShowRate = total > 0 ? Math.Round((double)missed / total * 100, 1) : 0,
            ByDoctor = byDoctor
        });
    }
}
