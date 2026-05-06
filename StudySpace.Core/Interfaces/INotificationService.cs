using StudySpace.Core.DTOs.Notification;
using StudySpace.Core.Enums;

namespace StudySpace.Core.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> ListAsync(int userId, bool unreadOnly = false, int take = 50);
    Task<int> CountUnreadAsync(int userId);
    Task MarkReadAsync(int userId, int notificationId);
    Task MarkAllReadAsync(int userId);
    Task DeleteAsync(int userId, int notificationId);
    Task PushAsync(int userId, NotificationType type, string title, string message, string? link = null);
    Task PushToGroupAsync(int groupId, NotificationType type, string title, string message, string? link = null, int? excludeUserId = null);
}
