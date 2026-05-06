namespace StudySpace.Core.Entities;

public class Document
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public StudyGroup StudyGroup { get; set; } = null!;
    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Tags { get; set; }
    public int DownloadCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
