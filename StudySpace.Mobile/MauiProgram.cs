using Microsoft.Extensions.Logging;
using StudySpace.Mobile.Services;
using StudySpace.Mobile.ViewModels;
using StudySpace.Mobile.Views;

namespace StudySpace.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ApiService>();
		builder.Services.AddSingleton<AuthState>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<GroupsPage>();
		builder.Services.AddTransient<GroupsViewModel>();
		builder.Services.AddTransient<SchedulesPage>();
		builder.Services.AddTransient<SchedulesViewModel>();
		builder.Services.AddTransient<NotificationsPage>();
		builder.Services.AddTransient<NotificationsViewModel>();
		builder.Services.AddTransient<DocumentsPage>();
		builder.Services.AddTransient<DocumentsViewModel>();
		builder.Services.AddTransient<ChatPage>();
		builder.Services.AddTransient<ChatViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
