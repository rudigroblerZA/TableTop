namespace TableTop.Hosting;

/// <summary>
/// Detects a Truth-or-Dare card's paired TRUTH/DARE prompts (and its chicken-
/// clause forfeit) from plain card text, so a UI can ask which the player
/// declares and reveal only that half — the physical game's whole point,
/// which showing both halves on screen at once defeats.
///
/// Purely lexical, same approach as <see cref="CardFaces"/> and
/// <see cref="ChoiceCards"/>: no card type or per-mode UI work is needed, and
/// any future deck that writes a card this way gets the same declare-then-
/// reveal behaviour for free.
///
/// <para>
/// A card matches when it has a line starting with <c>TRUTH:</c> followed
/// later by one starting with <c>DARE:</c>. Everything before the first is
/// the intro, shown before the player declares. An optional line starting
/// with <c>Chicken clause:</c> after the dare is the forfeit for backing out
/// once you've heard your half.
/// </para>
/// </summary>
public static class TruthOrDareCards
{
    private const string TruthMarker = "TRUTH:";
    private const string DareMarker = "DARE:";
    private const string ForfeitMarker = "Chicken clause:";

    /// <summary>True when <paramref name="cardText"/> is a Truth-or-Dare card.</summary>
    public static bool IsTruthOrDareCard(string? cardText) => TryParse(cardText) is not null;

    /// <summary>
    /// Splits a Truth-or-Dare card into its parts, or null when the text
    /// doesn't match the TRUTH/DARE convention (an ordinary card, or one that
    /// merely mentions the words in passing).
    /// </summary>
    public static (string Intro, string Truth, string Dare, string Forfeit)? TryParse(string? cardText)
    {
        if (string.IsNullOrEmpty(cardText)) return null;

        var lines = cardText.Split('\n');
        var truthAt = FindLine(lines, TruthMarker);
        var dareAt = FindLine(lines, DareMarker);
        if (truthAt < 0 || dareAt < 0 || dareAt <= truthAt) return null;

        var forfeitAt = FindLine(lines, ForfeitMarker);
        var dareEnd = forfeitAt >= 0 ? forfeitAt : lines.Length;

        var intro = Join(lines, 0, truthAt);
        var truth = StripMarker(Join(lines, truthAt, dareAt), TruthMarker);
        var dare = StripMarker(Join(lines, dareAt, dareEnd), DareMarker);
        var forfeit = forfeitAt >= 0 ? StripMarker(Join(lines, forfeitAt, lines.Length), ForfeitMarker) : "";

        return (intro, truth, dare, forfeit);
    }

    private static int FindLine(string[] lines, string marker)
    {
        for (var i = 0; i < lines.Length; i++)
            if (lines[i].TrimStart().StartsWith(marker, StringComparison.Ordinal))
                return i;
        return -1;
    }

    private static string Join(string[] lines, int start, int end) =>
        string.Join('\n', lines.Skip(start).Take(end - start)).Trim('\n', ' ', '\r');

    private static string StripMarker(string text, string marker)
    {
        var idx = text.IndexOf(marker, StringComparison.Ordinal);
        return idx < 0 ? text : text[(idx + marker.Length)..].Trim();
    }
}
