using StudySpace.Core.DTOs.Progress;

namespace StudySpace.Core.Interfaces;

public interface IProgressService
{
    Task<ProgressDto> CreateAsync(int currentUserId, CreateProgressRequest request);
    Task<ProgressDto> UpdateAsync(int currentUserId, int progressId, UpdateProgressRequest request);
    Task DeleteAsync(int currentUserId, int progressId);
    Task<ProgressDto> GetByIdAsync(int currentUserId, int progressId);
    Task<List<ProgressDto>> ListMineAsync(int currentUserId, int? groupId = null);
    Task<List<ProgressDto>> ListByGroupAsync(int currentUserId, int groupId);
    Task<GroupProgressSummaryDto> GroupSummaryAsync(int currentUserId, int groupId);
}
