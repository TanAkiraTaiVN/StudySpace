using StudySpace.Mobile.ViewModels;

namespace StudySpace.Mobile.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
