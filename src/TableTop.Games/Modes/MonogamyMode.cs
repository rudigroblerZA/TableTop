using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;
using TableTop.Games.Data;   // MonogamyCardBank

namespace TableTop.Games;

/// <summary>
/// Monogamy — a couples intimacy card game.
/// Four zones of increasing intimacy: Foreplay → Sensual → Steamy → Wild.
///
/// Cards come from the in-code <see cref="MonogamyCardBank"/>. They were
/// loaded from <c>Data/Json/monogamy.deck.json</c> first until 1.19.0, with the
/// bank as a fallback; the deck files were removed in 1.18.0 and the loader in
/// 1.19.0, so the bank is now the only source.
///
/// Supplies its own deck via <see cref="IMonogamyDeckProvider"/>.
/// </summary>
public sealed class MonogamyMode : IGameMode, IMonogamyDeckProvider
{
    /// <inheritdoc />
    public string Name        => "Monogamy";
    /// <inheritdoc />
    public string Description =>
        "A couples intimacy game. Dice roll your zone — Foreplay, Sensual, Steamy, or Wild.";

    /// <inheritdoc />
    public IReadOnlyList<MonogamyCard> GetDeck() => MonogamyCardBank.All;

    /// <inheritdoc />
    public int? WinningTokenCount => 10;
}