using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudySpace.Core.DTOs.Auth;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;
using StudySpace.Core.Interfaces;
using StudySpace.Infrastructure.Data;

namespace StudySpace.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly StudySpaceDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(StudySpaceDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        return GenerateToken(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return GenerateToken(user);
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");
        return ToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Phone)) user.Phone = request.Phone.Trim();
        if (request.Bio != null) user.Bio = request.Bio.Trim();
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserDto>> ListUsersAsync(string? search = null)
    {
        var q = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(s)
                          || u.Email.ToLower().Contains(s)
                          || u.Phone.Contains(s));
        }
        var users = await q.OrderByDescending(u => u.CreatedAt).Take(200).ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto> SetUserActiveAsync(int userId, bool isActive)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<UserDto> UpdateUserRoleAsync(int userId, string role)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!Enum.TryParse<UserRole>(role, true, out var parsed))
            throw new InvalidOperationException("Vai trò không hợp lệ.");

        user.Role = parsed;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    private AuthResponse GenerateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            ExpiresAt = expires
        };
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Phone = u.Phone,
        AvatarUrl = u.AvatarUrl,
        Bio = u.Bio,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
