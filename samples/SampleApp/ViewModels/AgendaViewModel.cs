using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Shiny.Maui.Scheduler;

namespace SampleApp.ViewModels;

public class AgendaViewModel : INotifyPropertyChanged
{
    DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    int _daysToShow = 1;

    public AgendaViewModel(ISchedulerEventProvider provider)
    {
        Provider = provider;
        ToggleDaysCommand = new Command(() =>
        {
            DaysToShow = DaysToShow == 1 ? 3 : 1;
        });
    }

    public ISchedulerEventProvider Provider { get; }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set { _selectedDate = value; OnPropertyChanged(); }
    }

    public int DaysToShow
    {
        get => _daysToShow;
        set { _daysToShow = value; OnPropertyChanged(); OnPropertyChanged(nameof(DaysToggleText)); }
    }

    public string DaysToggleText => DaysToShow == 1 ? "3-Day" : "1-Day";

    public ICommand ToggleDaysCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
