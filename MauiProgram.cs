using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using TheDuckIsWatching.Services;

namespace TheDuckIsWatching;

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
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            });

        builder.Services.AddSingleton<IGlobal, GlobalService>();
        builder.Services.AddSingleton<IStorage, StorageService>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SetPinPage>();
        builder.Services.AddTransient<ListPage>();
        builder.Services.AddTransient<CardPage>();
        builder.Services.AddTransient<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
