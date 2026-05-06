using System.ComponentModel.DataAnnotations;

namespace StudySpace.Core.DTOs.Group;

public class CreateGroupRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }
    public bool IsPublic { get; set; } = true;

    [Range(2, 500)]
    public int MaxMembers { get; set; } = 50;
}

public class UpdateGroupRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Subject { get; set; }

    public string? CoverImageUrl { get; set; }
    public bool? IsPublic { get; set; }
    public int? MaxMembers { get; set; }
}

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxMembers { get; set; }
    public int MemberCount { get; set; }
    public int DocumentCount { get; set; }
    public int ScheduleCount { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsMember { get; set; }
    public string? MyRole { get; set; }
}

public class GroupMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class JoinGroupRequest
{
    [Required]
    public string InviteCode { get; set; } = string.Empty;
}

public class UpdateMemberRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
