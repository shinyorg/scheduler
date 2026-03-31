using SampleApp.ViewModels;

namespace SampleApp.Pages;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(CalendarViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
