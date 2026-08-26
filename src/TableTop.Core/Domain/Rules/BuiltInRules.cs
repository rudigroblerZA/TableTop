using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;

namespace TableTop.Core.Domain.Rules;

/// <summary>
/// Evaluates a card's restriction against the current player.
/// Denies the card if the player does not satisfy the restriction.
/// </summary>
public sealed class RestrictionRule : IRule
{
    /// <inheritdoc />
    public string Name => "RestrictionRule";

    /// <inheritdoc />
    public string Description => "Denies the card when the player does not satisfy the card's restriction.";

    /// <inheritdoc />
    public RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context)
    {
        if (card.Restriction is null)
            return RuleResult.Allow();

        var allowed = card.Restriction.IsSatisfiedBy(player, context.Players);
        return allowed
            ? RuleResult.Allow()
            : RuleResult.Deny($"Player '{player.DisplayName}' does not satisfy: {card.Restriction.Description}");
    }
}

/// <summary>
/// Prevents a player from receiving the same card twice in a session.
/// </summary>
public sealed class NoDuplicateCardRule : IRule
{
    /// <inheritdoc />
    public string Name => "NoDuplicateCardRule";

    /// <inheritdoc />
    public string Description => "Prevents the same card from being dealt to a player more than once.";

    /// <inheritdoc />
    public RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context)
    {
        var alreadyPlayed = context.Metadata.HasCardBeenPlayedBy(player.Id, card.Id);
        return alreadyPlayed
            ? RuleResult.Deny($"Card '{card.Title}' has already been played by '{player.DisplayName}'.")
            : RuleResult.Allow();
    }
}

/// <summary>
/// Awards a score multiplier for hard or extreme cards.
/// Does not block play — only modifies score delta.
/// </summary>
public sealed class DifficultyScoreRule : IRule
{
    /// <inheritdoc />
    public string Name => "DifficultyScoreRule";

    /// <inheritdoc />
    public string Description => "Awards bonus score points based on card difficulty.";

    /// <inheritdoc />
    public RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context)
    {
        var bonus = card.Difficulty switch
        {
            Abstractions.Cards.Difficulty.Easy => 0,
            Abstractions.Cards.Difficulty.Medium => 1,
            Abstractions.Cards.Difficulty.Hard => 1,
            Abstractions.Cards.Difficulty.Extreme => 2,
            _ => 0
        };
        return RuleResult.Allow(scoreDelta: bonus);
    }
}

/// <summary>
/// Skips a player when their status is <see cref="PlayerStatus.Skipped"/>.
/// </summary>
public sealed class SkipPlayerRule : IRule
{
    /// <inheritdoc />
    public string Name => "SkipPlayerRule";

    /// <inheritdoc />
    public string Description => "Denies card selection for players in Skipped status.";

    /// <inheritdoc />
    public RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context) =>
        player.Status == PlayerStatus.Skipped
            ? RuleResult.Deny($"Player '{player.DisplayName}' is currently skipped.")
            : RuleResult.Allow();
}
