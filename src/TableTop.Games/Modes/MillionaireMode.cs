using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games;

/// <summary>
/// Who Wants to Be a Millionaire? game mode identity.
///
/// Questions come from the in-code <see cref="MillionaireQuestionBank"/>. They were
/// read from <c>Data/Json/millionaire-questions.deck.json</c> first until 1.19.0,
/// with the bank as a fallback; the deck files and the resolver are both gone.
/// </summary>
public sealed class MillionaireMode : IGameMode, IQuestionBankProvider
{
    /// <inheritdoc />
    public string Name => "Who Wants to Be a Millionaire?";

    /// <inheritdoc />
    public string Description =>
        "Hot-seat quiz. Climb 15 questions to £1,000,000. Three lifelines. One wrong answer ends your run.";

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => MillionaireQuestionBank.All;
}
