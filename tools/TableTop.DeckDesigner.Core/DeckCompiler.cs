using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Domain.Cards;

namespace TableTop.DeckDesigner.Core;

/// <summary>
/// Turns a <see cref="DeckDraft"/> into real <see cref="ICard"/>s by driving
/// the actual <see cref="CardDeckBuilder"/> — never a private re-implementation
/// of its rules. That is what guarantees the tool's preview, and the deck a
/// generated card bank builds at runtime, are byte-for-byte the same thing:
/// same ids (the deck name + category + title + description seed), same
/// validation (an empty deck name, a card with no category, two identical
/// this-or-that option labels all throw here exactly as they would in a
/// hand-written mode file), same everything.
/// </summary>
public static class DeckCompiler
{
    /// <summary>
    /// Validates <paramref name="deck"/> and, if valid, builds it through
    /// <see cref="CardDeckBuilder"/>. Never throws — every problem, whether
    /// caught here or surfaced by the builder itself, comes back as an entry
    /// in <see cref="DeckCompileResult.Errors"/>.
    /// </summary>
    public static DeckCompileResult Compile(DeckDraft deck)
    {
        ArgumentNullException.ThrowIfNull(deck);

        var errors = Validate(deck);
        if (errors.Count > 0)
            return DeckCompileResult.Failure(errors);

        try
        {
            var builder = CardDeckBuilder.For(deck.Name);
            string? currentCategory = null;

            foreach (var card in deck.Cards)
            {
                if (currentCategory != card.Category)
                {
                    builder.Category(card.Category);
                    currentCategory = card.Category;
                }

                if (card.IsThisOrThat)
                {
                    builder.ThisOrThatCard(
                        card.Title,
                        card.Description,
                        new ThisOrThatOption(card.OptionALabel, Detail: NullIfBlank(card.OptionADetail)),
                        new ThisOrThatOption(card.OptionBLabel, Detail: NullIfBlank(card.OptionBDetail)),
                        card.Difficulty);
                }
                else
                {
                    builder.Card(
                        card.Title,
                        card.Description,
                        card.Difficulty,
                        card.Tags.Count > 0 ? card.Tags : null);
                }
            }

            return DeckCompileResult.Success(builder.Build());
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // CardDeckBuilder's own guards (blank title, no Category(...) call
            // yet, duplicate this-or-that labels, ...) throw these two types —
            // surfaced as a compile error rather than left to crash the tool.
            return DeckCompileResult.Failure([ex.Message]);
        }
    }

    private static List<string> Validate(DeckDraft deck)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(deck.Name))
            errors.Add("Deck name is required.");

        if (deck.Cards.Count == 0)
            errors.Add("Add at least one card.");

        for (var i = 0; i < deck.Cards.Count; i++)
        {
            var card = deck.Cards[i];
            var label = $"Card {i + 1}";

            if (string.IsNullOrWhiteSpace(card.Category))
                errors.Add($"{label}: category is required.");
            if (string.IsNullOrWhiteSpace(card.Title))
                errors.Add($"{label}: title is required.");
            if (string.IsNullOrWhiteSpace(card.Description))
                errors.Add($"{label}: description is required.");

            if (card.IsThisOrThat &&
                (string.IsNullOrWhiteSpace(card.OptionALabel) || string.IsNullOrWhiteSpace(card.OptionBLabel)))
                errors.Add($"{label}: this-or-that cards need both option labels.");
        }

        return errors;
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
