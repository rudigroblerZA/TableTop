using CC = System.ConsoleColor;
using SC = System.Console;
namespace TableTop.Console;

/// <summary>
/// Thin console rendering helpers. Isolated here so the game logic never touches Console directly.
/// </summary>
internal static class ConsoleUi
{
    // ── Colour palette ────────────────────────────────────────────────────────
    private static readonly ConsoleColor ColorHeading = CC.Cyan;
    private static readonly ConsoleColor ColorCard = CC.Yellow;
    private static readonly ConsoleColor ColorPlayer = CC.Green;
    private static readonly ConsoleColor ColorRestricted = CC.DarkYellow;
    private static readonly ConsoleColor ColorScore = CC.Magenta;
    private static readonly ConsoleColor ColorError = CC.Red;
    private static readonly ConsoleColor ColorMuted = CC.DarkGray;
    private static readonly ConsoleColor ColorSuccess = CC.Green;

    // ── Layout constants ──────────────────────────────────────────────────────
    private const int CardWidth = 60;

    // ── Public API ────────────────────────────────────────────────────────────

    public static void Clear() => SC.Clear();

    public static void Banner()
    {
        SC.ForegroundColor = ColorHeading;
        SC.WriteLine();
        SC.WriteLine("  ╔══════════════════════════════════════════════════════╗");
        SC.WriteLine("  ║        T R U T H   O R   D A R E                    ║");
        SC.WriteLine("  ║        Powered by TableTop                    ║");
        SC.WriteLine("  ╚══════════════════════════════════════════════════════╝");
        SC.ResetColor();
        SC.WriteLine();
    }

    public static void SectionHeader(string text)
    {
        SC.WriteLine();
        SC.ForegroundColor = ColorHeading;
        SC.WriteLine($"  ── {text} ──");
        SC.ResetColor();
    }

    public static void PrintCard(string category, string title, string description, string difficulty, string? restriction = null)
    {
        var border = new string('─', CardWidth);

        SC.WriteLine();
        SC.ForegroundColor = category == "Dare" ? CC.Red : CC.Blue;
        SC.WriteLine($"  ┌{border}┐");
        SC.WriteLine($"  │  {Pad($"[ {category.ToUpperInvariant()} ]  •  {difficulty}", CardWidth - 2)}│");
        SC.WriteLine($"  ├{border}┤");

        SC.ForegroundColor = ColorCard;
        SC.WriteLine($"  │  {Pad(title, CardWidth - 2)}│");
        SC.ResetColor();

        // Word-wrap description
        foreach (var line in WrapText(description, CardWidth - 4))
            SC.WriteLine($"  │    {Pad(line, CardWidth - 4)}│");

        if (restriction is not null)
        {
            SC.ForegroundColor = ColorRestricted;
            SC.WriteLine($"  │  {Pad($"⚠  {restriction}", CardWidth - 2)}│");
            SC.ResetColor();
        }

        SC.ForegroundColor = category == "Dare" ? CC.Red : CC.Blue;
        SC.WriteLine($"  └{border}┘");
        SC.ResetColor();
    }

    public static void PrintRoundHeader(int round, string playerName)
    {
        SC.WriteLine();
        SC.ForegroundColor = ColorMuted;
        SC.WriteLine($"  Round {round}");
        SC.ForegroundColor = ColorPlayer;
        SC.Write($"  ► {playerName}'s turn");
        SC.ResetColor();
        SC.WriteLine();
    }

