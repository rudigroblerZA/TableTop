using System.Text.Json;

namespace TableTop.Tests;

/// <summary>
/// Keeps each deck consistent about what <c>title</c> means (backlog H.3).
///
/// The field carries two different jobs across the library:
///
///   • <b>title-style</b> — a real card title, distinct per card
///     (<c>monogamy</c>: "Slow Kiss", "The Photograph")
///   • <b>label-style</b> — a category the card belongs to, repeated across many
///     cards (<c>act-it-out</c>: 25 cards under 5 labels)
///
/// Measured across the current library: 41 decks pure title-style, 42 pure
/// label-style, and 5 that mix — but four of those five are title-style decks
/// with a handful of duplicate card names, not a modelling problem
/// (<c>monogamy</c> is 100 distinct titles and 3 repeats over 106 cards).
///
/// That split across decks is a deliberate content decision and this test does
/// not object to it. What it catches is a single deck drifting to genuinely
/// half-and-half, which is where a UI can no longer decide whether to render
/// <c>title</c> as a card header or a category chip — and the point at which the
/// field stops meaning anything.
///
/// <b>When this fails</b>, pick a convention for that deck. If the labels are
/// the real intent, the <c>category</c> field already exists on most cards and
/// is the honest home for them.
/// </summary>
public sealed class DeckTitleConventionTests
{
    /// <summary>
    /// Share of a deck's cards that must sit on the dominant side. 0.85 passes
    /// every deck in the library today, including the five with a few duplicate
    /// names, while failing anything approaching a 50/50 split.
    /// </summary>
    private const double DominantShareRequired = 0.85;

    /// <summary>Below this, the sample is too small for the ratio to mean much.</summary>
    private const int MinimumCardsToJudge = 8;

    /// <summary>Returns a mode's cards, or null when it can't produce them without a live session.</summary>
    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard>? SafeCards(
        TableTop.Core.Abstractions.Game.IGameMode mode)
    {
        // GetCards lives on IGameModeDefinition, not IGameMode; a handful of
        // modes (Millionaire, Monogamy, Claimed!, Day One) implement only the
        // latter and supply cards through their own capability interface.
        // Those are skipped rather than failed — the title convention is a
        // property of card decks, and they don't have one in this sense.
        if (mode is not TableTop.Core.Abstractions.Game.IGameModeDefinition def) return null;
        try { return def.GetCards([]); }
        catch { return null; }
    }

    [Fact]
    public void Every_deck_uses_title_consistently_as_a_name_or_as_a_label()
    {
        // Reads decks from the modes rather than from Data/Json/*.deck.json,
        // which was removed in 1.18.0. The invariant is unchanged and still
        // worth guarding — 'title' carries the same two meanings in the in-code
        // banks that it did in JSON, so a deck can still drift half-and-half.
        // Only the source moved.
        var offenders = new List<string>();
        var judged = 0;

        var decks = TableTop.Hosting.ArchetypeRegistry.Default()
            .AllModes
            .DistinctBy(m => m.Name)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .Select(m => new { m.Name, Cards = SafeCards(m) })
            .Where(d => d.Cards is not null);

        foreach (var deck in decks)
        {
            var deckName = deck.Name;
            var titles = deck.Cards!.Select(c => c.Title).ToList();
            if (titles.Count < MinimumCardsToJudge) continue;
            judged++;

            var groups = titles.GroupBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();

            // Cards whose title is theirs alone, versus cards sharing a title.
            var cardsWithOwnTitle = groups.Count(g => g.Count() == 1);
            var cardsUnderSharedName = groups.Where(g => g.Count() > 1).Sum(g => g.Count());
            var total = cardsWithOwnTitle + cardsUnderSharedName;
            if (total == 0) continue;

            var dominant = Math.Max(cardsWithOwnTitle, cardsUnderSharedName) / (double)total;

            if (dominant < DominantShareRequired)
            {
                offenders.Add(
                    $"{deckName}: {cardsWithOwnTitle} cards with their own title, " +
                    $"{cardsUnderSharedName} sharing one — {dominant:P0} on the dominant side, " +
                    $"below {DominantShareRequired:P0}");
            }
        }

        judged.Should().BeGreaterThan(50,
            "most of the library should be large enough to judge; near-zero means the deck files moved " +
            "or the schema changed, not that everything passes");

        offenders.Should().BeEmpty(
            "a deck should use 'title' as a card name or as a category label, not half of each — a UI " +
            "cannot decide whether to render it as a header or a chip. If the labels are the intent, the " +
            $"'category' field is the honest home for them.\n  {string.Join("\n  ", offenders)}");
    }

    private static List<string> ReadTitles(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (!doc.RootElement.TryGetProperty("cards", out var cards))
            return [];

        return cards.EnumerateArray()
                    .Select(c => c.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String
                        ? (t.GetString() ?? string.Empty).Trim()
                        : string.Empty)
                    .Where(t => t.Length > 0)
                    .ToList();
    }

    private static string FindGamesSourceDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "TableTop.Games");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate 'src/TableTop.Games' from '{AppContext.BaseDirectory}'.");
    }
}
