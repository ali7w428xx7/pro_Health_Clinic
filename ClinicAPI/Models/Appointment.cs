namespace ClinicAPI.Models;

public enum AppointmentStatus
{
    Requested,
    Confirmed,
    CheckedIn,
    InProgress,
    Completed,
    Cancelled,
    Missed
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;
    public string ReasonForVisit { get; set; } = "";
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public VisitRecord? VisitRecord { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
}
