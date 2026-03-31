using SampleApp.ViewModels;

namespace SampleApp.Pages;

public partial class AgendaPage : ContentPage
{
    public AgendaPage(AgendaViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
