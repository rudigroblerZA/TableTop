using System.Security.Cryptography;
using System.Text;
using TableTop.Core.Domain.Analysis;

namespace TableTop.Games.Fun;

/// <summary>
/// The fifty statements Big Five plays, ten per dimension.
///
/// <para>
/// <b>Provenance.</b> These are original statements written for this mode. They
/// follow the standard shape of a public-domain Big Five item pool — short
/// self-descriptive sentences, half of them reverse-keyed — but none is
/// reproduced from a published inventory. That matters because the content in
/// this repo is compiled in and shipped, and "it was probably fine to copy" is
/// not a licence.
/// </para>
///
/// <para>
/// <b>Ten items per dimension, five keyed each way.</b> The split is what makes
/// a player who agrees with everything land at the midpoint rather than at the
/// top of all five dimensions — see <see cref="BigFiveMode"/>. The count is
/// what makes a single odd answer worth ten points rather than fifty: with two
/// items per trait, one careless tap moves a dimension by half its range.
/// </para>
///
/// <para>
/// Ids are derived from a SHA-256 of the statement text, so a card's identity is
/// stable across runs and rebuilds. <c>TraitProfileBuilder</c> keys responses by
/// card id, and a resumed session whose ids had been regenerated would silently
/// re-ask everything the player had already answered.
/// </para>
/// </summary>
public static class BigFiveItemBank
{
    /// <summary>All fifty items, grouped by dimension in OCEAN order.</summary>
    public static IReadOnlyList<TraitItemCard> All { get; } = Build();

    /// <summary>
    /// True when every dimension carries the same number of forward- and
    /// reverse-keyed items.
    ///
    /// <para>
    /// Exposed rather than merely asserted in a test because it is the property
    /// the mode's correctness rests on, and a future item added to the bank
    /// should fail a build rather than quietly tilt a dimension. Computed from
    /// the bank itself, so it cannot drift from the content the way a hand-typed
    /// count would.
    /// </para>
    /// </summary>
    public static bool IsBalanced =>
        All.SelectMany(c => c.TraitWeights)
           .GroupBy(w => w.Key, StringComparer.OrdinalIgnoreCase)
           .All(g => g.Count(w => w.Value > 0) == g.Count(w => w.Value < 0));

    private static TraitItemCard Item(string traitKey, string statement, bool reverse) =>
        new(DeterministicId(statement),
            statement,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                [traitKey] = reverse ? -1d : 1d,
            },
            traitKey);

    private static Guid DeterministicId(string statement)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes($"BigFive|{statement}"));
        return new Guid(digest[..16]);
    }

    private static IReadOnlyList<TraitItemCard> Build()
    {
        var o = BigFiveTraits.OpennessKey;
        var c = BigFiveTraits.ConscientiousnessKey;
        var e = BigFiveTraits.ExtraversionKey;
        var a = BigFiveTraits.AgreeablenessKey;
        var n = BigFiveTraits.NeuroticismKey;

        return
        [
            // ── Openness ──────────────────────────────────────────────────────
            Item(o, "I get pulled in by ideas that have no practical use whatsoever.", false),
            Item(o, "I would rather try an unfamiliar dish than order the thing I know is good.", false),
            Item(o, "I notice when a room, a song or a sentence has been put together well.", false),
            Item(o, "I enjoy conversations that wander somewhere neither of us planned.", false),
            Item(o, "I like things I don't immediately understand.", false),
            Item(o, "I would rather repeat a holiday I loved than gamble on somewhere new.", true),
            Item(o, "Abstract conversations lose me quickly.", true),
            Item(o, "I prefer a story with a clear ending to one that leaves things open.", true),
            Item(o, "I have little patience for theories that can't be put to use.", true),
            Item(o, "I stick to the same few things I already know I enjoy.", true),

            // ── Conscientiousness ─────────────────────────────────────────────
            Item(c, "I finish what I start, even after the interesting part is over.", false),
            Item(c, "I like knowing what the plan is before we leave the house.", false),
            Item(c, "I put things back where they belong without thinking about it.", false),
            Item(c, "People can rely on me to remember the boring details.", false),
            Item(c, "I would rather be early and wait than risk being late.", false),
            Item(c, "My good intentions comfortably outnumber my finished projects.", true),
            Item(c, "I leave things until the deadline forces me.", true),
            Item(c, "I lose track of small commitments.", true),
            Item(c, "My space is messier than I would like to admit.", true),
            Item(c, "I work out what I'm doing today somewhere around lunchtime.", true),

            // ── Extraversion ──────────────────────────────────────────────────
            Item(e, "A room full of people I don't know sounds like a good evening.", false),
            Item(e, "I tend to talk first and think about it afterwards.", false),
            Item(e, "I come away from a busy day out with more energy than I started with.", false),
            Item(e, "I'm usually the one who suggests going out.", false),
            Item(e, "I'm comfortable being the centre of attention.", false),
            Item(e, "After a big social event I need a day to myself.", true),
            Item(e, "I would rather have one long conversation than ten short ones.", true),
            Item(e, "In a group I'm happy to let other people do the talking.", true),
            Item(e, "Making small talk with strangers tires me out.", true),
            Item(e, "My ideal weekend is a quiet one.", true),

            // ── Agreeableness ─────────────────────────────────────────────────
            Item(a, "I would rather lose an argument than have it turn cold.", false),
            Item(a, "I notice quickly when someone in the room has gone quiet.", false),
            Item(a, "I assume people mean well until they show me otherwise.", false),
            Item(a, "I go out of my way to make things easier for people.", false),
            Item(a, "Someone else having a bad day affects mine.", false),
            Item(a, "I say what I think even when it lands badly.", true),
            Item(a, "I find it easy to say no without feeling guilty about it.", true),
            Item(a, "I'm sceptical of people's motives until I know them.", true),
            Item(a, "I would rather be right than keep the peace.", true),
            Item(a, "Other people's problems are not mine to carry.", true),

            // ── Sensitivity (Neuroticism) ─────────────────────────────────────
            Item(n, "I replay conversations afterwards, looking for what I got wrong.", false),
            Item(n, "A small setback can colour my whole day.", false),
            Item(n, "I worry about things that turn out completely fine.", false),
            Item(n, "My mood can turn on something minor.", false),
            Item(n, "Criticism stays with me longer than it was meant to.", false),
            Item(n, "I stay level when things go wrong around me.", true),
            Item(n, "I let go of embarrassment quickly.", true),
            Item(n, "It takes a lot to rattle me.", true),
            Item(n, "I sleep fine the night before something big.", true),
            Item(n, "I rarely feel tense.", true),
        ];
    }
}
