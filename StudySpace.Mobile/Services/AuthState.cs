namespace StudySpace.Mobile.Services;

public class AuthState
{
    public string? Token { get; private set; }
    public int UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Member";
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public event EventHandler<bool>? AuthChanged;

    public void SetAuth(AuthResponse response)
    {
        Token = response.Token;
        UserId = response.UserId;
        FullName = response.FullName;
        Role = response.Role;
        Preferences.Set("token", response.Token);
        Preferences.Set("uid", response.UserId);
        Preferences.Set("fullname", response.FullName);
        Preferences.Set("role", response.Role);
        AuthChanged?.Invoke(this, true);
    }

    public void Logout()
    {
        Token = null;
        UserId = 0;
        FullName = string.Empty;
        Role = "Member";
        Preferences.Remove("token");
        Preferences.Remove("uid");
        Preferences.Remove("fullname");
        Preferences.Remove("role");
        AuthChanged?.Invoke(this, false);
    }

    public void Restore()
    {
        Token = Preferences.Get("token", null);
        UserId = Preferences.Get("uid", 0);
        FullName = Preferences.Get("fullname", string.Empty);
        Role = Preferences.Get("role", "Member");
    }
}
