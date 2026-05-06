using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Auth;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> List([FromQuery] string? search)
        => Ok(ApiResponse<List<UserDto>>.Ok(await _auth.ListUsersAsync(search)));

    [HttpPut("{id}/active")]
    public async Task<ActionResult<ApiResponse<UserDto>>> SetActive(int id, [FromBody] SetActiveRequest request)
    {
        try
        {
            return Ok(ApiResponse<UserDto>.Ok(await _auth.SetUserActiveAsync(id, request.IsActive),
                request.IsActive ? "Đã kích hoạt người dùng." : "Đã khoá người dùng."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<ApiResponse<UserDto>>> SetRole(int id, [FromBody] SetRoleRequest request)
    {
        try
        {
            return Ok(ApiResponse<UserDto>.Ok(await _auth.UpdateUserRoleAsync(id, request.Role), "Đã cập nhật vai trò."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    public class SetActiveRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
