using ClinicAPI.DTOs;
using ClinicAPI.Models;

namespace ClinicAPI.Services;

public interface IAppointmentService
{
    Task<bool> IsSlotAvailableAsync(int doctorId, DateOnly date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null);
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date);
    bool IsValidTransition(AppointmentStatus current, AppointmentStatus next);
}
