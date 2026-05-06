using StudySpace.Core.DTOs.Dashboard;

namespace StudySpace.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
