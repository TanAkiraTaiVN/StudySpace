using System.Collections.ObjectModel;
using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class ChatViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly AuthState _auth;

    public ObservableCollection<GroupModel> Groups { get; } = new();
    public ObservableCollection<ChatMessageModel> Messages { get; } = new();

    private GroupModel? _selectedGroup;
    public GroupModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetField(ref _selectedGroup, value))
                _ = LoadMessagesAsync();
        }
    }

    private string _draft = string.Empty;
    public string Draft { get => _draft; set => SetField(ref _draft, value); }

    public int CurrentUserId => _auth.UserId;

    public ICommand SendCommand { get; }
    public ICommand RefreshCommand { get; }

    public ChatViewModel(ApiService api, AuthState auth)
    {
        _api = api;
        _auth = auth;
        SendCommand = new Command(async () => await SendAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(Draft));
        RefreshCommand = new Command(async () => await LoadMessagesAsync());
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

    public async Task LoadMessagesAsync()
    {
        Messages.Clear();
        if (SelectedGroup == null) return;
        try
        {
            IsBusy = true;
            var r = await _api.GetAsync<List<ChatMessageModel>>($"/api/chat/by-group/{SelectedGroup.Id}?take=80");
            if (r.Success && r.Data != null)
                foreach (var m in r.Data) Messages.Add(m);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task SendAsync()
    {
        if (SelectedGroup == null || string.IsNullOrWhiteSpace(Draft)) return;
        try
        {
            IsBusy = true;
            var r = await _api.PostAsync<ChatMessageModel>("/api/chat/send",
                new SendMessageRequest { StudyGroupId = SelectedGroup.Id, Content = Draft.Trim() });
            if (r.Success && r.Data != null) Messages.Add(r.Data);
            Draft = string.Empty;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
