using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Notification;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notif;

    public NotificationsController(INotificationService notif)
    {
        _notif = notif;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> List([FromQuery] bool unreadOnly = false, [FromQuery] int take = 50)
    {
        var uid = CurrentUser.RequireUserId(User);
        return Ok(ApiResponse<List<NotificationDto>>.Ok(await _notif.ListAsync(uid, unreadOnly, take)));
    }

    [HttpGet("count-unread")]
    public async Task<ActionResult<ApiResponse<int>>> CountUnread()
    {
        var uid = CurrentUser.RequireUserId(User);
        return Ok(ApiResponse<int>.Ok(await _notif.CountUnreadAsync(uid)));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse>> MarkRead(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _notif.MarkReadAsync(uid, id);
            return Ok(ApiResponse.Ok());
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllRead()
    {
        var uid = CurrentUser.RequireUserId(User);
        await _notif.MarkAllReadAsync(uid);
        return Ok(ApiResponse.Ok("Đã đánh dấu tất cả là đã đọc."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            var uid = CurrentUser.RequireUserId(User);
            await _notif.DeleteAsync(uid, id);
            return Ok(ApiResponse.Ok("Đã xoá thông báo."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
