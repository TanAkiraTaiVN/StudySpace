using System.Collections.ObjectModel;
using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class DocumentsViewModel : BaseViewModel
{
    private readonly ApiService _api;
    public ObservableCollection<GroupModel> Groups { get; } = new();
    public ObservableCollection<DocumentModel> Documents { get; } = new();

    private GroupModel? _selectedGroup;
    public GroupModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetField(ref _selectedGroup, value))
                _ = LoadDocumentsAsync();
        }
    }

    public ICommand RefreshCommand { get; }

    public DocumentsViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () =>
        {
            await LoadGroupsAsync();
            await LoadDocumentsAsync();
        });
    }

    public async Task LoadGroupsAsync()
    {
        try
        {
            var r = await _api.GetAsync<List<GroupModel>>("/api/groups/mine");
            Groups.Clear();
            if (r.Success && r.Data != null)
            {
                foreach (var g in r.Data) Groups.Add(g);
                SelectedGroup ??= Groups.FirstOrDefault();
            }
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    public async Task LoadDocumentsAsync()
    {
        Documents.Clear();
        if (SelectedGroup == null) return;
        try
        {
            IsBusy = true;
            var r = await _api.GetAsync<List<DocumentModel>>($"/api/documents/by-group/{SelectedGroup.Id}");
            if (r.Success && r.Data != null)
                foreach (var d in r.Data) Documents.Add(d);
            else ErrorMessage = r.Message;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    public string GetDownloadUrl(int documentId) => $"{_api.BaseUrl}/api/documents/{documentId}/download";
}
