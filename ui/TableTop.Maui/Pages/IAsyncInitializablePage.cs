namespace TableTop.Maui.Pages;

/// <summary>
/// A page whose ViewModel needs an async build step — controller creation
/// through <c>IControllerFactory.CreateAsync</c> — that cannot happen in the
/// page's own constructor.
///
/// MAUI never awaits page construction (<c>Navigation.PushAsync</c> takes an
/// already-built <see cref="Microsoft.Maui.Controls.Page"/>), and a
/// constructor cannot itself be async, so the async build has to live
/// somewhere a caller CAN await: here. A caller must call
/// <see cref="InitializeAsync"/> and await it before the page is pushed or
/// otherwise shown — until then the page has no ViewModel and is not usable.
///
/// Backlog item 20: this replaces a `.GetAwaiter().GetResult()` inside each
/// implementing page's constructor, which blocked the UI thread for the
/// length of a controller build (deck construction, possibly disk I/O for a
/// resumed session) on every navigation into that screen.
/// </summary>
public interface IAsyncInitializablePage
{
    /// <summary>Builds the page's ViewModel and sets <c>BindingContext</c>. Must be awaited before the page is shown.</summary>
    Task InitializeAsync();
}
