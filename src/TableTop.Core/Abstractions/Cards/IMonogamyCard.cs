namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card from the Monogamy game — a zone-based couples intimacy game.
/// Each card belongs to an intimacy zone, targets one or both partners,
/// and is worth a number of tokens when completed.
///
/// Extends <see cref="ICard"/> for full engine compatibility (LSP) while
/// exposing Monogamy-specific metadata renderers can use for theming.
/// </summary>
public interface IMonogamyCard : ICard
{
    /// <summary>The intimacy zone this card belongs to.</summary>
    MonogamyZone Zone { get; }

    /// <summary>Which partner(s) this card is directed at or involves.</summary>
    CardTarget Target { get; }

    /// <summary>Tokens awarded when this card is completed. Default: 1.</summary>
    int TokenValue { get; }

    /// <summary>
    /// Approximate time required to complete this card, in minutes.
    /// Null means no specific duration.
    /// </summary>
    int? DurationMinutes { get; }
}

/// <summary>
/// The four intimacy zones in the Monogamy game.
/// Each zone corresponds to a colour on the board and a level of intimacy.
/// </summary>
public enum MonogamyZone
{
    /// <summary>Playful, light, non-explicit. Entry level.</summary>
    Foreplay = 1,

    /// <summary>Romantic and sensual. Emotional connection focus.</summary>
    Sensual = 2,

    /// <summary>More intimate physical challenges. Adults only.</summary>
    Steamy = 3,

    /// <summary>Adventurous. For couples who want to push boundaries.</summary>
    Wild = 4,

    /// <summary>
    /// The deck's most explicit tier. Where Wild escalates what you <i>do</i>,
    /// Fantasy escalates what you're willing to <i>name</i> — voicing a
    /// scenario out loud and then enacting it.
    ///
    /// Deliberately the rarest zone by dice roll (11–12, so 3 in 36) and the
    /// only zone most tables will reach by *choosing* it on doubles rather
    /// than being sent there. That asymmetry is the design: the most exposing
    /// content should be opted into, not landed on.
    /// </summary>
    Fantasy = 5,
}

/// <summary>
/// Who a Monogamy card is directed at.
/// </summary>
public enum CardTarget
{
    /// <summary>The card is for the player who drew it to do to/for their partner.</summary>
    ForDrawer,

    /// <summary>The card is for the partner to do to/for the drawer.</summary>
    ForPartner,

    /// <summary>Both partners participate equally.</summary>
    ForBoth,

    /// <summary>The drawer chooses who performs the action.</summary>
    PlayerChoice,
}
