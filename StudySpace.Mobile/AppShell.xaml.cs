using StudySpace.Mobile.Services;

namespace StudySpace.Mobile;

public partial class AppShell : Shell
{
	private readonly AuthState _auth;

	public AppShell(AuthState auth)
	{
		InitializeComponent();
		_auth = auth;
	}

	private async void OnLogoutClicked(object sender, EventArgs e)
	{
		_auth.Logout();
		await GoToAsync("//login");
	}
}
