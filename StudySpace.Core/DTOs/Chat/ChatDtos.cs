using System.ComponentModel.DataAnnotations;

namespace StudySpace.Core.DTOs.Chat;

public class SendMessageRequest
{
    [Required]
    public int StudyGroupId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string? AttachmentUrl { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; }
}
