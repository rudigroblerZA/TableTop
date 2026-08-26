using TableTop.Core.Abstractions.Cards;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls the Who Wants to Be a Millionaire? hot-seat game loop.
/// The UI calls answer/walk-away/lifeline methods; the controller raises events for every state change.
/// </summary>
public interface IMillionaireController : IGameController
{
    /// <summary>HotSeatBegan.</summary>
    event EventHandler<HotSeatBeganEvent>       HotSeatBegan;
    /// <summary>QuestionReady.</summary>
    event EventHandler<QuestionReadyEvent>      QuestionReady;
    /// <summary>LifelineUsed.</summary>
    event EventHandler<LifelineUsedEvent>       LifelineUsed;
    /// <summary>AnswerCorrect.</summary>
    event EventHandler<AnswerCorrectEvent>      AnswerCorrect;
    /// <summary>AnswerWrong.</summary>
    event EventHandler<AnswerWrongEvent>        AnswerWrong;
    /// <summary>WalkedAway.</summary>
    event EventHandler<WalkedAwayEvent>         WalkedAway;
    /// <summary>MillionaireWon.</summary>
    event EventHandler<MillionaireWonEvent>     MillionaireWon;
    /// <summary>GameEnded.</summary>
    event EventHandler<MillionaireGameEndedEvent> GameEnded;

    /// <summary>Starts the first player's hot-seat run.</summary>
    void Start();

    /// <summary>
    /// Submits an answer for the current question.
    /// Raises <see cref="AnswerCorrectEvent"/> or <see cref="AnswerWrongEvent"/>.
    /// If correct and not the final question, automatically loads the next question.
    /// </summary>
    void SubmitAnswer(AnswerLabel label);

    /// <summary>Banks the current rung prize and ends this player's run.</summary>
    void WalkAway();

    /// <summary>Activates a lifeline by index (0 = 50:50, 1 = Phone, 2 = Audience).</summary>
    void UseLifeline(int index);

    /// <summary>Current answer labels still available (affected by 50:50).</summary>
    IReadOnlyList<AnswerLabel> AvailableOptions { get; }
}