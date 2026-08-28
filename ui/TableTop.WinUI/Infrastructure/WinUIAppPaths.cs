namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Resolves where this head is allowed to actually write persistent data.
///
/// <para>
/// <see cref="AppContext.BaseDirectory"/> — the install directory — was the
/// default every JSON-backed store here used to write beside: settings,
/// saved rosters, the session snapshot, player profiles. That works for a
/// developer running from a build output folder and fails for an installed
/// one: <c>Program Files</c> is not writable by a standard user, and even
/// where a location happens to be writable, an app update that replaces the
/// install directory's contents takes the player's data with it.
/// </para>
///
/// <para>
/// This app ships unpackaged (<c>WindowsPackageType=None</c> — see
/// <c>TableTop.WinUI.csproj</c>), so the WinRT
/// <c>Windows.Storage.ApplicationData.Current</c> API is not available: it
/// requires package identity and throws for a Win32 app with none. The
/// portable equivalent every unpackaged Win32 app uses is
/// <see cref="Environment.SpecialFolder.LocalApplicationData"/>
/// (<c>%LOCALAPPDATA%</c>) — writable by the current user without elevation,
/// untouched by reinstalling or updating the app, and already how
/// <c>WinUIAppSettings</c> and <c>WinUIRosterStore</c> are documented as
/// wanting to behave, just not what either one's default path actually did.
/// </para>
/// </summary>
internal static class WinUIAppPaths
{
    /// <summary>
    /// The directory this head's JSON stores should write into — created on
    /// first access if it doesn't exist yet.
    /// </summary>
    public static string DataDirectory { get; } = Resolve();

    private static string Resolve()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TableTop");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
