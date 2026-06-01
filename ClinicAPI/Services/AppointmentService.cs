using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ClinicDbContext _db;

    public AppointmentService(ClinicDbContext db) => _db = db;

    public async Task<bool> IsSlotAvailableAsync(int doctorId, DateOnly date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
    {
        // Check doctor is on leave
        var onLeave = await _db.LeavePeriods.AnyAsync(l =>
            l.DoctorId == doctorId &&
            l.IsApproved &&
            l.StartDate <= date && l.EndDate >= date);
        if (onLeave) return false;

        // Check doctor has a schedule for that day
        var schedule = await _db.DoctorSchedules.FirstOrDefaultAsync(s =>
            s.DoctorId == doctorId &&
            s.DayOfWeek == date.DayOfWeek &&
            s.IsActive);
        if (schedule == null) return false;

        // Slot must be within working hours
        if (startTime < schedule.StartTime || endTime > schedule.EndTime) return false;

        // Check no overlapping confirmed/active appointments
        var conflict = await _db.Appointments.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.AppointmentDate == date &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Missed &&
            (excludeAppointmentId == null || a.Id != excludeAppointmentId) &&
            a.StartTime < endTime && a.EndTime > startTime);

        return !conflict;
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        var schedule = await _db.DoctorSchedules.FirstOrDefaultAsync(s =>
            s.DoctorId == doctorId &&
            s.DayOfWeek == date.DayOfWeek &&
            s.IsActive);
        if (schedule == null) return [];

        var onLeave = await _db.LeavePeriods.AnyAsync(l =>
            l.DoctorId == doctorId &&
            l.IsApproved &&
            l.StartDate <= date && l.EndDate >= date);
        if (onLeave) return [];

        var bookedSlots = await _db.Appointments
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate == date &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Missed)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync();

        var slots = new List<TimeSlotDto>();
        var current = schedule.StartTime;
        var duration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

        while (current + duration <= schedule.EndTime)
        {
            var end = current + duration;
            var isBooked = bookedSlots.Any(b => b.StartTime < end && b.EndTime > current);
            if (!isBooked)
                slots.Add(new TimeSlotDto { StartTime = current, EndTime = end });
            current = end;
        }

        return slots;
    }

    public bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
    {
        return (current, next) switch
        {
            (AppointmentStatus.Requested, AppointmentStatus.Confirmed) => true,
            (AppointmentStatus.Requested, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Missed) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.InProgress) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Missed) => true,
            _ => false
        };
    }
}
