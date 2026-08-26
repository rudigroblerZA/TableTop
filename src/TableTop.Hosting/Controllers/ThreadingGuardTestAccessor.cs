namespace TableTop.Hosting.Controllers;

/// <summary>
/// Exposes <see cref="ThreadingGuard"/> internals for unit testing.
///
/// The guard used to be compiled out of Release entirely, and this accessor's
/// documentation said so. Backlog L.4 replaced <c>[Conditional("DEBUG")]</c>
/// with the runtime <see cref="Enabled"/> switch, so a Release-mode test can now
/// turn the guard on and exercise it — which is the configuration CI runs.
/// </summary>
public sealed class ThreadingGuard_TestAccessor
{
    private readonly ThreadingGuard _guard = new();

    /// <summary>
    /// Whether the guard enforces its contract. Defaults to on in Debug, off in
    /// Release. Tests that need it should set it and restore the previous value
    /// in a <c>finally</c>, because it is process-wide.
    /// </summary>
    public static bool Enabled
    {
        get => ThreadingGuard.Enabled;
        set => ThreadingGuard.Enabled = value;
    }

    /// <summary>Transfers ownership to the current thread (calls the real guard).</summary>
    public void TransferOwnership() => _guard.TransferOwnership();

    /// <summary>
    /// Calls Assert on the real guard with an explicit method name. Throws
    /// <see cref="System.InvalidOperationException"/> from a non-owner thread
    /// when <see cref="Enabled"/> is true.
    /// </summary>
    public void Assert(string method) => _guard.Assert(method);
}
