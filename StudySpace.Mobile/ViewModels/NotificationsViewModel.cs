using System.Collections.ObjectModel;
using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class NotificationsViewModel : BaseViewModel
{
    private readonly ApiService _api;
    public ObservableCollection<NotificationModel> Notifications { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand MarkAllReadCommand { get; }

    public NotificationsViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadAsync());
        MarkAllReadCommand = new Command(async () =>
        {
            await _api.PutAsync<object>("/api/notifications/read-all");
            await LoadAsync();
        });
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var r = await _api.GetAsync<List<NotificationModel>>("/api/notifications?take=100");
            Notifications.Clear();
            if (r.Success && r.Data != null)
                foreach (var n in r.Data) Notifications.Add(n);
            else ErrorMessage = r.Message;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
