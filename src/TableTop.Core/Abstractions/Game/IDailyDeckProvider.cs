using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Opt-in capability for modes that unlock exactly one card per real calendar
/// day rather than dealing a shuffled deck within a single sitting — an
/// advent-calendar campaign instead of a one-evening game.
///
/// A mode implementing this is routed by <c>ControllerFactory</c>
/// to <c>DayOneController</c>, exactly the
/// way <c>IQuestionBankProvider</c> routes to the Millionaire controller.
///
/// The returned deck is STRICTLY ORDERED: index 0 is Day 1, index 1 is Day 2,
/// and so on. There is no shuffling — the day sequence IS the design.
/// </summary>
public interface IDailyDeckProvider
{
    /// <summary>The ordered campaign deck; element N unlocks on day N+1.</summary>
    IReadOnlyList<ICard> GetDailyDeck();
}
