using Android.App;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Extensions;
using TableTop.Droid.Infrastructure;
using TableTop.Hosting.Extensions;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid;

/// <summary>
/// Android application object and the head's single composition root.
///
/// <para>
/// The container is built once here and exposed as <see cref="Services"/>. Every
/// engine/hosting type is resolved from it — nothing else in this head uses
/// <c>new</c> for an engine type, the same discipline Console's <c>Program.cs</c>
/// and WinUI's <c>App.xaml.cs</c> follow (backlog item 5).
/// </para>
/// </summary>
[Application]
public sealed class MainApplication : Application
{
    /// <summary>Required marshalling constructor for an <see cref="Application"/> subclass.</summary>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>The application-wide service container. Available from <see cref="OnCreate"/> onward.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <inheritdoc />
    public override void OnCreate()
    {
        base.OnCreate();

        // FilesDir is the app's private internal-storage directory — always
        // writable for an installed, unelevated app, the Android analog of
        // MAUI's FileSystem.AppDataDirectory. AddTableTopHosting's own default
        // (beside the executable) is not a valid write location here.
        var appData = FilesDir!.AbsolutePath;

        Services = new ServiceCollection()
            .AddTableTop()
            .AddTableTopHosting(
                sessionFilePath: Path.Combine(appData, "session.json"),
                playerFilePath: Path.Combine(appData, "players.json"))
            .AddSingleton<IAppSettings>(_ => new AndroidAppSettings(this))
            .AddSingleton<IRosterStore>(_ => new AndroidRosterStore(this))
            .BuildServiceProvider();
    }
}
