using Android.App;
using Android.Content.PM;

namespace TableTop.Maui;

// Exported is REQUIRED from Android 12 (API 31) on any component with an
// intent filter — and MainLauncher creates one. Without it the platform
// refuses to launch the activity.
[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
