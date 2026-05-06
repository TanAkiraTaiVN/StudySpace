using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly AuthState _auth;

    private string _email = "admin@studyspace.com";
    public string Email { get => _email; set => SetField(ref _email, value); }

    private string _password = "Admin@123";
    public string Password { get => _password; set => SetField(ref _password, value); }

    private string _baseUrl = string.Empty;
    public string BaseUrl { get => _baseUrl; set => SetField(ref _baseUrl, value); }

    public ICommand LoginCommand { get; }

    public LoginViewModel(ApiService api, AuthState auth)
    {
        _api = api;
        _auth = auth;
        _baseUrl = api.BaseUrl;
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        try
        {
            ErrorMessage = null;
            IsBusy = true;
            if (!string.IsNullOrWhiteSpace(BaseUrl)) _api.SetBaseUrl(BaseUrl);

            var resp = await _api.PostAsync<AuthResponse>("/api/auth/login",
                new LoginRequest { Email = Email, Password = Password });

            if (!resp.Success || resp.Data is null)
            {
                ErrorMessage = resp.Message ?? "Đăng nhập thất bại";
                return;
            }

            _auth.SetAuth(resp.Data);
            await Shell.Current.GoToAsync("//groups");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
