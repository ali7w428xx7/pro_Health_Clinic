using ClinicAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = "";
    public string PatientCPR { get; set; } = "";
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public int SpecializationId { get; set; }
    public string SpecializationName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = "";
    public string ReasonForVisit { get; set; } = "";
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentDto
{
    [Required] public int PatientId { get; set; }
    [Required] public int DoctorId { get; set; }
    [Required] public int SpecializationId { get; set; }
    [Required] public DateOnly AppointmentDate { get; set; }
    [Required] public TimeSpan StartTime { get; set; }
    [Required, MaxLength(500)] public string ReasonForVisit { get; set; } = "";
}

public class UpdateAppointmentStatusDto
{
    [Required] public AppointmentStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
}

public class PublicLookupDto
{
    [Required] public string CPRNumber { get; set; } = "";
    [Required] public string PatientReferenceNumber { get; set; } = "";
}

public class PublicLookupResultDto
{
    public string PatientName { get; set; } = "";
    public List<AppointmentDto> UpcomingAppointments { get; set; } = [];
    public List<RecentVisitDto> RecentVisits { get; set; } = [];
}

public class RecentVisitDto
{
    public DateOnly AppointmentDate { get; set; }
    public string DoctorName { get; set; } = "";
    public string SpecializationName { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string TreatmentPlan { get; set; } = "";
}
