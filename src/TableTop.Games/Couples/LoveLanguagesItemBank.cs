using System.Security.Cryptography;
using System.Text;
using TableTop.Core.Domain.Analysis;

namespace TableTop.Games.Couples;

/// <summary>
/// The forty statements Love Languages plays, eight per language.
///
/// <para>
/// <b>Four forward and four reverse per language, for the same reason Big Five
/// is five and five.</b> Acquiescence bias matters even more here than there. A
/// ranking is the entire output of this mode, and a player who agrees warmly
/// with every statement on an all-positive bank scores high on all five — which
/// is not a flattering result, it is *no result at all*, because there is no
/// top language left to name. Reverse-keyed items are what make the ranking mean
/// something. <c>LoveLanguagesItemBank.IsBalanced</c> computes the split from the
/// bank so an added statement fails a test rather than quietly tilting a language.
/// </para>
///
/// <para>
/// Reverse items are phrased as a genuine other-preference — "I'd rather have
/// someone's company than their help" — rather than as a negation of the
/// language. "I don't like gifts" is a weaker item than "I'd honestly rather
/// someone didn't buy me anything": the first invites disagreement from anyone
/// polite, the second describes a real person.
/// </para>
///
/// <para>
/// Written to stay comfortable for a teen rating. Physical Touch is everyday
/// affection — hugs, sitting close, a hand on your back — because the language
/// is about how contact reads, not about sex. The adult register lives in the
/// modes filed under it.
/// </para>
///
/// <para>
/// Ids are a SHA-256 of the statement, so a card's identity survives a rebuild
/// and a resumed session does not re-ask what has been answered.
/// </para>
/// </summary>
public static class LoveLanguagesItemBank
{
    /// <summary>All forty items, grouped by language.</summary>
    public static IReadOnlyList<TraitItemCard> All { get; } = Build();

    /// <summary>
    /// True when every language carries the same number of forward- and
    /// reverse-keyed items. See the remarks above for why this one is
    /// load-bearing rather than tidy.
    /// </summary>
    public static bool IsBalanced =>
        All.SelectMany(c => c.TraitWeights)
           .GroupBy(w => w.Key, StringComparer.OrdinalIgnoreCase)
           .All(g => g.Count(w => w.Value > 0) == g.Count(w => w.Value < 0));

    private static TraitItemCard Item(string languageKey, string statement, bool reverse) =>
        new(DeterministicId(statement),
            statement,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                [languageKey] = reverse ? -1d : 1d,
            },
            languageKey);

    private static Guid DeterministicId(string statement)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes($"LoveLanguages|{statement}"));
        return new Guid(digest[..16]);
    }

    private static IReadOnlyList<TraitItemCard> Build()
    {
        var w = LoveLanguages.WordsKey;
        var s = LoveLanguages.ServiceKey;
        var g = LoveLanguages.GiftsKey;
        var q = LoveLanguages.TimeKey;
        var p = LoveLanguages.TouchKey;

        return
        [
            // ── Words of Affirmation ──────────────────────────────────────────
            Item(w, "Being told exactly what someone appreciates about me stays with me for days.", false),
            Item(w, "A message out of nowhere saying something kind can turn my whole week around.", false),
            Item(w, "I would rather hear \"I'm proud of you\" than be taken out to celebrate.", false),
            Item(w, "When someone thanks me properly, in words, I feel genuinely seen.", false),
            Item(w, "Compliments slide off me; I never quite know what to do with them.", true),
            Item(w, "Being told I'm appreciated matters less to me than being shown it.", true),
            Item(w, "I would take a quiet gesture over anything said out loud.", true),
            Item(w, "Praise makes me more uncomfortable than it makes me happy.", true),

            // ── Acts of Service ───────────────────────────────────────────────
            Item(s, "Someone quietly handling a job I had been dreading feels like real affection.", false),
            Item(s, "I notice straight away when somebody has done something to make my day easier.", false),
            Item(s, "\"Let me take care of that\" is one of my favourite things to hear.", false),
            Item(s, "Practical help lands harder for me than almost anything else.", false),
            Item(s, "Someone doing a task for me is just a task done; it doesn't read as affection.", true),
            Item(s, "I would rather have someone's company than their help.", true),
            Item(s, "Being helped can make me feel more managed than cared for.", true),
            Item(s, "I don't read much into who ends up doing the washing up.", true),

            // ── Receiving Gifts ───────────────────────────────────────────────
            Item(g, "A small thing someone picked out because it made them think of me means a lot.", false),
            Item(g, "I keep things people have given me long past their usefulness.", false),
            Item(g, "Being brought something back from a trip tells me I was on their mind.", false),
            Item(g, "The thought behind a present reaches me more than most gestures do.", false),
            Item(g, "Presents make me slightly awkward, however well chosen.", true),
            Item(g, "I would honestly rather someone didn't buy me anything.", true),
            Item(g, "An object is an object; it doesn't tell me much about how someone feels.", true),
            Item(g, "I forget what people have given me fairly quickly.", true),

            // ── Quality Time ──────────────────────────────────────────────────
            Item(q, "An unhurried evening with someone's full attention is my favourite kind.", false),
            Item(q, "I would rather have one undistracted hour than a whole day half-there.", false),
            Item(q, "Being asked to do something together, just the two of us, is what I look forward to.", false),
            Item(q, "Phones down and properly talking is when I feel closest to someone.", false),
            Item(q, "I'm content in the same room as someone without either of us paying much attention.", true),
            Item(q, "Long stretches of one-on-one time can feel like a lot.", true),
            Item(q, "I don't especially need someone's undivided attention to feel close to them.", true),
            Item(q, "I would rather we each did our own thing and met in the middle.", true),

            // ── Physical Touch ────────────────────────────────────────────────
            Item(p, "A hand on my back says more to me than most sentences.", false),
            Item(p, "I gravitate to sitting close to the people I'm fond of.", false),
            Item(p, "A proper hug resets something in me.", false),
            Item(p, "Everyday affection — a squeeze of the arm, a bumped shoulder — is how I know we're good.", false),
            Item(p, "I need a fair amount of physical space, even with people I love.", true),
            Item(p, "Being touched, however kindly, isn't how I read affection.", true),
            Item(p, "I'm not much of a hugger.", true),
            Item(p, "Closeness for me is about words and time far more than contact.", true),
        ];
    }
}
