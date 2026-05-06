using Microsoft.EntityFrameworkCore;
using StudySpace.Core.DTOs.Document;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly StudySpaceDbContext _db;
    private readonly INotificationService _notifications;

    public DocumentService(StudySpaceDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<DocumentDto> UploadAsync(
        int currentUserId,
        int studyGroupId,
        string title,
        string? description,
        string? tags,
        string fileName,
        string contentType,
        long fileSize,
        Stream fileStream,
        string storageRootPath,
        string publicBaseUrl)
    {
        await EnsureMemberAsync(currentUserId, studyGroupId);

        if (fileSize <= 0) throw new InvalidOperationException("Tệp tải lên không hợp lệ.");
        const long maxSize = 50L * 1024 * 1024;
        if (fileSize > maxSize) throw new InvalidOperationException("Tệp vượt quá 50MB.");

        var safeOriginal = string.IsNullOrWhiteSpace(fileName) ? "document" : Path.GetFileName(fileName);
        var ext = Path.GetExtension(safeOriginal);
        var stored = $"{Guid.NewGuid():N}{ext}";

        var groupDir = Path.Combine(storageRootPath, "uploads", "documents", studyGroupId.ToString());
        Directory.CreateDirectory(groupDir);

        var fullPath = Path.Combine(groupDir, stored);
        await using (var dest = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(dest);
        }

        var doc = new Document
        {
            StudyGroupId = studyGroupId,
            UploadedById = currentUserId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Tags = tags?.Trim(),
            FileName = safeOriginal,
            StoredFileName = stored,
            FileUrl = $"{publicBaseUrl.TrimEnd('/')}/uploads/documents/{studyGroupId}/{stored}",
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = fileSize,
            CreatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        var group = await _db.StudyGroups.FindAsync(studyGroupId);
        await _notifications.PushToGroupAsync(
            studyGroupId,
            NotificationType.NewDocument,
            "Tài liệu mới",
            $"Có tài liệu mới trong nhóm {group?.Name}: {doc.Title}",
            $"/groups/{studyGroupId}/documents/{doc.Id}",
            excludeUserId: currentUserId);

        return await GetByIdAsync(currentUserId, doc.Id);
    }

    public async Task<DocumentDto> GetByIdAsync(int currentUserId, int documentId)
    {
        var doc = await _db.Documents
            .Include(d => d.StudyGroup)
            .Include(d => d.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        await EnsureMemberAsync(currentUserId, doc.StudyGroupId);
        return ToDto(doc);
    }

    public async Task<List<DocumentDto>> ListByGroupAsync(int currentUserId, int groupId, string? search = null)
    {
        await EnsureMemberAsync(currentUserId, groupId);

        var q = _db.Documents
            .Include(d => d.StudyGroup)
            .Include(d => d.UploadedBy)
            .Where(d => d.StudyGroupId == groupId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(d => d.Title.ToLower().Contains(s)
                          || (d.Description != null && d.Description.ToLower().Contains(s))
                          || (d.Tags != null && d.Tags.ToLower().Contains(s)));
        }

        var docs = await q.OrderByDescending(d => d.CreatedAt).Take(500).ToListAsync();
        return docs.Select(ToDto).ToList();
    }

    public async Task<DocumentDto> UpdateAsync(int currentUserId, int documentId, UpdateDocumentRequest request)
    {
        var doc = await _db.Documents
            .Include(d => d.StudyGroup)
            .Include(d => d.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        await EnsureCanModifyAsync(currentUserId, doc);

        if (!string.IsNullOrWhiteSpace(request.Title)) doc.Title = request.Title.Trim();
        if (request.Description != null) doc.Description = request.Description.Trim();
        if (request.Tags != null) doc.Tags = request.Tags.Trim();

        await _db.SaveChangesAsync();
        return ToDto(doc);
    }

    public async Task DeleteAsync(int currentUserId, int documentId, string storageRootPath)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        await EnsureCanModifyAsync(currentUserId, doc);

        var path = Path.Combine(storageRootPath, "uploads", "documents", doc.StudyGroupId.ToString(), doc.StoredFileName);
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { /* ignore */ }
        }

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
    }

    public async Task<(string FilePath, string FileName, string ContentType)> GetDownloadAsync(int currentUserId, int documentId, string storageRootPath)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        await EnsureMemberAsync(currentUserId, doc.StudyGroupId);

        var path = Path.Combine(storageRootPath, "uploads", "documents", doc.StudyGroupId.ToString(), doc.StoredFileName);
        if (!File.Exists(path))
            throw new FileNotFoundException("Tệp đã bị xoá khỏi máy chủ.");

        doc.DownloadCount += 1;
        await _db.SaveChangesAsync();

        return (path, doc.FileName, doc.ContentType);
    }

    private async Task EnsureMemberAsync(int currentUserId, int groupId)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;

        var member = await _db.GroupMembers.AnyAsync(m => m.StudyGroupId == groupId && m.UserId == currentUserId);
        if (!member) throw new UnauthorizedAccessException("Bạn không phải thành viên nhóm này.");
    }

    private async Task EnsureCanModifyAsync(int currentUserId, Document doc)
    {
        var user = await _db.Users.FindAsync(currentUserId);
        if (user?.Role == UserRole.Admin) return;
        if (doc.UploadedById == currentUserId) return;

        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.StudyGroupId == doc.StudyGroupId && m.UserId == currentUserId);
        if (membership?.Role == GroupMemberRole.Leader) return;

        throw new UnauthorizedAccessException("Bạn không có quyền sửa/xoá tài liệu này.");
    }

    private static DocumentDto ToDto(Document d) => new()
    {
        Id = d.Id,
        StudyGroupId = d.StudyGroupId,
        GroupName = d.StudyGroup?.Name ?? string.Empty,
        Title = d.Title,
        Description = d.Description,
        FileName = d.FileName,
        FileUrl = d.FileUrl,
        ContentType = d.ContentType,
        FileSize = d.FileSize,
        Tags = d.Tags,
        DownloadCount = d.DownloadCount,
        UploadedById = d.UploadedById,
        UploadedByName = d.UploadedBy?.FullName ?? string.Empty,
        CreatedAt = d.CreatedAt
    };
}
