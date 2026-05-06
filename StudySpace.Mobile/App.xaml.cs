using StudySpace.Mobile.Services;

namespace StudySpace.Mobile;

public partial class App : Application
{
	public App(AuthState auth)
	{
		InitializeComponent();
		MainPage = new AppShell(auth);
	}

	protected override void OnStart()
	{
		base.OnStart();
		var auth = IPlatformApplication.Current!.Services.GetRequiredService<AuthState>();
		auth.Restore();
		var route = auth.IsAuthenticated ? "//groups" : "//login";
		Shell.Current?.GoToAsync(route);
	}
}
