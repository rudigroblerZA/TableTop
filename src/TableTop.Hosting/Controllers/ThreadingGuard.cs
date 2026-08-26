namespace TableTop.Hosting.Controllers;

/// <summary>
/// Enforces the <see cref="CardTurnController"/>'s single-threaded contract.
///
/// <para>
/// <strong>Threading model.</strong>
/// <see cref="CardTurnController"/> is not thread-safe. It is designed to be called
/// from a single thread — the thread that called <see cref="CardTurnController.Start"/>.
/// Events fire synchronously on that same thread. UIs that receive callbacks on a
/// different thread (e.g. a timer callback, a MAUI dispatcher) must marshal back
/// to the controller's thread before invoking any mutating methods.
/// </para>
///
/// <para>
/// <strong>Why not add locks?</strong>
/// Locking would not make the controller safe — events fire outside any lock boundary,
/// and <c>AdvanceTurn</c> calls itself recursively inside event handlers, which would
/// deadlock with any re-entrant mutex. A hard single-thread assertion is simpler and
/// unambiguously correct.
/// </para>
///
/// <para>
/// <strong>What this guard does.</strong>
/// Captures the thread ID at <see cref="TransferOwnership"/> (called by <c>Start</c>)
/// and throws <see cref="InvalidOperationException"/> when a mutating method is called
/// from any other thread.
/// </para>
///
/// <para>
/// <strong>Release builds (backlog L.4).</strong>
/// <see cref="Assert"/> used to carry <c>[Conditional("DEBUG")]</c>, which strips its
/// call sites from any caller compiled in Release. CI builds and tests in Release, and
/// releases ship in Release — so the guard protected no build anyone actually ran, and
/// a cross-thread call in production produced silent corruption instead of an exception.
/// A guard that is absent everywhere it matters is closer to a comment than a guard.
/// </para>
///
/// <para>
/// The attribute is gone; the default behaviour is unchanged. <see cref="Enabled"/>
/// starts <c>true</c> in Debug and <c>false</c> in Release, so nothing that ships
/// starts throwing where it previously limped along — but it is now a runtime switch,
/// so a Release build can turn it on to diagnose a threading bug without recompiling,
/// and Release-mode tests can exercise it. The cost when disabled is one boolean read
/// per mutating call.
/// </para>
/// </summary>
internal sealed class ThreadingGuard
{
    private int _ownerThreadId = -1;   // -1 = not yet set
    private string _ownerDescription = "unstarted";

    /// <summary>
    /// Whether <see cref="Assert"/> enforces the contract. Defaults to on in
    /// Debug, off in Release — the behaviour the <c>[Conditional("DEBUG")]</c>
    /// attribute used to give, but switchable at runtime rather than baked in
    /// at compile time.
    ///
    /// Turn it on in a Release build to diagnose a suspected threading bug, and
    /// in Release-mode tests that need to exercise the guard.
    /// </summary>
    public static bool Enabled { get; set; } =
#if DEBUG
        true;
#else
        false;
#endif

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Transfers ownership to the current thread.
    /// Call once from <c>Start()</c> so the guard tracks the gameplay thread,
    /// not the construction thread (which may differ when built via async factory).
    /// </summary>
    public void TransferOwnership()
    {
        _ownerThreadId = Environment.CurrentManagedThreadId;
        _ownerDescription = Thread.CurrentThread.Name is { Length: > 0 } n
            ? $"'{n}' (id {_ownerThreadId})"
            : $"thread #{_ownerThreadId}";
    }

    /// <summary>
    /// Asserts that the current call is on the owning thread. Does nothing when
    /// <see cref="Enabled"/> is false, which is the default in Release.
    /// </summary>
    public void Assert([System.Runtime.CompilerServices.CallerMemberName] string? method = null)
    {
        if (!Enabled) return;
        if (_ownerThreadId == -1) return;  // not yet started — allow freely

        var current = Environment.CurrentManagedThreadId;
        if (current == _ownerThreadId) return;

        var currentDesc = Thread.CurrentThread.Name is { Length: > 0 } n
            ? $"'{n}' (id {current})"
            : $"thread #{current}";

        throw new InvalidOperationException(
            $"CardTurnController.{method} was called from {currentDesc}, " +
            $"but the controller is owned by {_ownerDescription}. " +
            $"CardTurnController is not thread-safe — marshal calls to the owner thread before " +
            $"invoking any mutating methods. See ThreadingGuard.cs for details.");
    }
}
