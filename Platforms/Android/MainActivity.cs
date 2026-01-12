using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace TheDuckIsWatching;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density
    )]
public class MainActivity : MauiAppCompatActivity {
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // установка флага безопасности для всего приложения на уровне ядра (пустой или черный экран, но иногда может фоткать модалки)
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
    }
}
