using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Chat;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly StudySpaceDbContext _db;

    public ChatService(StudySpaceDbContext db)
    {
        _db = db;
    }

    public async Task<ChatMessageDto> SendAsync(int currentUserId, SendMessageRequest request)
    {
        await EnsureMemberAsync(currentUserId, request.StudyGroupId);

        var msg = new ChatMessage
        {
            StudyGroupId = request.StudyGroupId,
            SenderId = currentUserId,
            Content = request.Content.Trim(),
            AttachmentUrl = request.AttachmentUrl,
            SentAt = DateTime.UtcNow
        };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        return await BuildDtoAsync(msg.Id);
    }

    public async Task<List<ChatMessageDto>> ListAsync(int currentUserId, int groupId, int take = 50, int? beforeId = null)
    {
        await EnsureMemberAsync(currentUserId, groupId);

        var q = _db.ChatMessages.Include(m => m.Sender)
            .Where(m => m.StudyGroupId == groupId && !m.IsDeleted);
        if (beforeId.HasValue) q = q.Where(m => m.Id < beforeId.Value);

        var msgs = await q.OrderByDescending(m => m.Id)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync();

        msgs.Reverse();

        return msgs.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            StudyGroupId = m.StudyGroupId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName ?? string.Empty,
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            Content = m.Content,
            AttachmentUrl = m.AttachmentUrl,
            SentAt = m.SentAt
        }).ToList();
    }

    public async Task DeleteAsync(int currentUserId, int messageId)
    {
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Không tìm thấy tin nhắn.");

        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role != UserRole.Admin && msg.SenderId != currentUserId)
        {
            var membership = await _db.GroupMembers
                .FirstOrDefaultAsync(m => m.StudyGroupId == msg.StudyGroupId && m.UserId == currentUserId);
            if (membership?.Role != GroupMemberRole.Leader)
                throw new UnauthorizedAccessException("Bạn không có quyền xoá tin nhắn này.");
        }

        msg.IsDeleted = true;
        await _db.SaveChangesAsync();
    }

    private async Task EnsureMemberAsync(int currentUserId, int groupId)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;
        var ok = await _db.GroupMembers.AnyAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId);
        if (!ok) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm này.");
    }

    private async Task<ChatMessageDto> BuildDtoAsync(int messageId)
    {
        var m = await _db.ChatMessages.Include(x => x.Sender).FirstAsync(x => x.Id == messageId);
        return new ChatMessageDto
        {
            Id = m.Id,
            StudyGroupId = m.StudyGroupId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName ?? string.Empty,
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            Content = m.Content,
            AttachmentUrl = m.AttachmentUrl,
            SentAt = m.SentAt
        };
    }
}
