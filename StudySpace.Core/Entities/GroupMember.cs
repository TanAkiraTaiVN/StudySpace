using StudySpace.Core.Enums;

namespace StudySpace.Core.Entities;

public class GroupMember
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public StudyGroup StudyGroup { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
