using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui.Services;

/// <summary>
/// MAUI's implementation of <see cref="INavigator"/>.
///
/// The interface is deliberately just "go back", because that is the only
/// navigation concept WinUI's ViewModel-swapping and MAUI's page stack both
/// express honestly. Everything richer stays in the head that owns it.
/// </summary>
public sealed class MauiNavigator(Page page) : INavigator
{
    /// <inheritdoc />
    public void GoBack() => page.Navigation.PopAsync();
}
