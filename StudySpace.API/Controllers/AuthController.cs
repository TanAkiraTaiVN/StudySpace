using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Helpers;
using StudySpace.Core.DTOs;
using StudySpace.Core.DTOs.Auth;
using StudySpace.Core.Interfaces;

namespace StudySpace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _auth.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _auth.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng ký thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me()
    {
        try
        {
            var id = CurrentUser.RequireUserId(User);
            var profile = await _auth.GetProfileAsync(id);
            return Ok(ApiResponse<UserDto>.Ok(profile));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<UserDto>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var id = CurrentUser.RequireUserId(User);
            var profile = await _auth.UpdateProfileAsync(id, request);
            return Ok(ApiResponse<UserDto>.Ok(profile, "Cập nhật hồ sơ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var id = CurrentUser.RequireUserId(User);
            await _auth.ChangePasswordAsync(id, request);
            return Ok(ApiResponse.Ok("Đổi mật khẩu thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
