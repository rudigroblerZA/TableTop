using TableTop.Core.Domain.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Implemented by game modes that supply their own multiple-choice question bank
/// (e.g. Millionaire-style quizzes). Lets the hosting factory obtain the correct
/// questions <i>from the mode</i> rather than reaching into a specific static
/// card-bank class — keeping hosting ignorant of game content (DIP, OCP).
/// </summary>
public interface IQuestionBankProvider
{
    /// <summary>
    /// The ordered question bank this mode plays with.
    /// The controller is responsible for laddering / shuffling as appropriate.
    /// </summary>
    IReadOnlyList<MultipleChoiceCard> GetQuestionBank();
}
