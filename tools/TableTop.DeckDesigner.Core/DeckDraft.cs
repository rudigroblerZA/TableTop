namespace TableTop.DeckDesigner.Core;

/// <summary>
/// A deck in progress: its name (which seeds every card's id — see
/// <see cref="TableTop.Core.Domain.Cards.CardDeckBuilder.For(string)"/>) plus
/// its cards in authoring order. Order matters: <see cref="DeckCompiler"/>
/// starts a new <c>.Category(...)</c> call each time consecutive cards'
/// categories differ, exactly like hand-written <c>CardDeckBuilder</c> chains
/// do, so cards for the same category should be kept together.
/// </summary>
public sealed class DeckDraft
{
    /// <summary>The deck's name. Also seeds every card's stable id.</summary>
    public string Name { get; set; } = "";

    /// <summary>Cards in authoring order.</summary>
    public List<CardDraft> Cards { get; } = [];
}
