using StudySpace.Mobile.ViewModels;

namespace StudySpace.Mobile.Views;

public partial class GroupsPage : ContentPage
{
	private readonly GroupsViewModel _vm;

	public GroupsPage(GroupsViewModel vm)
	{
		InitializeComponent();
		BindingContext = _vm = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync();
	}
}
