using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Schedule;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _schedules;

    public SchedulesController(IScheduleService schedules)
    {
        _schedules = schedules;
    }

    [HttpGet("by-group/{groupId}")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> ListByGroup(int groupId)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<List<ScheduleDto>>.Ok(await _schedules.ListByGroupAsync(uid, groupId)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<List<ScheduleDto>>.Fail(ex.Message)); }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> Upcoming([FromQuery] int days = 7)
    {
        var uid = CurrentUser.RequireUserId(User);
        return Ok(ApiResponse<List<ScheduleDto>>.Ok(await _schedules.MyUpcomingAsync(uid, days)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Get(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ScheduleDto>.Ok(await _schedules.GetByIdAsync(uid, id)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ScheduleDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ScheduleDto>.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Create([FromBody] CreateScheduleRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ScheduleDto>.Ok(await _schedules.CreateAsync(uid, request), "Tạo lịch thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ScheduleDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<ScheduleDto>.Fail(ex.Message)); }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Update(int id, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ScheduleDto>.Ok(await _schedules.UpdateAsync(uid, id, request), "Cập nhật lịch thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ScheduleDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ScheduleDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<ScheduleDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _schedules.DeleteAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã xoá lịch."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
