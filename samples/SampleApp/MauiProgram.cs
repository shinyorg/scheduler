using Microsoft.Extensions.Logging;
using MauiDevFlow.Agent;
using SampleApp.Pages;
using SampleApp.Services;
using SampleApp.ViewModels;
using Shiny.Maui.Scheduler;

namespace SampleApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<ISchedulerEventProvider, SampleSchedulerProvider>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<AgendaViewModel>();
        builder.Services.AddTransient<CalendarListViewModel>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<AgendaPage>();
        builder.Services.AddTransient<CalendarListPage>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent();
#endif

        return builder.Build();
    }
}
