using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.School;

/// <summary>
/// Who Wants to Be a Millionaire? pitched at Grade 6 (age 11–12).
///
/// Questions come from the in-code <see cref="TableTop.Games.School.Grade6QuestionBank"/>.
/// They were read from <c>Data/Json/grade6-questions.deck.json</c> first until 1.19.0,
/// with the bank as a fallback; the deck files and the resolver are both gone.
/// </summary>
public sealed class SchoolMillionaireMode : IGameMode, IQuestionBankProvider
{
    /// <inheritdoc />
    public string Name => "School Millionaire (Grade 6)";
    /// <inheritdoc />
    public string Description =>
        "Who Wants to Be a Millionaire? with curriculum-based Grade 6 questions. " +
        "English, Maths, Science, History and Geography.";

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => Grade6QuestionBank.All;
}