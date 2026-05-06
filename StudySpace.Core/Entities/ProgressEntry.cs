using StudySpace.Core.Enums;

namespace StudySpace.Core.Entities;

public class ProgressEntry
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public StudyGroup StudyGroup { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;
    public int CompletionPercent { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
