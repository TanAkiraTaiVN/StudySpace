using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Dashboard;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly StudySpaceDbContext _db;

    public DashboardService(StudySpaceDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);

        var summary = new DashboardSummaryDto
        {
            TotalUsers = await _db.Users.CountAsync(),
            TotalGroups = await _db.StudyGroups.CountAsync(),
            TotalDocuments = await _db.Documents.CountAsync(),
            TotalSchedules = await _db.Schedules.CountAsync(),
            UpcomingSchedules = await _db.Schedules.CountAsync(s => s.StartTime >= now && s.Status != ScheduleStatus.Cancelled),
            MessagesLast7Days = await _db.ChatMessages.CountAsync(m => m.SentAt >= weekAgo && !m.IsDeleted),
            NewUsersLast7Days = await _db.Users.CountAsync(u => u.CreatedAt >= weekAgo)
        };

        var topGroups = await _db.StudyGroups
            .Select(g => new GroupActivityDto
            {
                GroupId = g.Id,
                GroupName = g.Name,
                MemberCount = g.Members.Count,
                DocumentCount = g.Documents.Count,
                MessageCount = g.Messages.Count(m => !m.IsDeleted)
            })
            .OrderByDescending(g => g.MessageCount + g.DocumentCount + g.MemberCount)
            .Take(5)
            .ToListAsync();
        summary.TopGroups = topGroups;

        var messages = await _db.ChatMessages
            .Where(m => m.SentAt >= weekAgo && !m.IsDeleted)
            .Select(m => m.SentAt.Date)
            .ToListAsync();
        var docs = await _db.Documents
            .Where(d => d.CreatedAt >= weekAgo)
            .Select(d => d.CreatedAt.Date)
            .ToListAsync();
        var newMembers = await _db.GroupMembers
            .Where(m => m.JoinedAt >= weekAgo)
            .Select(m => m.JoinedAt.Date)
            .ToListAsync();

        var activity = new List<DailyActivityDto>();
        for (int i = 6; i >= 0; i--)
        {
            var d = now.Date.AddDays(-i);
            activity.Add(new DailyActivityDto
            {
                Date = d,
                NewMessages = messages.Count(x => x == d),
                NewDocuments = docs.Count(x => x == d),
                NewMembers = newMembers.Count(x => x == d)
            });
        }
        summary.ActivityLast7Days = activity;

        return summary;
    }
}
