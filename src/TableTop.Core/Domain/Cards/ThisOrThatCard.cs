using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A two-option comparison card, optionally illustrated on each side.
///
/// <para>
/// <see cref="BaseCard.Description"/> carries the question itself ("Which
/// would you rather?"), so a head with no <see cref="IThisOrThatCard"/>
/// awareness — Console, most obviously — still renders something coherent
/// rather than a blank card. The options are additive detail on top of a card
/// that already works without them.
/// </para>
/// </summary>
public sealed class ThisOrThatCard : BaseCard, IThisOrThatCard
{
    /// <inheritdoc />
    public ThisOrThatOption OptionA { get; }

    /// <inheritdoc />
    public ThisOrThatOption OptionB { get; }

    /// <summary>Initialises a new <see cref="ThisOrThatCard"/>.</summary>
    public ThisOrThatCard(
        Guid id,
        string title,
        string description,
        Difficulty difficulty,
        string category,
        ThisOrThatOption optionA,
        ThisOrThatOption optionB,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
        : base(id, title, description, difficulty, category, tags, restriction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(optionA.Label);
        ArgumentException.ThrowIfNullOrWhiteSpace(optionB.Label);

        // Two identical labels make the card unanswerable — a player cannot
        // say which they picked, and a head cannot show a meaningful result.
        if (string.Equals(optionA.Label, optionB.Label, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Both options are labelled '{optionA.Label}' — a this-or-that card needs two distinguishable choices.",
                nameof(optionB));

        OptionA = optionA;
        OptionB = optionB;
    }

    /// <summary>
    /// Convenience factory with a deterministic id derived from the card's
    /// content, matching <see cref="CardDeckBuilder"/>'s approach — so a saved
    /// session still resolves its cards after a restart.
    /// </summary>
    public static ThisOrThatCard Create(
        string deckName,
        string title,
        string description,
        Difficulty difficulty,
        string category,
        ThisOrThatOption optionA,
        ThisOrThatOption optionB)
    {
        var seed = $"{deckName}|{category}|{title}|{description}|{optionA.Label}|{optionB.Label}";
        var digest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return new ThisOrThatCard(new Guid(digest[..16]), title, description, difficulty, category, optionA, optionB);
    }
}
