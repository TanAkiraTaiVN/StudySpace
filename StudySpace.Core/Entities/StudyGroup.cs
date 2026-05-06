namespace StudySpace.Core.Entities;

public class StudyGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public int MaxMembers { get; set; } = 50;
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
}
