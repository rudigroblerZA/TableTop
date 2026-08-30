namespace TableTop.Maui.Pages;

/// <summary>
/// Navigation calls wrapped so an exception cannot escape an
/// <c>async void</c> event handler.
///
/// <para>
/// <b>Why this exists.</b> An exception escaping an <c>async void</c> handler
/// terminates the process on Android — there is no caller left to catch it.
/// This codebase already knew that and said so above four handlers, but the
/// rule was applied by hand and six other handlers never got it (backlog item
/// 27). A rule followed 4-of-10 times is one nobody can rely on.
/// </para>
///
/// <para>
/// So the rule lives here once instead of being restated at each call site.
/// That is the same preference this project's backlog reaches for elsewhere:
/// a guard that reads its decision from a single source cannot drift from it,
/// while five hand-copied try/catch blocks can — and four of ten already had.
/// </para>
/// </summary>
internal static class SafeNavigation
{
    /// <summary>
    /// Returns to the root page, reporting a failure instead of letting it
    /// reach the handler's non-existent caller.
    ///
    /// Every "Done"/"leave the game" button routes through here. In practice
    /// <c>PopToRootAsync</c> on an already-rooted stack is tolerated, so this
    /// is a backstop rather than a fix for a reproduced crash — but it is the
    /// documented rule, and the cost of honouring it is one call.
    /// </summary>
    public static async Task SafePopToRootAsync(this Page page)
    {
        try
        {
            await page.Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            await page.DisplayAlertAsync("Couldn't leave the game", ex.Message, "OK");
        }
    }
}
