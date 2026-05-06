using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Dashboard;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> Summary()
        => Ok(ApiResponse<DashboardSummaryDto>.Ok(await _dashboard.GetSummaryAsync()));
}
