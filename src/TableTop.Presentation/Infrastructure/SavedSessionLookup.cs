using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;

namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// Looks for a saved session so a landing screen can offer to resume it.
///
/// This was declared twice, near byte-for-byte identical: MAUI's
/// <c>GameSelectionViewModel.LookForSavedSessionAsync</c> and WinUI's
/// <c>IntroViewModel.LookForSavedSessionAsync</c>, both wrapping
/// <see cref="ControllerFactory.LoadSavedSessionAsync"/> and
/// <see cref="SessionResumer.TryResolve"/> in the same try/catch, computing the
/// same <c>CanResume</c>/<c>ResumeText</c> pair. One implementation now.
///
/// A <see cref="Task"/>-returning method rather than a constructor side effect,
/// so each head decides when to call it — MAUI's landing page re-checks on
/// <c>OnAppearing</c> so a finished game doesn't leave a stale offer; WinUI
/// fires it once from its constructor. Both are legitimate, so neither is baked
/// in here.
/// </summary>
public sealed class SavedSessionLookup
{
    private readonly IControllerFactory? _controllerFactory;

    /// <summary>
    /// Creates a lookup.
    /// </summary>
    /// <param name="controllerFactory">
    /// Optional factory. Defaults to a plain <c>ControllerFactory</c> — what
    /// this class built inline before, which meant it ignored whatever
    /// persistence the host had configured and could therefore only ever
    /// report "no saved session".
    ///
    /// <para>
    /// Worth recording: that was previously written up in this project's own
    /// backlog as the "session found" path being <i>structurally
    /// untestable</i>. It never was — it was this bypass, misdiagnosed as an
    /// inherent limitation. With a factory injected, the path is ordinary to
    /// test and, more importantly, actually works in a head that configures
    /// persistence.
    /// </para>
    /// </param>
    public SavedSessionLookup(IControllerFactory? controllerFactory = null) =>
        _controllerFactory = controllerFactory;

    /// <summary>The resolved session, or null if there is nothing to resume.</summary>
    public ResumableSession? Resumable { get; private set; }

    /// <summary>True when there is a session worth offering.</summary>
    public bool CanResume => Resumable is not null;

    /// <summary>"Continue — Alice, Bob · round 4", or empty.</summary>
    public string ResumeText => Resumable is null
        ? string.Empty
        : $"Continue — {Resumable.PlayerSummary} · round {Resumable.Round}";

    /// <summary>
    /// Re-checks for a saved session. Failures are swallowed — a corrupt save
    /// should cost the player the resume button, nothing more.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            var snapshot = await (_controllerFactory ?? new ControllerFactory()).LoadSavedSessionAsync();
            Resumable = SessionResumer.TryResolve(
                snapshot, ArchetypeRegistry.Default().AllModes, currentRoster: null, out _);
        }
        catch
        {
            Resumable = null;
        }
    }
}
