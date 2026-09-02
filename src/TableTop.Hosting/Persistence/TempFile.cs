namespace TableTop.Hosting.Persistence;

/// <summary>
/// Cleanup for the write-to-temp-then-rename dance the three JSON repositories
/// in this folder share.
/// </summary>
/// <remarks>
/// Each of those repositories writes a uniquely-named <c>.tmp</c> file, renames
/// it over the real one, and on failure deletes the temp file before rethrowing.
/// That delete used to be unguarded, which made the failure path worse than the
/// failure: if deleting the temp file threw — a read-only directory, or the file
/// still held open by a scanner — that exception propagated <i>instead of</i> the
/// original write failure, so the caller was told "cannot delete
/// settings.json.a1b2.tmp" when the real cause was a full disk. The cleanup can
/// only ever be best-effort, so it now says so.
/// </remarks>
internal static class TempFile
{
    /// <summary>
    /// Deletes <paramref name="path"/> if it is there, and never throws for the
    /// reasons a delete realistically fails.
    /// </summary>
    /// <remarks>
    /// No <c>File.Exists</c> check: <see cref="File.Delete(string)"/> already
    /// no-ops on a missing file, and checking first is a race rather than a
    /// guard. A leftover temp file is harmless — it is uniquely named, so it
    /// collides with nothing, and the next successful write replaces the real
    /// file regardless.
    /// </remarks>
    internal static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Best-effort: swallowed so the caller's original exception is the
            // one that reaches the caller.
        }
        catch (UnauthorizedAccessException)
        {
            // Same — see above.
        }
    }
}
