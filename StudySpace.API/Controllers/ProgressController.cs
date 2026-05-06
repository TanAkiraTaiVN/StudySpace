using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Progress;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progress;

    public ProgressController(IProgressService progress)
    {
        _progress = progress;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<ProgressDto>>>> Mine([FromQuery] int? groupId)
    {
        var uid = CurrentUser.RequireUserId(User);
        return Ok(ApiResponse<List<ProgressDto>>.Ok(await _progress.ListMineAsync(uid, groupId)));
    }

    [HttpGet("by-group/{groupId}")]
    public async Task<ActionResult<ApiResponse<List<ProgressDto>>>> ByGroup(int groupId)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<List<ProgressDto>>.Ok(await _progress.ListByGroupAsync(uid, groupId)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<List<ProgressDto>>.Fail(ex.Message)); }
    }

    [HttpGet("group/{groupId}/summary")]
    public async Task<ActionResult<ApiResponse<GroupProgressSummaryDto>>> GroupSummary(int groupId)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<GroupProgressSummaryDto>.Ok(await _progress.GroupSummaryAsync(uid, groupId)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<GroupProgressSummaryDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<GroupProgressSummaryDto>.Fail(ex.Message)); }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProgressDto>>> Get(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ProgressDto>.Ok(await _progress.GetByIdAsync(uid, id)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ProgressDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProgressDto>.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProgressDto>>> Create([FromBody] CreateProgressRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ProgressDto>.Ok(await _progress.CreateAsync(uid, request), "Tạo tiến độ thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ProgressDto>.Fail(ex.Message)); }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProgressDto>>> Update(int id, [FromBody] UpdateProgressRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ProgressDto>.Ok(await _progress.UpdateAsync(uid, id, request), "Cập nhật tiến độ thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ProgressDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProgressDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _progress.DeleteAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã xoá."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
