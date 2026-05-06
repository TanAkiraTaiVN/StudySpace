using StudySpace.Core.DTOs.Chat;

namespace StudySpace.Core.Interfaces;

public interface IChatService
{
    Task<ChatMessageDto> SendAsync(int currentUserId, SendMessageRequest request);
    Task<List<ChatMessageDto>> ListAsync(int currentUserId, int groupId, int take = 50, int? beforeId = null);
    Task DeleteAsync(int currentUserId, int messageId);
}
