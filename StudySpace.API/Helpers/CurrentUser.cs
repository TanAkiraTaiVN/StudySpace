using System.Security.Claims;

namespace StudySpace.API.Helpers;

public static class CurrentUser
{
    public static int? GetUserId(ClaimsPrincipal? user)
    {
        var idStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : (int?)null;
    }

    public static int RequireUserId(ClaimsPrincipal? user)
        => GetUserId(user) ?? throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");
}
