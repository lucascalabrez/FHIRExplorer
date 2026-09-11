using FHIRExplorer.Pages;
using FHIRExplorer.Services;

namespace FHIRExplorer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

        // Contract -> concrete implementation.
        builder.Services.AddSingleton<IFhirService, FhirService>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ResourceDetailPage>();
        builder.Services.AddTransient<SearchsetJsonPage>();

        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
