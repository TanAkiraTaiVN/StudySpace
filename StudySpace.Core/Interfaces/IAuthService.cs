using StudySpace.Core.DTOs.Auth;

namespace StudySpace.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<List<UserDto>> ListUsersAsync(string? search = null);
    Task<UserDto> SetUserActiveAsync(int userId, bool isActive);
    Task<UserDto> UpdateUserRoleAsync(int userId, string role);
}
