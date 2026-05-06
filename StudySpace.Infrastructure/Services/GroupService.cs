using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Group;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly StudySpaceDbContext _db;
    private readonly INotificationService _notifications;

    public GroupService(StudySpaceDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<GroupDto> CreateAsync(int currentUserId, CreateGroupRequest request)
    {
        var user = await _db.Users.FindAsync(currentUserId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var inviteCode = await GenerateUniqueInviteCodeAsync();

        var group = new StudyGroup
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Subject = request.Subject.Trim(),
            CoverImageUrl = request.CoverImageUrl,
            InviteCode = inviteCode,
            IsPublic = request.IsPublic,
            MaxMembers = request.MaxMembers,
            CreatedById = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.StudyGroups.Add(group);
        await _db.SaveChangesAsync();

        _db.GroupMembers.Add(new GroupMember
        {
            StudyGroupId = group.Id,
            UserId = currentUserId,
            Role = GroupMemberRole.Leader,
            JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return await GetByIdAsync(currentUserId, group.Id);
    }

    public async Task<GroupDto> UpdateAsync(int currentUserId, int groupId, UpdateGroupRequest request)
    {
        var group = await _db.StudyGroups.FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Không tìm thấy nhóm học.");

        await EnsureLeaderOrAdminAsync(currentUserId, groupId);

        if (!string.IsNullOrWhiteSpace(request.Name)) group.Name = request.Name.Trim();
        if (request.Description != null) group.Description = request.Description.Trim();
        if (!string.IsNullOrWhiteSpace(request.Subject)) group.Subject = request.Subject.Trim();
        if (request.CoverImageUrl != null) group.CoverImageUrl = request.CoverImageUrl;
        if (request.IsPublic.HasValue) group.IsPublic = request.IsPublic.Value;
        if (request.MaxMembers.HasValue && request.MaxMembers.Value >= 2) group.MaxMembers = request.MaxMembers.Value;
        group.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(currentUserId, group.Id);
    }

    public async Task DeleteAsync(int currentUserId, int groupId)
    {
        var group = await _db.StudyGroups.FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Không tìm thấy nhóm học.");

        await EnsureLeaderOrAdminAsync(currentUserId, groupId, allowOwner: true);

        _db.StudyGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<GroupDto> GetByIdAsync(int? currentUserId, int groupId)
    {
        var group = await _db.StudyGroups
            .Include(g => g.CreatedBy)
            .FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Không tìm thấy nhóm học.");

        var memberCount = await _db.GroupMembers.CountAsync(m => m.StudyGroupId == groupId);
        var docCount = await _db.Documents.CountAsync(d => d.StudyGroupId == groupId);
        var schedCount = await _db.Schedules.CountAsync(s => s.StudyGroupId == groupId);

        GroupMember? myMembership = null;
        if (currentUserId.HasValue)
        {
            myMembership = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId.Value);
        }

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Subject = group.Subject,
            CoverImageUrl = group.CoverImageUrl,
            InviteCode = group.InviteCode,
            IsPublic = group.IsPublic,
            MaxMembers = group.MaxMembers,
            MemberCount = memberCount,
            DocumentCount = docCount,
            ScheduleCount = schedCount,
            CreatedById = group.CreatedById,
            CreatedByName = group.CreatedBy?.FullName ?? string.Empty,
            CreatedAt = group.CreatedAt,
            IsMember = myMembership != null,
            MyRole = myMembership?.Role.ToString()
        };
    }

    public async Task<List<GroupDto>> ListAsync(int? currentUserId, string? search = null, bool? joinedOnly = null)
    {
        var q = _db.StudyGroups.Include(g => g.CreatedBy).AsQueryable();

        if (currentUserId.HasValue && joinedOnly == true)
        {
            var joinedIds = _db.GroupMembers
                .Where(m => m.UserId == currentUserId.Value)
                .Select(m => m.StudyGroupId);
            q = q.Where(g => joinedIds.Contains(g.Id));
        }
        else if (!currentUserId.HasValue || joinedOnly != true)
        {
            q = q.Where(g => g.IsPublic || (currentUserId.HasValue &&
                _db.GroupMembers.Any(m => m.StudyGroupId == g.Id && m.UserId == currentUserId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(g => g.Name.ToLower().Contains(s)
                          || g.Subject.ToLower().Contains(s)
                          || g.Description.ToLower().Contains(s));
        }

        var groups = await q.OrderByDescending(g => g.CreatedAt).Take(200).ToListAsync();

        var ids = groups.Select(g => g.Id).ToList();
        var memberCounts = await _db.GroupMembers.Where(m => ids.Contains(m.StudyGroupId))
            .GroupBy(m => m.StudyGroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count);

        var docCounts = await _db.Documents.Where(d => ids.Contains(d.StudyGroupId))
            .GroupBy(d => d.StudyGroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count);

        var schedCounts = await _db.Schedules.Where(s => ids.Contains(s.StudyGroupId))
            .GroupBy(s => s.StudyGroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count);

        Dictionary<int, GroupMember> myMemberships = new();
        if (currentUserId.HasValue)
        {
            myMemberships = await _db.GroupMembers
                .Where(m => m.UserId == currentUserId.Value && ids.Contains(m.StudyGroupId))
                .ToDictionaryAsync(m => m.StudyGroupId);
        }

        return groups.Select(g => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            Subject = g.Subject,
            CoverImageUrl = g.CoverImageUrl,
            InviteCode = g.InviteCode,
            IsPublic = g.IsPublic,
            MaxMembers = g.MaxMembers,
            MemberCount = memberCounts.GetValueOrDefault(g.Id, 0),
            DocumentCount = docCounts.GetValueOrDefault(g.Id, 0),
            ScheduleCount = schedCounts.GetValueOrDefault(g.Id, 0),
            CreatedById = g.CreatedById,
            CreatedByName = g.CreatedBy?.FullName ?? string.Empty,
            CreatedAt = g.CreatedAt,
            IsMember = myMemberships.ContainsKey(g.Id),
            MyRole = myMemberships.TryGetValue(g.Id, out var mm) ? mm.Role.ToString() : null
        }).ToList();
    }

    public Task<List<GroupDto>> MyGroupsAsync(int currentUserId)
        => ListAsync(currentUserId, null, true);

    public async Task<GroupMemberDto> JoinAsync(int currentUserId, JoinGroupRequest request)
    {
        var group = await _db.StudyGroups.FirstOrDefaultAsync(g => g.InviteCode == request.InviteCode.Trim().ToUpper())
            ?? throw new KeyNotFoundException("Mã mời không hợp lệ.");

        var exists = await _db.GroupMembers.AnyAsync(m => m.StudyGroupId == group.Id && m.UserId == currentUserId);
        if (exists) throw new InvalidOperationException("Bạn đã ở trong nhóm này.");

        var memberCount = await _db.GroupMembers.CountAsync(m => m.StudyGroupId == group.Id);
        if (memberCount >= group.MaxMembers)
            throw new InvalidOperationException("Nhóm đã đầy thành viên.");

        var membership = new GroupMember
        {
            StudyGroupId = group.Id,
            UserId = currentUserId,
            Role = GroupMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        };
        _db.GroupMembers.Add(membership);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(currentUserId);
        await _notifications.PushToGroupAsync(
            group.Id,
            NotificationType.GroupJoinRequest,
            "Thành viên mới",
            $"{user?.FullName} đã tham gia nhóm {group.Name}.",
            $"/groups/{group.Id}",
            excludeUserId: currentUserId);

        return await BuildMemberDtoAsync(membership.Id);
    }

    public async Task LeaveAsync(int currentUserId, int groupId)
    {
        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId)
            ?? throw new KeyNotFoundException("Bạn không ở trong nhóm này.");

        var group = await _db.StudyGroups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group != null && group.CreatedById == currentUserId)
            throw new InvalidOperationException("Người tạo nhóm không thể rời nhóm. Hãy chuyển quyền hoặc xoá nhóm.");

        _db.GroupMembers.Remove(membership);
        await _db.SaveChangesAsync();
    }

    public async Task<List<GroupMemberDto>> ListMembersAsync(int? currentUserId, int groupId)
    {
        var members = await _db.GroupMembers
            .Include(m => m.User)
            .Where(m => m.StudyGroupId == groupId)
            .OrderBy(m => m.Role == GroupMemberRole.Leader ? 0 : 1)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync();

        return members.Select(m => new GroupMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FullName = m.User.FullName,
            Email = m.User.Email,
            AvatarUrl = m.User.AvatarUrl,
            Role = m.Role.ToString(),
            JoinedAt = m.JoinedAt
        }).ToList();
    }

    public async Task<GroupMemberDto> UpdateMemberRoleAsync(int currentUserId, int groupId, int targetUserId, UpdateMemberRoleRequest request)
    {
        await EnsureLeaderOrAdminAsync(currentUserId, groupId);

        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.StudyGroupId == groupId && m.UserId == targetUserId)
            ?? throw new KeyNotFoundException("Không tìm thấy thành viên.");

        if (!Enum.TryParse<GroupMemberRole>(request.Role, true, out var parsed))
            throw new InvalidOperationException("Vai trò không hợp lệ.");

        membership.Role = parsed;
        await _db.SaveChangesAsync();
        return await BuildMemberDtoAsync(membership.Id);
    }

    public async Task RemoveMemberAsync(int currentUserId, int groupId, int targetUserId)
    {
        await EnsureLeaderOrAdminAsync(currentUserId, groupId);
        if (currentUserId == targetUserId)
            throw new InvalidOperationException("Không thể tự xoá chính mình. Hãy dùng chức năng rời nhóm.");

        var group = await _db.StudyGroups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group != null && group.CreatedById == targetUserId)
            throw new InvalidOperationException("Không thể xoá người tạo nhóm.");

        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.StudyGroupId == groupId && m.UserId == targetUserId)
            ?? throw new KeyNotFoundException("Không tìm thấy thành viên.");

        _db.GroupMembers.Remove(membership);
        await _db.SaveChangesAsync();
    }

    private async Task<GroupMemberDto> BuildMemberDtoAsync(int membershipId)
    {
        var m = await _db.GroupMembers.Include(x => x.User)
            .FirstAsync(x => x.Id == membershipId);
        return new GroupMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FullName = m.User.FullName,
            Email = m.User.Email,
            AvatarUrl = m.User.AvatarUrl,
            Role = m.Role.ToString(),
            JoinedAt = m.JoinedAt
        };
    }

    private async Task EnsureLeaderOrAdminAsync(int currentUserId, int groupId, bool allowOwner = false)
    {
        var user = await _db.Users.FindAsync(currentUserId)
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

        if (user.Role == UserRole.Admin) return;

        var membership = await _db.GroupMembers.FirstOrDefaultAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId);
        if (membership == null) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm.");

        if (membership.Role != GroupMemberRole.Leader)
        {
            if (allowOwner)
            {
                var group = await _db.StudyGroups.FindAsync(groupId);
                if (group != null && group.CreatedById == currentUserId) return;
            }
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
        }
    }

    private async Task<string> GenerateUniqueInviteCodeAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = new Random();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = new string(Enumerable.Range(0, 8).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
            if (!await _db.StudyGroups.AnyAsync(g => g.InviteCode == code))
                return code;
        }
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
