namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card presenting exactly two options side by side, each optionally
/// illustrated, with detail revealed once an option is chosen.
///
/// <para>
/// Extends <see cref="ICard"/> so every existing piece of engine machinery —
/// progression, restrictions, scoring, persistence — handles it transparently
/// without knowing it's different (LSP). A head that doesn't understand this
/// interface still renders <see cref="ICard.Title"/> and
/// <see cref="ICard.Description"/> and produces a playable, if plainer, card.
/// That degradation is deliberate: Console has no way to show an image, and
/// should not be broken by a deck that contains one.
/// </para>
///
/// <para>
/// <b>Images are referenced, never embedded.</b> <see cref="ThisOrThatOption.ImageKey"/>
/// is a logical name, not a path or a URL — heads resolve it against their own
/// asset store. That keeps decks portable across platforms whose asset
/// conventions differ completely (Android resources, WinUI content files), and
/// keeps deck JSON small enough to stay diffable, which base64 payloads would
/// destroy immediately.
/// </para>
///
/// <para>
/// An option with no <see cref="ThisOrThatOption.ImageKey"/> is entirely
/// valid — the mechanic works as pure text, and a deck can mix illustrated and
/// text-only cards freely.
/// </para>
/// </summary>
public interface IThisOrThatCard : ICard
{
    /// <summary>The left-hand option.</summary>
    ThisOrThatOption OptionA { get; }

    /// <summary>The right-hand option.</summary>
    ThisOrThatOption OptionB { get; }

    /// <summary>
    /// True when both options carry an image key — the case a head can render
    /// as a genuine side-by-side picture comparison rather than two labels.
    /// </summary>
    bool IsFullyIllustrated => OptionA.ImageKey is not null && OptionB.ImageKey is not null;

    /// <summary>Returns the chosen option.</summary>
    ThisOrThatOption OptionFor(ThisOrThatChoice choice) =>
        choice == ThisOrThatChoice.A ? OptionA : OptionB;
}

/// <summary>Which side of a <see cref="IThisOrThatCard"/> was chosen.</summary>
public enum ThisOrThatChoice
{
    /// <summary>The left-hand option.</summary>
    A,

    /// <summary>The right-hand option.</summary>
    B,
}

/// <summary>
/// One side of a <see cref="IThisOrThatCard"/>.
/// </summary>
/// <param name="Label">
/// Short name shown under or beside the image — what a player says out loud
/// when picking. Always present, including on illustrated cards, so the card
/// still works for anyone who can't see the image well.
/// </param>
/// <param name="ImageKey">
/// Logical asset name (e.g. <c>"tot-beach"</c>), resolved by each head against
/// its own asset store. Null for a text-only option.
/// </param>
/// <param name="Detail">
/// Revealed after a choice is made — the payoff. Null when picking is the
/// whole interaction and there's nothing further to say.
/// </param>
public readonly record struct ThisOrThatOption(string Label, string? ImageKey = null, string? Detail = null)
{
    /// <summary>True when this option has detail to reveal after it's chosen.</summary>
    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    /// <summary>True when this option is illustrated.</summary>
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageKey);
}
