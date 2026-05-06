using StudySpace.Core.DTOs.Schedule;

namespace StudySpace.Core.Interfaces;

public interface IScheduleService
{
    Task<ScheduleDto> CreateAsync(int currentUserId, CreateScheduleRequest request);
    Task<ScheduleDto> UpdateAsync(int currentUserId, int scheduleId, UpdateScheduleRequest request);
    Task DeleteAsync(int currentUserId, int scheduleId);
    Task<ScheduleDto> GetByIdAsync(int currentUserId, int scheduleId);
    Task<List<ScheduleDto>> ListByGroupAsync(int currentUserId, int groupId);
    Task<List<ScheduleDto>> MyUpcomingAsync(int currentUserId, int days = 7);
}
