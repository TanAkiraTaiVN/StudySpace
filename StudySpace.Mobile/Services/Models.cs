using System.Text.Json.Serialization;

namespace StudySpace.Mobile.Services;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public DateTime ExpiresAt { get; set; }
}

public class GroupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    public int DocumentCount { get; set; }
    public int ScheduleCount { get; set; }
    public bool IsMember { get; set; }
    public string? MyRole { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class JoinGroupRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

public class ScheduleModel
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
    public string Status { get; set; } = "Upcoming";

    [JsonIgnore] public string StartLocal => StartTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    [JsonIgnore] public string EndLocal => EndTime.ToLocalTime().ToString("HH:mm");
}

public class DocumentModel
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int DownloadCount { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    [JsonIgnore] public string SizeText => FileSize switch
    {
        < 1024 => $"{FileSize} B",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        _ => $"{FileSize / 1024.0 / 1024.0:F1} MB"
    };
}

public class NotificationModel
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore] public string CreatedAtText => CreatedAt.ToLocalTime().ToString("dd/MM HH:mm");
}

public class ChatMessageModel
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    [JsonIgnore] public string SentAtText => SentAt.ToLocalTime().ToString("HH:mm");
}

public class SendMessageRequest
{
    public int StudyGroupId { get; set; }
    public string Content { get; set; } = string.Empty;
}
