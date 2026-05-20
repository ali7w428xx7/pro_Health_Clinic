namespace ClinicAPI.Models;

public enum NotificationType
{
    AppointmentBooked,
    AppointmentConfirmed,
    AppointmentCancelled,
    AppointmentReminder,
    AppointmentStatusChanged,
    VisitRecordCreated,
    PrescriptionAdded,
    LeaveApproved,
    General
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType NotificationType { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? RelatedAppointmentId { get; set; }
    public Appointment? RelatedAppointment { get; set; }
}
