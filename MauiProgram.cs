using FHIRExplorer.Services;

namespace FHIRExplorer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<FhirService>();

        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
