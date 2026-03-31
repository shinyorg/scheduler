using SampleApp.ViewModels;

namespace SampleApp.Pages;

public partial class CalendarListPage : ContentPage
{
    public CalendarListPage(CalendarListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
