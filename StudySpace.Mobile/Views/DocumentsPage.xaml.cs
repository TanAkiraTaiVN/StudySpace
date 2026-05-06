using StudySpace.Mobile.ViewModels;

namespace StudySpace.Mobile.Views;

public partial class DocumentsPage : ContentPage
{
	private readonly DocumentsViewModel _vm;

	public DocumentsPage(DocumentsViewModel vm)
	{
		InitializeComponent();
		BindingContext = _vm = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadGroupsAsync();
		await _vm.LoadDocumentsAsync();
	}

	private async void OnDownloadClicked(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.CommandParameter is int id)
		{
			var url = _vm.GetDownloadUrl(id);
			await Launcher.Default.OpenAsync(new Uri(url));
		}
	}
}
