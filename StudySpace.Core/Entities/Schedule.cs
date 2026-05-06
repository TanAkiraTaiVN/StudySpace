using StudySpace.Core.Enums;

namespace StudySpace.Core.Entities;

public class Schedule
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public StudyGroup StudyGroup { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Upcoming;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
