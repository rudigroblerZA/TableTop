namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// The navigation a shared ViewModel is allowed to ask for.
///
/// <para>
/// Deliberately narrow. WinUI navigates by swapping a <c>ViewModelBase</c> into
/// a host control; MAUI pushes <c>Page</c> objects onto a <c>NavigationPage</c>
/// stack. Those are irreconcilable as a shared abstraction, and trying to model
/// both is how a "shared" layer ends up carrying two platform's worth of
/// concepts and sharing nothing.
/// </para>
///
/// <para>
/// So this models only what every screen genuinely needs and both heads can
/// honestly implement: going back. Anything richer stays in the head that owns
/// it, where it can use its own idiom without pretending.
/// </para>
/// </summary>
public interface INavigator
{
    /// <summary>Returns to the previous screen.</summary>
    void GoBack();
}
