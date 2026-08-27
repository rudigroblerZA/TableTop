using System.Text.RegularExpressions;

namespace TableTop.Hosting;

/// <summary>
/// Detects and extracts multiple-choice options (A) … B) … C) … D) …) from a
/// card's text. Personality-quiz modes format their scenarios this way; UIs
/// use this to replace the generic "Done" button with tappable answer buttons
/// and to tally each player's letters for the results card.
///
/// Purely lexical — no card type changes needed, so every existing and future
/// mode that writes "A) option" lines gets answer buttons for free.
/// </summary>
public static class ChoiceCards
{
    private static readonly Regex ChoiceLine = new(
        @"^\s*([A-D])\)\s+(.+?)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Extracts the choice options from <paramref name="cardText"/>.
    /// Returns an empty list unless at least two options are present and the
    /// letters start at A and ascend without gaps — anything else is treated
    /// as ordinary prose that merely mentions "A)".
    /// </summary>
    /// <param name="cardText">The card description (HTML tags are tolerated).</param>
    public static IReadOnlyList<(char Letter, string Text)> Extract(string? cardText)
    {
        if (string.IsNullOrEmpty(cardText)) return Array.Empty<(char, string)>();

        var found = new List<(char Letter, string Text)>();
        foreach (Match m in ChoiceLine.Matches(cardText))
            found.Add((m.Groups[1].Value[0], m.Groups[2].Value));

        if (found.Count < 2) return Array.Empty<(char, string)>();
        for (var i = 0; i < found.Count; i++)
            if (found[i].Letter != (char)('A' + i))
                return Array.Empty<(char, string)>();

        return found;
    }

    /// <summary>True when the card presents an A/B/C/D style multiple choice.</summary>
    public static bool IsChoiceCard(string? cardText) => Extract(cardText).Count > 0;

    /// <summary>
    /// Returns the dominant letter in a tally (most-chosen; ties broken by
    /// earliest letter), or null for an empty tally.
    /// </summary>
    public static char? Dominant(IReadOnlyDictionary<char, int> tally) =>
        tally.Count == 0
            ? null
            : tally.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First().Key;

    private static readonly Regex StyleLine = new(
        @"Mostly\s+([A-D])\s*[—–-]+\s*(The\s+[^.<\n]+)",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// Scans a deck's card texts for a results key of the form
    /// "Mostly A — The Pathfinder." and returns the letter → style-name map,
    /// so UIs can announce an actual personality ("Bob → The Pathfinder")
    /// instead of a bare letter. Empty when the deck has no such key.
    /// </summary>
    public static IReadOnlyDictionary<char, string> ExtractStyleNames(IEnumerable<string?> cardTexts)
    {
        var map = new Dictionary<char, string>();
        foreach (var text in cardTexts)
        {
            if (string.IsNullOrEmpty(text)) continue;
            foreach (Match m in StyleLine.Matches(text))
                map[m.Groups[1].Value[0]] = m.Groups[2].Value.Trim();
        }
        return map;
    }

    /// <summary>
    /// Convenience: resolves a tally straight to its verdict — the style name
    /// when the deck defines one ("The Pathfinder"), otherwise "mostly A".
    /// </summary>
    public static string Verdict(IReadOnlyDictionary<char, int> tally, IReadOnlyDictionary<char, string> styles)
    {
        var d = Dominant(tally);
        if (d is null) return "no answers";
        return styles.TryGetValue(d.Value, out var name) ? $"{name} ({d})" : $"mostly {d}";
    }

    /// <summary>Formats a tally as "A:3 B:1 C:2" for compact display.</summary>
    public static string Format(IReadOnlyDictionary<char, int> tally) =>
        string.Join(" ", tally.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"));
}
