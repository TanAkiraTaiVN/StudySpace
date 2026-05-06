using System.ComponentModel.DataAnnotations;

namespace StudySpace.Core.DTOs.Schedule;

public class CreateScheduleRequest
{
    [Required]
    public int StudyGroupId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? MeetingUrl { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}

public class UpdateScheduleRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? MeetingUrl { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Status { get; set; }
}

public class ScheduleDto
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}
