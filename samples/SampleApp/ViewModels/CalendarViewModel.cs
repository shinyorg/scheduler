using System.ComponentModel;
using System.Runtime.CompilerServices;
using Shiny.Maui.Scheduler;

namespace SampleApp.ViewModels;

public class CalendarViewModel : INotifyPropertyChanged
{
    DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    DateOnly _displayMonth = DateOnly.FromDateTime(DateTime.Today);

    public CalendarViewModel(ISchedulerEventProvider provider)
    {
        Provider = provider;
    }

    public ISchedulerEventProvider Provider { get; }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set { _selectedDate = value; OnPropertyChanged(); }
    }

    public DateOnly DisplayMonth
    {
        get => _displayMonth;
        set { _displayMonth = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
