using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Progress;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly StudySpaceDbContext _db;

    public ProgressService(StudySpaceDbContext db)
    {
        _db = db;
    }

    public async Task<ProgressDto> CreateAsync(int currentUserId, CreateProgressRequest request)
    {
        await EnsureMemberAsync(currentUserId, request.StudyGroupId);

        if (!Enum.TryParse<ProgressStatus>(request.Status, true, out var status))
            status = ProgressStatus.NotStarted;

        var entry = new ProgressEntry
        {
            StudyGroupId = request.StudyGroupId,
            UserId = currentUserId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = status,
            CompletionPercent = Math.Clamp(request.CompletionPercent, 0, 100),
            CompletedAt = status == ProgressStatus.Completed ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };
        _db.ProgressEntries.Add(entry);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(currentUserId, entry.Id);
    }

    public async Task<ProgressDto> UpdateAsync(int currentUserId, int progressId, UpdateProgressRequest request)
    {
        var entry = await _db.ProgressEntries.FirstOrDefaultAsync(x => x.Id == progressId)
            ?? throw new KeyNotFoundException("Không tìm thấy mục tiến độ.");

        if (entry.UserId != currentUserId)
        {
            var u = await _db.Users.FindAsync(currentUserId);
            if (u?.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Bạn chỉ được sửa tiến độ của chính mình.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title)) entry.Title = request.Title.Trim();
        if (request.Description != null) entry.Description = request.Description.Trim();
        if (request.CompletionPercent.HasValue) entry.CompletionPercent = Math.Clamp(request.CompletionPercent.Value, 0, 100);
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ProgressStatus>(request.Status, true, out var st))
        {
            entry.Status = st;
            entry.CompletedAt = st == ProgressStatus.Completed ? DateTime.UtcNow : null;
        }
        else if (entry.CompletionPercent == 100)
        {
            entry.Status = ProgressStatus.Completed;
            entry.CompletedAt ??= DateTime.UtcNow;
        }

        entry.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(currentUserId, entry.Id);
    }

    public async Task DeleteAsync(int currentUserId, int progressId)
    {
        var entry = await _db.ProgressEntries.FirstOrDefaultAsync(x => x.Id == progressId)
            ?? throw new KeyNotFoundException("Không tìm thấy mục tiến độ.");

        if (entry.UserId != currentUserId)
        {
            var u = await _db.Users.FindAsync(currentUserId);
            if (u?.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Không có quyền xoá.");
        }

        _db.ProgressEntries.Remove(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<ProgressDto> GetByIdAsync(int currentUserId, int progressId)
    {
        var entry = await _db.ProgressEntries
            .Include(x => x.StudyGroup)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == progressId)
            ?? throw new KeyNotFoundException("Không tìm thấy mục tiến độ.");

        await EnsureMemberAsync(currentUserId, entry.StudyGroupId);
        return ToDto(entry);
    }

    public async Task<List<ProgressDto>> ListMineAsync(int currentUserId, int? groupId = null)
    {
        var q = _db.ProgressEntries.Include(x => x.StudyGroup).Include(x => x.User)
            .Where(x => x.UserId == currentUserId);
        if (groupId.HasValue) q = q.Where(x => x.StudyGroupId == groupId.Value);
        var list = await q.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<List<ProgressDto>> ListByGroupAsync(int currentUserId, int groupId)
    {
        await EnsureMemberAsync(currentUserId, groupId);
        var list = await _db.ProgressEntries.Include(x => x.User).Include(x => x.StudyGroup)
            .Where(x => x.StudyGroupId == groupId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<GroupProgressSummaryDto> GroupSummaryAsync(int currentUserId, int groupId)
    {
        await EnsureMemberAsync(currentUserId, groupId);

        var group = await _db.StudyGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Không tìm thấy nhóm.");

        var entries = await _db.ProgressEntries.Where(x => x.StudyGroupId == groupId).ToListAsync();

        return new GroupProgressSummaryDto
        {
            StudyGroupId = groupId,
            GroupName = group.Name,
            TotalEntries = entries.Count,
            CompletedEntries = entries.Count(e => e.Status == ProgressStatus.Completed),
            InProgressEntries = entries.Count(e => e.Status == ProgressStatus.InProgress),
            NotStartedEntries = entries.Count(e => e.Status == ProgressStatus.NotStarted),
            AverageCompletion = entries.Count == 0 ? 0 : Math.Round(entries.Average(e => (double)e.CompletionPercent), 2)
        };
    }

    private async Task EnsureMemberAsync(int currentUserId, int groupId)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;
        var ok = await _db.GroupMembers.AnyAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId);
        if (!ok) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm này.");
    }

    private static ProgressDto ToDto(ProgressEntry e) => new()
    {
        Id = e.Id,
        StudyGroupId = e.StudyGroupId,
        GroupName = e.StudyGroup?.Name ?? string.Empty,
        UserId = e.UserId,
        UserName = e.User?.FullName ?? string.Empty,
        UserAvatarUrl = e.User?.AvatarUrl,
        Title = e.Title,
        Description = e.Description,
        Status = e.Status.ToString(),
        CompletionPercent = e.CompletionPercent,
        CompletedAt = e.CompletedAt,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
