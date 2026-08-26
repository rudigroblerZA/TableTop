using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// Monogamy's dice-to-zone mapping. Was <c>DiceRoll.ToZone()</c> — moved here
/// so the shared <see cref="DiceRoll"/> record carries no knowledge of any
/// specific mode's enum. See <see cref="DiceRoll"/>'s remarks for why.
/// </summary>
public static class MonogamyDiceMapping
{
    /// <summary>
    /// Maps a dice total to a Monogamy zone:
    /// 2–4   → Foreplay  (playful, light)   — 6 in 36
    /// 5–6   → Sensual   (romantic)         — 9 in 36
    /// 7–8   → Steamy    (intimate)         — 11 in 36
    /// 9–10  → Wild      (adventurous)      — 7 in 36
    /// 11–12 → Fantasy   (most explicit)    — 3 in 36
    ///
    /// <para>
    /// Fantasy is deliberately the rarest outcome. Adding a fifth zone meant
    /// re-cutting the whole 2–12 range rather than appending to it, and the
    /// obvious split would have handed Fantasy the same share as everything
    /// else. The deck's most exposing content shouldn't be something a table
    /// gets *sent* to one roll in five — it should be somewhere they mostly
    /// arrive by choosing it on doubles. The curve does that: Steamy stays the
    /// centre of gravity, Fantasy sits at the tail.
    /// </para>
    /// </summary>
    public static MonogamyZone ToZone(this DiceRoll roll) => roll.Total switch
    {
        <= 4  => MonogamyZone.Foreplay,
        <= 6  => MonogamyZone.Sensual,
        <= 8  => MonogamyZone.Steamy,
        <= 10 => MonogamyZone.Wild,
        _     => MonogamyZone.Fantasy,
    };
}
