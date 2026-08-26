using Android.App;
using Android.Runtime;

namespace TableTop.Maui;

/// <summary>Android application bootstrapper — MAUI's entry on this platform.</summary>
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership) { }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
