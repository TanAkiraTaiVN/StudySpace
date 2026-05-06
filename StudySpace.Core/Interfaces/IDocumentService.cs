using StudySpace.Core.DTOs.Document;

namespace StudySpace.Core.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(
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
        string publicBaseUrl);

    Task<DocumentDto> GetByIdAsync(int currentUserId, int documentId);
    Task<List<DocumentDto>> ListByGroupAsync(int currentUserId, int groupId, string? search = null);
    Task<DocumentDto> UpdateAsync(int currentUserId, int documentId, UpdateDocumentRequest request);
    Task DeleteAsync(int currentUserId, int documentId, string storageRootPath);
    Task<(string FilePath, string FileName, string ContentType)> GetDownloadAsync(int currentUserId, int documentId, string storageRootPath);
}
