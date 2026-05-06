using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Group;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groups;

    public GroupsController(IGroupService groups)
    {
        _groups = groups;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GroupDto>>>> List([FromQuery] string? search, [FromQuery] bool? joinedOnly)
    {
        var uid = CurrentUser.GetUserId(User);
        var list = await _groups.ListAsync(uid, search, joinedOnly);
        return Ok(ApiResponse<List<GroupDto>>.Ok(list));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<GroupDto>>>> Mine()
    {
        var uid = CurrentUser.RequireUserId(User);
        return Ok(ApiResponse<List<GroupDto>>.Ok(await _groups.MyGroupsAsync(uid)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GroupDto>>> Get(int id)
    {
        try
        {
            var uid = CurrentUser.GetUserId(User);
            return Ok(ApiResponse<GroupDto>.Ok(await _groups.GetByIdAsync(uid, id)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GroupDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupDto>>> Create([FromBody] CreateGroupRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<GroupDto>.Ok(await _groups.CreateAsync(uid, request), "Tạo nhóm thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GroupDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<GroupDto>>> Update(int id, [FromBody] UpdateGroupRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<GroupDto>.Ok(await _groups.UpdateAsync(uid, id, request), "Cập nhật nhóm thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<GroupDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<GroupDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _groups.DeleteAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã xoá nhóm."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("join")]
    public async Task<ActionResult<ApiResponse<GroupMemberDto>>> Join([FromBody] JoinGroupRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<GroupMemberDto>.Ok(await _groups.JoinAsync(uid, request), "Tham gia nhóm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<GroupMemberDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<GroupMemberDto>.Fail(ex.Message)); }
    }

    [HttpPost("{id}/leave")]
    public async Task<ActionResult<ApiResponse>> Leave(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _groups.LeaveAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã rời nhóm."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<ApiResponse<List<GroupMemberDto>>>> Members(int id)
    {
        var uid = CurrentUser.GetUserId(User);
        return Ok(ApiResponse<List<GroupMemberDto>>.Ok(await _groups.ListMembersAsync(uid, id)));
    }

    [HttpPut("{id}/members/{userId}/role")]
    public async Task<ActionResult<ApiResponse<GroupMemberDto>>> UpdateMemberRole(int id, int userId, [FromBody] UpdateMemberRoleRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<GroupMemberDto>.Ok(await _groups.UpdateMemberRoleAsync(uid, id, userId, request), "Cập nhật vai trò thành công."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<GroupMemberDto>.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<GroupMemberDto>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<GroupMemberDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<ActionResult<ApiResponse>> RemoveMember(int id, int userId)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _groups.RemoveMemberAsync(uid, id, userId);
            return Ok(ApiResponse.Ok("Đã xoá thành viên."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }
}
