using System.ComponentModel.DataAnnotations;

namespace StudySpace.Core.DTOs.Progress;

public class CreateProgressRequest
{
    [Required]
    public int StudyGroupId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string Status { get; set; } = "NotStarted";

    [Range(0, 100)]
    public int CompletionPercent { get; set; }
}

public class UpdateProgressRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? Status { get; set; }

    [Range(0, 100)]
    public int? CompletionPercent { get; set; }
}

public class ProgressDto
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CompletionPercent { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GroupProgressSummaryDto
{
    public int StudyGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TotalEntries { get; set; }
    public int CompletedEntries { get; set; }
    public int InProgressEntries { get; set; }
    public int NotStartedEntries { get; set; }
    public double AverageCompletion { get; set; }
}
