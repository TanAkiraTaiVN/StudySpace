namespace StudySpace.Core.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int TotalGroups { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalSchedules { get; set; }
    public int UpcomingSchedules { get; set; }
    public int MessagesLast7Days { get; set; }
    public int NewUsersLast7Days { get; set; }
    public List<GroupActivityDto> TopGroups { get; set; } = new();
    public List<DailyActivityDto> ActivityLast7Days { get; set; } = new();
}

public class GroupActivityDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int DocumentCount { get; set; }
    public int MessageCount { get; set; }
}

public class DailyActivityDto
{
    public DateTime Date { get; set; }
    public int NewMessages { get; set; }
    public int NewDocuments { get; set; }
    public int NewMembers { get; set; }
}
