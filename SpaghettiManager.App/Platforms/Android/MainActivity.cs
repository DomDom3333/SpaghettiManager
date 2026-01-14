using Android.App;
using Android.Content.PM;

namespace SpaghettiManager.App;

[Activity(
    Name = "EssiDev.SpaghettiManager.MainActivity",
    LaunchMode = LaunchMode.SingleTop,
    ResizeableActivity = true,
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density
)]
[IntentFilter(
    [
        ShinyPushIntents.NotificationClickAction,
        Platform.Intent.ActionAppAction,
        global::Android.Content.Intent.ActionView
    ],
    Categories =
    [
        global::Android.Content.Intent.CategoryDefault,
        global::Android.Content.Intent.CategoryBrowsable
    ]
)]
public class MainActivity : MauiAppCompatActivity
{
}