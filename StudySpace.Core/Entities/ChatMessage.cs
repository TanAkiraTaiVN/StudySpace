namespace StudySpace.Core.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public StudyGroup StudyGroup { get; set; } = null!;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
