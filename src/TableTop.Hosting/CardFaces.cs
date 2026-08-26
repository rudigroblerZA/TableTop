namespace TableTop.Hosting;

/// <summary>
/// Splits a card's text into a FRONT face (the question / statement) and an
/// optional BACK face (the answer / reading), so UIs can present answer-bearing
/// cards as physically flippable — question up, flip to reveal.
///
/// A back face begins at the first line starting with one of the reveal
/// markers ("Answer:", "The reading:"). Cards without a marker are single-faced.
/// Reader-instruction lines that only made sense when the answer was printed
/// on the same face ("keep the next line to yourself…") are dropped from the
/// front, since the flip now does that job physically.
/// </summary>
public static class CardFaces
{
    private static readonly string[] BackMarkers = ["Answer:", "The reading:"];

    private static readonly string[] ObsoleteFrontHints =
    [
        "keep the next line to yourself",
        "then tap to see the answer",
        "Reveal together, then tap",
        "then the reader reveals the folklore reading",
    ];

    /// <summary>
    /// Splits <paramref name="cardText"/> (HTML already stripped) into faces.
    /// <c>Back</c> is null when the card has no reveal marker.
    /// </summary>
    public static (string Front, string? Back) Split(string? cardText)
    {
        if (string.IsNullOrEmpty(cardText)) return (cardText ?? string.Empty, null);

        var lines = cardText.Split('\n');
        var backStart = -1;
        for (var i = 0; i < lines.Length; i++)
        {
            // Tag-tolerant: WPF passes HTML ("<b>Answer:</b> …"), MAUI
            // passes plain text — detect on the stripped line, split the original.
            var trimmed = CardText.StripHtml(lines[i]).TrimStart();
            if (BackMarkers.Any(m => trimmed.StartsWith(m, StringComparison.Ordinal)))
            {
                backStart = i;
                break;
            }
        }

        if (backStart < 0) return (cardText, null);

        var front = string.Join('\n',
                lines.Take(backStart)
                     .Where(l => !ObsoleteFrontHints.Any(h =>
                         l.Contains(h, StringComparison.OrdinalIgnoreCase))))
            .TrimEnd('\n', ' ', '\r');

        var back = string.Join('\n', lines.Skip(backStart)).Trim('\n', ' ', '\r');

        return (front, back);
    }

    /// <summary>True when the card carries a reveal (answer/reading) back face.</summary>
    public static bool HasBack(string? cardText) => Split(cardText).Back is not null;
}
