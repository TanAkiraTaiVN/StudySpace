using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Schedule;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly StudySpaceDbContext _db;
    private readonly INotificationService _notifications;

    public ScheduleService(StudySpaceDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ScheduleDto> CreateAsync(int currentUserId, CreateScheduleRequest request)
    {
        await EnsureLeaderOrAdminAsync(currentUserId, request.StudyGroupId);

        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException("Thời gian kết thúc phải sau thời gian bắt đầu.");

        var s = new Schedule
        {
            StudyGroupId = request.StudyGroupId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Location = request.Location?.Trim(),
            MeetingUrl = request.MeetingUrl?.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = ScheduleStatus.Upcoming,
            CreatedById = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Schedules.Add(s);
        await _db.SaveChangesAsync();

        var group = await _db.StudyGroups.FindAsync(request.StudyGroupId);
        await _notifications.PushToGroupAsync(
            request.StudyGroupId,
            NotificationType.ScheduleReminder,
            "Lịch học mới",
            $"Lịch mới trong nhóm {group?.Name}: {s.Title} ({s.StartTime:dd/MM/yyyy HH:mm})",
            $"/groups/{request.StudyGroupId}/schedules/{s.Id}",
            excludeUserId: currentUserId);

        return await GetByIdAsync(currentUserId, s.Id);
    }

    public async Task<ScheduleDto> UpdateAsync(int currentUserId, int scheduleId, UpdateScheduleRequest request)
    {
        var s = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == scheduleId)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch học.");

        await EnsureLeaderOrAdminAsync(currentUserId, s.StudyGroupId);

        if (!string.IsNullOrWhiteSpace(request.Title)) s.Title = request.Title.Trim();
        if (request.Description != null) s.Description = request.Description.Trim();
        if (request.Location != null) s.Location = request.Location.Trim();
        if (request.MeetingUrl != null) s.MeetingUrl = request.MeetingUrl.Trim();
        if (request.StartTime.HasValue) s.StartTime = request.StartTime.Value;
        if (request.EndTime.HasValue) s.EndTime = request.EndTime.Value;
        if (s.EndTime <= s.StartTime) throw new InvalidOperationException("Thời gian kết thúc phải sau bắt đầu.");

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ScheduleStatus>(request.Status, true, out var st))
        {
            s.Status = st;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(currentUserId, s.Id);
    }

    public async Task DeleteAsync(int currentUserId, int scheduleId)
    {
        var s = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == scheduleId)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch học.");
        await EnsureLeaderOrAdminAsync(currentUserId, s.StudyGroupId);
        _db.Schedules.Remove(s);
        await _db.SaveChangesAsync();
    }

    public async Task<ScheduleDto> GetByIdAsync(int currentUserId, int scheduleId)
    {
        var s = await _db.Schedules
            .Include(x => x.StudyGroup)
            .FirstOrDefaultAsync(x => x.Id == scheduleId)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch học.");
        await EnsureMemberAsync(currentUserId, s.StudyGroupId);
        return ToDto(s);
    }

    public async Task<List<ScheduleDto>> ListByGroupAsync(int currentUserId, int groupId)
    {
        await EnsureMemberAsync(currentUserId, groupId);
        var list = await _db.Schedules.Include(x => x.StudyGroup)
            .Where(x => x.StudyGroupId == groupId)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<List<ScheduleDto>> MyUpcomingAsync(int currentUserId, int days = 7)
    {
        var until = DateTime.UtcNow.AddDays(days);
        var groupIds = await _db.GroupMembers
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.StudyGroupId)
            .ToListAsync();

        var list = await _db.Schedules.Include(x => x.StudyGroup)
            .Where(x => groupIds.Contains(x.StudyGroupId)
                     && x.StartTime >= DateTime.UtcNow
                     && x.StartTime <= until
                     && x.Status != ScheduleStatus.Cancelled)
            .OrderBy(x => x.StartTime)
            .ToListAsync();

        return list.Select(ToDto).ToList();
    }

    private async Task EnsureMemberAsync(int currentUserId, int groupId)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;
        var ok = await _db.GroupMembers.AnyAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId);
        if (!ok) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm này.");
    }

    private async Task EnsureLeaderOrAdminAsync(int currentUserId, int groupId)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;
        var m = await _db.GroupMembers.FirstOrDefaultAsync(x => x.StudyGroupId == groupId && x.UserId == currentUserId);
        if (m == null) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm.");
        if (m.Role != GroupMemberRole.Leader) throw new UnauthorizedAccessException("Chỉ trưởng nhóm mới được phép.");
    }

    private static ScheduleDto ToDto(Schedule s) => new()
    {
        Id = s.Id,
        StudyGroupId = s.StudyGroupId,
        GroupName = s.StudyGroup?.Name ?? string.Empty,
        Title = s.Title,
        Description = s.Description,
        Location = s.Location,
        MeetingUrl = s.MeetingUrl,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Status = s.Status.ToString(),
        CreatedById = s.CreatedById,
        CreatedAt = s.CreatedAt
    };
}
