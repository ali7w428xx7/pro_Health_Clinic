using ClinicAPI.Models;

namespace ClinicAPI.Services;

public interface INotificationService
{
    Task SendAsync(string userId, string title, string message,
        NotificationType type, int? relatedAppointmentId = null);
    Task SendToManyAsync(IEnumerable<string> userIds, string title, string message,
        NotificationType type, int? relatedAppointmentId = null);
}
