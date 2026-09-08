using TableTop.Core.Abstractions.Cards;

namespace TableTop.DeckDesigner.Core;

/// <summary>
/// One card as authored in the Deck Designer, before it has been compiled
/// through <see cref="TableTop.Core.Domain.Cards.CardDeckBuilder"/> — plain
/// data, no engine or UI dependency, so it can be built up field-by-field as
/// the user types.
/// </summary>
public sealed class CardDraft
{
    /// <summary>Category the card belongs to. Cards are compiled in list order, and a category change here starts a new <c>.Category(...)</c> call.</summary>
    public string Category { get; set; } = "";

    /// <summary>Card title.</summary>
    public string Title { get; set; } = "";

    /// <summary>Card body. For a this-or-that card, this is the question shown above the two options.</summary>
    public string Description { get; set; } = "";

    /// <summary>Difficulty tier.</summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;

    /// <summary>Free-form tags. Empty means the card carries none.</summary>
    public IReadOnlyList<string> Tags { get; set; } = [];

    /// <summary>True to compile this as a <see cref="TableTop.Core.Domain.Cards.ThisOrThatCard"/> instead of a <see cref="TableTop.Core.Domain.Cards.StandardCard"/>.</summary>
    public bool IsThisOrThat { get; set; }

    /// <summary>First option's label. Required when <see cref="IsThisOrThat"/> is true.</summary>
    public string OptionALabel { get; set; } = "";

    /// <summary>First option's revealed detail. Optional.</summary>
    public string? OptionADetail { get; set; }

    /// <summary>Second option's label. Required when <see cref="IsThisOrThat"/> is true.</summary>
    public string OptionBLabel { get; set; } = "";

    /// <summary>Second option's revealed detail. Optional.</summary>
    public string? OptionBDetail { get; set; }
}
