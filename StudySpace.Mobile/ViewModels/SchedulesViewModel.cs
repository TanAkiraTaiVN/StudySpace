using System.Collections.ObjectModel;
using System.Windows.Input;
using StudySpace.Mobile.Services;

namespace StudySpace.Mobile.ViewModels;

public class SchedulesViewModel : BaseViewModel
{
    private readonly ApiService _api;
    public ObservableCollection<ScheduleModel> Schedules { get; } = new();
    public ICommand RefreshCommand { get; }

    public SchedulesViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var r = await _api.GetAsync<List<ScheduleModel>>("/api/schedules/upcoming?days=30");
            Schedules.Clear();
            if (r.Success && r.Data != null)
                foreach (var s in r.Data) Schedules.Add(s);
            else ErrorMessage = r.Message;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
