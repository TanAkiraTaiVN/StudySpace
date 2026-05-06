using StudySpace.Mobile.ViewModels;

namespace StudySpace.Mobile.Views;

public partial class ChatPage : ContentPage
{
	private readonly ChatViewModel _vm;

	public ChatPage(ChatViewModel vm)
	{
		InitializeComponent();
		BindingContext = _vm = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadGroupsAsync();
		await _vm.LoadMessagesAsync();
	}
}
