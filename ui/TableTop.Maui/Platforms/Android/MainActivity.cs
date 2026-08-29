using Android.App;
using Android.Content;
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
// Android TV home-row entry. MainLauncher above emits the MAIN/LAUNCHER filter
// phones and tablets scan; this adds the LEANBACK_LAUNCHER category the TV
// launcher looks for. Same activity — MAUI's AppCompat views are D-pad
// focusable out of the box, so the in-app UI needs no per-screen wiring.
[IntentFilter(
    new[] { Intent.ActionMain },
    Categories = new[] { "android.intent.category.LEANBACK_LAUNCHER" })]
public class MainActivity : MauiAppCompatActivity
{
}
