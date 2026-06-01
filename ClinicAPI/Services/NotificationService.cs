using ClinicAPI.Data;
using ClinicAPI.Models;

namespace ClinicAPI.Services;

public class NotificationService : INotificationService
{
    private readonly ClinicDbContext _db;

    public NotificationService(ClinicDbContext db) => _db = db;

    public async Task SendAsync(string userId, string title, string message,
        NotificationType type, int? relatedAppointmentId = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            RelatedAppointmentId = relatedAppointmentId
        });
        await _db.SaveChangesAsync();
    }

    public async Task SendToManyAsync(IEnumerable<string> userIds, string title, string message,
        NotificationType type, int? relatedAppointmentId = null)
    {
        foreach (var userId in userIds)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                RelatedAppointmentId = relatedAppointmentId
            });
        }
        await _db.SaveChangesAsync();
    }
}
