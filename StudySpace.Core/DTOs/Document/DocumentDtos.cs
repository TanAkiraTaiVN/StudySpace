using System.ComponentModel.DataAnnotations;

namespace StudySpace.Core.DTOs.Document;

public class DocumentDto
{
    public int Id { get; set; }
    public int StudyGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Tags { get; set; }
    public int DownloadCount { get; set; }
    public int UploadedById { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateDocumentRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Tags { get; set; }
}