    public static void PrintScoreboard(IEnumerable<(string Name, int Score)> standings)
    {
        SectionHeader("SCOREBOARD");
        var ordered = standings.OrderByDescending(s => s.Score).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => "  " };
            SC.ForegroundColor = i == 0 ? ColorScore : CC.White;
            SC.WriteLine($"    {medal}  {ordered[i].Name,-20} {ordered[i].Score,4} pts");
        }
        SC.ResetColor();
        SC.WriteLine();
    }

    public static void PrintTurnResult(string playerName, string outcome, int scoreDelta)
    {
        SC.ForegroundColor = outcome == "Completed" ? ColorSuccess : ColorMuted;
        SC.Write($"\n  {outcome}!");
        if (scoreDelta > 0)
        {
            SC.ForegroundColor = ColorScore;
            SC.Write($"  +{scoreDelta} pts to {playerName}");
        }
        SC.ResetColor();
        SC.WriteLine();
    }

    public static void PrintSkippedTurn(string playerName, string reason)
    {
        SC.ForegroundColor = ColorMuted;
        SC.WriteLine($"\n  ⚠  No eligible card found for {playerName}. ({reason})");
        SC.ResetColor();
    }

    /// <summary>
    /// Reports a successful undo. The reversed card reappears via the engine's
    /// own CardReady event straight after this — WinUI and MAUI show the same
    /// two-part sequence (a toast, then the card again), so this mirrors that
    /// rather than printing the whole card a second time itself.
    /// </summary>
    public static void PrintTurnUndone(string playerName, string cardTitle, int scoreRestored)
    {
        SC.ForegroundColor = ColorMuted;
        SC.Write($"\n  ↶ Undid \"{cardTitle}\" for {playerName}.");
        if (scoreRestored != 0)
        {
            SC.ForegroundColor = ColorScore;
            SC.Write($"  {(scoreRestored > 0 ? "+" : "")}{scoreRestored} pts");
        }
        SC.ResetColor();
        SC.WriteLine();
    }

    public static void PrintMessage(string message)
    {
        SC.ForegroundColor = ColorMuted;
        SC.WriteLine($"  {message}");
        SC.ResetColor();
    }

    public static void PrintError(string message)
    {
        SC.ForegroundColor = ColorError;
        SC.WriteLine($"  ✗  {message}");
        SC.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        SC.ForegroundColor = ColorSuccess;
        SC.WriteLine($"  ✓  {message}");
        SC.ResetColor();
    }

    public static void PrintFinalStandings(IEnumerable<(string Name, int Score)> standings, int totalRounds)
    {
        SC.WriteLine();
        SC.ForegroundColor = ColorHeading;
        SC.WriteLine("  ╔══════════════════════════════════════════════════════╗");
        SC.WriteLine("  ║               G A M E   O V E R                     ║");
        SC.WriteLine("  ╚══════════════════════════════════════════════════════╝");
        SC.ResetColor();
        SC.WriteLine($"\n  {totalRounds} round(s) played.\n");

        var ordered = standings.OrderByDescending(s => s.Score).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            string medal = i switch { 0 => "🥇 WINNER", 1 => "🥈", 2 => "🥉", _ => "   " };
            SC.ForegroundColor = i == 0 ? ColorScore : CC.White;
            SC.WriteLine($"    {medal}  {ordered[i].Name,-20} {ordered[i].Score,4} pts");
        }
        SC.ResetColor();
        SC.WriteLine();
    }

    // ── Input helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// True once stdin has reached end of input. Once set, the prompt helpers
    /// stop looping and fall back to a default rather than re-asking a stream
    /// that will never answer.
    /// </summary>
    public static bool InputEnded { get; private set; }

    /// <summary>Flags end of input from a renderer that reads stdin directly.</summary>
    public static void NoteInputEnded() => InputEnded = true;

    public static string Prompt(string message)
    {
        SC.ForegroundColor = ColorHeading;
        SC.Write($"  {message} ");
        SC.ResetColor();

        var line = SC.ReadLine();
        if (line is null) InputEnded = true;   // EOF, not an empty answer
        return line?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Prints a prompt marker WITHOUT reading — for a screen where the caller's
    /// own loop does the actual read afterwards.
    ///
    /// <c>Prompt(">")</c> used to be called for exactly this at three sites —
    /// the card-turn and Millionaire "ready" screens — with its return value
    /// discarded. <c>Prompt</c> blocks on <c>SC.ReadLine()</c> regardless of
    /// whether anyone uses what it reads, so every one of those screens
    /// consumed and silently threw away one real line of input before the
    /// actual command loop ever got a turn. In a piped/scripted run that just
    /// needs an extra blank line to compensate and the bug hides. At a real
    /// terminal a player sees "&gt;", types their command, presses Enter — and
    /// it vanishes. They have to type it a second time for anything to happen.
    /// </summary>
    public static void PrintPromptMarker(string message)
    {
        SC.ForegroundColor = ColorHeading;
        SC.Write($"  {message} ");
        SC.ResetColor();
    }

    public static int PromptInt(string message, int min, int max)
    {
        while (true)
        {
            var raw = Prompt($"{message} ({min}-{max}):");
            if (int.TryParse(raw, out var value) && value >= min && value <= max)
                return value;

            // EOF used to be indistinguishable from an empty line, so this loop
            // re-prompted a dead stream forever — the app could not be piped
            // input or smoke-tested without an external timeout.
            if (InputEnded)
            {
                PrintError($"Input ended; using {min}.");
                return min;
            }

            PrintError($"Please enter a number between {min} and {max}.");
        }
    }

    public static bool PromptYesNo(string message)
    {
        while (true)
        {
            var raw = Prompt($"{message} (y/n):").ToLowerInvariant();
            if (raw is "y" or "yes") return true;
            if (raw is "n" or "no") return false;

            // See PromptInt: at EOF, answering "no" is what lets a piped run
            // finish instead of spinning.
            if (InputEnded) return false;

            PrintError("Please enter y or n.");
        }
    }

    public static void PressEnterToContinue()
    {
        SC.ForegroundColor = ColorMuted;
        SC.Write("\n  Press ENTER to continue...");
        SC.ResetColor();
        if (SC.ReadLine() is null) InputEnded = true;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Pads or truncates to <paramref name="width"/> TERMINAL COLUMNS.
    ///
    /// This used to use <c>string.Length</c>, which counts UTF-16 code units.
    /// The decks are full of emoji, and an emoji is two code units but renders
    /// two columns wide — so the card border drifted, and <c>text[..width]</c>
    /// could cut a surrogate pair in half and emit a replacement glyph.
    /// </summary>
    private static string Pad(string text, int width)
    {
        var used = 0;
        var end = text.Length;

        // Walk grapheme clusters, so an emoji with a skin-tone or ZWJ sequence
        // counts once rather than per code point.
        var e = System.Globalization.StringInfo.GetTextElementEnumerator(text);
        while (e.MoveNext())
        {
            var cluster = (string)e.Current;
            var w = ClusterWidth(cluster);
            if (used + w > width) { end = e.ElementIndex; break; }
            used += w;
        }

        var kept = text[..end];
        return used >= width ? kept : kept + new string(' ', width - used);
    }

    /// <summary>
    /// Columns one grapheme cluster occupies. Two for emoji and East Asian wide
    /// characters, one otherwise. Zero-width joiners and variation selectors
    /// fall inside the cluster and so cost nothing extra.
    /// </summary>
    private static int ClusterWidth(string cluster)
    {
        if (cluster.Length == 0) return 0;

        var rune = System.Text.Rune.GetRuneAt(cluster, 0).Value;

        // Emoji and pictographs
        if (rune is >= 0x1F300 and <= 0x1FAFF) return 2;
        if (rune is >= 0x2600 and <= 0x27BF) return 2;   // misc symbols, dingbats
        if (rune is >= 0x1F000 and <= 0x1F2FF) return 2;   // mahjong, cards, enclosed
        // East Asian Wide / Fullwidth
        if (rune is >= 0x1100 and <= 0x115F) return 2;   // Hangul Jamo
        if (rune is >= 0x2E80 and <= 0xA4CF) return 2;   // CJK
        if (rune is >= 0xAC00 and <= 0xD7A3) return 2;   // Hangul syllables
        if (rune is >= 0xF900 and <= 0xFAFF) return 2;   // CJK compatibility
        if (rune is >= 0xFF00 and <= 0xFF60) return 2;   // fullwidth forms
        if (rune is >= 0xFFE0 and <= 0xFFE6) return 2;

        return 1;
    }

    /// <summary>Terminal columns a whole string occupies.</summary>
    private static int DisplayWidth(string text)
    {
        var total = 0;
        var e = System.Globalization.StringInfo.GetTextElementEnumerator(text);
        while (e.MoveNext()) total += ClusterWidth((string)e.Current);
        return total;
    }

    /// <summary>
    /// Word-wraps to <paramref name="width"/>, honouring line breaks already in
    /// the text.
    ///
    /// This used to split on spaces alone. Card text carries real newlines — the
    /// decks separate a prompt from its answer with one — and a newline inside a
    /// "word" was emitted inside a padded line, so the card border lost its left
    /// edge and the right edge landed in the wrong column. Blank lines are
    /// preserved, because the decks use them for spacing.
    /// </summary>
    private static IEnumerable<string> WrapText(string text, int width)
    {
        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (paragraph.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            var line = string.Empty;
            foreach (var word in paragraph.Split(' '))
            {
                if (DisplayWidth(line + word) > width)
                {
                    if (line.Length > 0) yield return line.TrimEnd();
                    line = string.Empty;
                }
                line += word + " ";
            }
            if (line.Length > 0) yield return line.TrimEnd();
        }
    }
}
