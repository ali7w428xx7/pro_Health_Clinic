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
    public async Task<IActionResult> DoctorUtilization([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.DoctorSpecializations).ThenInclude(ds => ds.Specialization)
            .Include(d => d.Appointments)
            .Where(d => d.IsActive)
            .ToListAsync();

        var result = doctors.Select(d =>
        {
            var appointments = d.Appointments.AsQueryable();
            if (from.HasValue) appointments = appointments.Where(a => a.AppointmentDate >= from.Value);
            if (to.HasValue) appointments = appointments.Where(a => a.AppointmentDate <= to.Value);
            var filteredAppointments = appointments.ToList();

            return new DoctorUtilizationDto
        {
            DoctorId = d.Id,
            DoctorName = d.ApplicationUser.FullName,
            Specializations = d.DoctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
            TotalAppointments = filteredAppointments.Count,
            CompletedAppointments = filteredAppointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = filteredAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            UtilizationRate = filteredAppointments.Count > 0
                ? Math.Round((double)filteredAppointments.Count(a => a.Status == AppointmentStatus.Completed) / filteredAppointments.Count * 100, 1)
                : 0
            };
        }).OrderByDescending(d => d.TotalAppointments).ToList();

        return Ok(result);
    }

    [HttpGet("cancellation-rates")]
    public async Task<IActionResult> CancellationRates([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var doctors = await _db.Doctors
            .Include(d => d.ApplicationUser)
            .Include(d => d.Appointments)
            .Where(d => d.IsActive)
            .ToListAsync();

        var allQuery = _db.Appointments.AsQueryable();
        if (from.HasValue) allQuery = allQuery.Where(a => a.AppointmentDate >= from.Value);
        if (to.HasValue) allQuery = allQuery.Where(a => a.AppointmentDate <= to.Value);

        var all = await allQuery.ToListAsync();
        var total = all.Count;
        var cancelled = all.Count(a => a.Status == AppointmentStatus.Cancelled);
        var missed = all.Count(a => a.Status == AppointmentStatus.Missed);

        var byDoctor = doctors.Select(d =>
        {
            var appointments = d.Appointments.AsQueryable();
            if (from.HasValue) appointments = appointments.Where(a => a.AppointmentDate >= from.Value);
            if (to.HasValue) appointments = appointments.Where(a => a.AppointmentDate <= to.Value);
            var filteredAppointments = appointments.ToList();

            return new DoctorCancellationDto
            {
                DoctorName = d.ApplicationUser.FullName,
                Total = filteredAppointments.Count,
                Cancelled = filteredAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                Missed = filteredAppointments.Count(a => a.Status == AppointmentStatus.Missed),
                CancellationRate = filteredAppointments.Count > 0
                    ? Math.Round((double)filteredAppointments.Count(a => a.Status == AppointmentStatus.Cancelled) / filteredAppointments.Count * 100, 1) : 0,
                NoShowRate = filteredAppointments.Count > 0
                    ? Math.Round((double)filteredAppointments.Count(a => a.Status == AppointmentStatus.Missed) / filteredAppointments.Count * 100, 1) : 0
            };
        }).OrderByDescending(d => d.CancellationRate).ToList();

        return Ok(new CancellationRateDto
        {
            OverallCancellationRate = total > 0 ? Math.Round((double)cancelled / total * 100, 1) : 0,
            OverallNoShowRate = total > 0 ? Math.Round((double)missed / total * 100, 1) : 0,
            ByDoctor = byDoctor
        });
    }
}
