using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Notification;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly StudySpaceDbContext _db;

    public NotificationService(StudySpaceDbContext db)
    {
        _db = db;
    }

    public async Task<List<NotificationDto>> ListAsync(int userId, bool unreadOnly = false, int take = 50)
    {
        var q = _db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        var list = await q.OrderByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public Task<int> CountUnreadAsync(int userId)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkReadAsync(int userId, int notificationId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
        n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId, int notificationId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
        _db.Notifications.Remove(n);
        await _db.SaveChangesAsync();
    }

    public async Task PushAsync(int userId, NotificationType type, string title, string message, string? link = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        await _db.SaveChangesAsync();
    }

    public async Task PushToGroupAsync(int groupId, NotificationType type, string title, string message, string? link = null, int? excludeUserId = null)
    {
        var memberIds = await _db.GroupMembers
            .Where(m => m.StudyGroupId == groupId && (excludeUserId == null || m.UserId != excludeUserId))
            .Select(m => m.UserId)
            .ToListAsync();

        var notifs = memberIds.Select(uid => new Notification
        {
            UserId = uid,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        }).ToList();

        if (notifs.Count > 0)
        {
            _db.Notifications.AddRange(notifs);
            await _db.SaveChangesAsync();
        }
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type.ToString(),
        Title = n.Title,
        Message = n.Message,
        Link = n.Link,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
