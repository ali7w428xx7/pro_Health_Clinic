using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.DTOs;

public class DoctorDto
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string Bio { get; set; } = "";
    public bool IsActive { get; set; }
    public List<string> Specializations { get; set; } = [];
}

public class DoctorAvailabilityDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public DateOnly Date { get; set; }
    public List<TimeSlotDto> AvailableSlots { get; set; } = [];
}

public class TimeSlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Display => $"{StartTime:hh\\:mm} – {EndTime:hh\\:mm}";
}
