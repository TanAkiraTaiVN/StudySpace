using System.Collections.ObjectModel;
using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class GroupsViewModel : BaseViewModel
{
    private readonly ApiService _api;

    public ObservableCollection<GroupModel> Groups { get; } = new();

    private string _inviteCode = string.Empty;
    public string InviteCode { get => _inviteCode; set => SetField(ref _inviteCode, value); }

    public ICommand RefreshCommand { get; }
    public ICommand JoinCommand { get; }

    public GroupsViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadAsync());
        JoinCommand = new Command(async () => await JoinAsync(), () => !IsBusy);
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var r = await _api.GetAsync<List<GroupModel>>("/api/groups/mine");
            Groups.Clear();
            if (r.Success && r.Data != null)
                foreach (var g in r.Data) Groups.Add(g);
            else ErrorMessage = r.Message;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task JoinAsync()
    {
        if (string.IsNullOrWhiteSpace(InviteCode)) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var r = await _api.PostAsync<object>("/api/groups/join",
                new JoinGroupRequest { InviteCode = InviteCode.Trim().ToUpper() });
            if (!r.Success) { ErrorMessage = r.Message; return; }
            InviteCode = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
