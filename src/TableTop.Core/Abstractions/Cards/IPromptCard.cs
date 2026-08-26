using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card whose prompt text is resolved at draw time rather than fixed at authoring time.
/// Extends <see cref="ICard"/> so all existing rules, restrictions, and engines
/// handle it transparently (LSP).
/// </summary>
public interface IPromptCard : ICard
{
    /// <summary>
    /// Returns the text to display for the given player.
    /// May differ by gender, attribute, locale, or any other player property.
    /// </summary>
    string ResolvePrompt(IPlayer player);
}
