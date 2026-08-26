using TableTop.Core.Domain.Cards;

namespace TableTop.Games.Data;

/// <summary>
/// Monogamy Extended — Now consolidated into MonogamyCardBank.
/// This class exists for backward compatibility only.
/// </summary>
public static class MonogamyCardBankExtended
{
    /// <summary>The full deck (all cards). Now consolidated in MonogamyCardBank.</summary>
    public static IReadOnlyList<MonogamyCard> FullDeck => MonogamyCardBank.All;

    /// <summary>All extended cards are now in MonogamyCardBank.All. Use that instead.</summary>
    public static IReadOnlyList<MonogamyCard> All => MonogamyCardBank.All;
}