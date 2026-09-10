using FHIRExplorer.Services;
using FHIRExplorer.Pages;
using FHIRExplorer.Services;
namespace FHIRExplorer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<IFhirService, FhirService>();

        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<ResourceDetailPage>();

        builder.Services.AddTransient<SearchsetJsonPage>();

        return builder.Build();
    }
}
