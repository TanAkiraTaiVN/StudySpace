using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Chat;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chat;

    public ChatController(IChatService chat)
    {
        _chat = chat;
    }

    [HttpGet("by-group/{groupId}")]
    public async Task<ActionResult<ApiResponse<List<ChatMessageDto>>>> ListByGroup(int groupId, [FromQuery] int take = 50, [FromQuery] int? beforeId = null)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<List<ChatMessageDto>>.Ok(await _chat.ListAsync(uid, groupId, take, beforeId)));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<List<ChatMessageDto>>.Fail(ex.Message)); }
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> Send([FromBody] SendMessageRequest request)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            return Ok(ApiResponse<ChatMessageDto>.Ok(await _chat.SendAsync(uid, request), "Đã gửi tin nhắn."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<ChatMessageDto>.Fail(ex.Message)); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _chat.DeleteAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã xoá tin nhắn."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
