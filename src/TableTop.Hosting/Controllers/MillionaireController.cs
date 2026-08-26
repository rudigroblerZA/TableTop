using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Game;
using TableTop.Core.Domain.Lifelines;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using TableTop.Games;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives the Who Wants to Be a Millionaire? hot-seat loop.
/// Has no knowledge of any UI.
/// </summary>
public sealed class MillionaireController : IMillionaireController
{
    private readonly IReadOnlyList<IPlayer>   _players;
    private readonly Dictionary<Guid, long>   _bankedPrizes;

    private List<MultipleChoiceCard> _questionPool = [];
    private PrizeLadder              _ladder       = new();
    private List<Core.Domain.Lifelines.FiftyFiftyLifeline>      _lifelineList = [];
    private List<Core.Abstractions.Lifelines.ILifeline>          _lifelines    = [];
    private List<AnswerLabel>        _activeOptions = [];
    private MultipleChoiceCard?      _currentQuestion;
    private int                      _hotSeatIndex;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>HotSeatBegan.</summary>
    public event EventHandler<HotSeatBeganEvent>?         HotSeatBegan;
    /// <summary>QuestionReady.</summary>
    public event EventHandler<QuestionReadyEvent>?        QuestionReady;
    /// <summary>LifelineUsed.</summary>
    public event EventHandler<LifelineUsedEvent>?         LifelineUsed;
    /// <summary>AnswerCorrect.</summary>
    public event EventHandler<AnswerCorrectEvent>?        AnswerCorrect;
    /// <summary>AnswerWrong.</summary>
    public event EventHandler<AnswerWrongEvent>?          AnswerWrong;
    /// <summary>WalkedAway.</summary>
    public event EventHandler<WalkedAwayEvent>?           WalkedAway;
    /// <summary>MillionaireWon.</summary>
    public event EventHandler<MillionaireWonEvent>?       MillionaireWon;
    /// <summary>GameEnded.</summary>
    public event EventHandler<MillionaireGameEndedEvent>? GameEnded;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <summary>AvailableOptions.</summary>
    public IReadOnlyList<AnswerLabel> AvailableOptions => _activeOptions.AsReadOnly();

    private readonly IReadOnlyList<MultipleChoiceCard>? _customBank;

    /// <param name="players">Players in the session.</param>
    /// <param name="questionBank">
    /// Optional custom question bank. When null, uses <see cref="MillionaireQuestionBank.All"/>.
    /// Pass <see cref="TableTop.Games.School.Grade6QuestionBank.All"/> for the school edition.
    /// </param>
    public MillionaireController(
        IReadOnlyList<IPlayer>              players,
        IReadOnlyList<MultipleChoiceCard>?  questionBank = null)
    {
        _players      = players;
        _customBank   = questionBank;
        _bankedPrizes = players.ToDictionary(p => p.Id, _ => 0L);
    }

    // ── IMillionaireController ────────────────────────────────────────────────

    /// <inheritdoc />
    public void Start()
    {
        IsRunning = true;
        BeginHotSeat();
    }

    /// <summary>Submits the player's answer for the current question. Advances the ladder on correct or ends the game on incorrect.</summary>
    public void SubmitAnswer(AnswerLabel label)
    {
        if (_currentQuestion is null || !_activeOptions.Contains(label)) return;

        if (_currentQuestion.IsCorrect(label))
        {
            _ladder.Advance();

            if (_ladder.IsComplete)
            {
                var player = _players[_hotSeatIndex];
                _bankedPrizes[player.Id] = 1_000_000;
                MillionaireWon?.Invoke(this, new MillionaireWonEvent(player.DisplayName));
                AdvanceToNextPlayer();
                return;
            }

            var rung = _ladder.Rungs[_ladder.CurrentRungIndex - 1];
            AnswerCorrect?.Invoke(this, new AnswerCorrectEvent(
                PrizeWon:         rung.PrizeAmount,
                SafeHavenReached: _ladder.CurrentRung.IsSafeHaven,
                GuaranteedPrize:  _ladder.GuaranteedPrize,
                Ladder:           BuildLadderSnapshot()));

            LoadNextQuestion();
        }
        else
        {
            var guaranteed = _ladder.GuaranteedPrize;
            _bankedPrizes[_players[_hotSeatIndex].Id] = guaranteed;
            AnswerWrong?.Invoke(this, new AnswerWrongEvent(
                CorrectLabel:    _currentQuestion.CorrectAnswer,
                CorrectText:     _currentQuestion.Answers[_currentQuestion.CorrectAnswer],
                GuaranteedPrize: guaranteed));
            AdvanceToNextPlayer();
        }
    }

    /// <summary>The player takes their current prize money and exits the hot seat.</summary>
    public void WalkAway()
    {
        var rung  = _ladder.CurrentRung;
        var prize = rung.PrizeAmount;
        _bankedPrizes[_players[_hotSeatIndex].Id] = prize;
        WalkedAway?.Invoke(this, new WalkedAwayEvent(prize));
        AdvanceToNextPlayer();
    }

    /// <summary>Activates the lifeline at <paramref name="index"/> in the lifeline list.</summary>
    public void UseLifeline(int index)
    {
        if (index < 0 || index >= _lifelines.Count) return;
        var lifeline = _lifelines[index];
        if (!lifeline.IsAvailable || _currentQuestion is null) return;

        var player = _players[_hotSeatIndex];
        var result = lifeline.Activate(_currentQuestion, player, _players);

        if (lifeline is FiftyFiftyLifeline)
            _activeOptions = result.RemainingOptions.ToList();

        LifelineUsed?.Invoke(this, new LifelineUsedEvent(
            LifelineName:     lifeline.Name,
            Narrative:        result.Narrative,
            RemainingOptions: result.RemainingOptions,
            Suggestion:       result.Suggestion));

        // Re-emit question with updated options
        RaiseQuestionReady();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void BeginHotSeat()
    {
        if (_hotSeatIndex >= _players.Count)
        {
            EndGame();
            return;
        }

        var player    = _players[_hotSeatIndex];
        _ladder       = new PrizeLadder();
        _lifelines    = [new FiftyFiftyLifeline(), new PhoneAFriendLifeline(), new AskTheAudienceLifeline()];
        _questionPool = BuildQuestionPool();

        HotSeatBegan?.Invoke(this, new HotSeatBeganEvent(
            PlayerName:   player.DisplayName,
            PlayerIndex:  _hotSeatIndex,
            TotalPlayers: _players.Count));

        LoadNextQuestion();
    }

    private void LoadNextQuestion()
    {
        if (_ladder.IsComplete) { AdvanceToNextPlayer(); return; }

        var q = PickQuestion(_ladder.CurrentRung, _questionPool);
        if (q is null) { _ladder.Advance(); LoadNextQuestion(); return; }

        _questionPool.Remove(q);
        _currentQuestion = q;
        _activeOptions   = Enum.GetValues<AnswerLabel>().ToList();

        RaiseQuestionReady();
    }

    private void RaiseQuestionReady()
    {
        if (_currentQuestion is null) return;
        QuestionReady?.Invoke(this, new QuestionReadyEvent(
            QuestionText:     _currentQuestion.Title,
            Answers:          _currentQuestion.Answers,
            AvailableOptions: _activeOptions.AsReadOnly(),
            Ladder:           BuildLadderSnapshot(),
            Lifelines:        _lifelines.Select(l => new LifelineSnapshot(l.Name, l.IsAvailable)).ToList().AsReadOnly()));
    }

    private void AdvanceToNextPlayer()
    {
        _hotSeatIndex++;
        if (_hotSeatIndex < _players.Count)
            BeginHotSeat();
        else
            EndGame();
    }

    private void EndGame()
    {
        IsRunning = false;
        var results = _players
            .Select(p => new HotSeatResult(p.DisplayName, _bankedPrizes[p.Id]))
            .OrderByDescending(r => r.Prize)
            .ToList().AsReadOnly();
        GameEnded?.Invoke(this, new MillionaireGameEndedEvent(results));
    }

    private PrizeLadderSnapshot BuildLadderSnapshot() =>
        new(
            Rungs: _ladder.Rungs.Select((r, i) => new LadderRungSnapshot(
                r.QuestionNumber, r.PrizeAmount, r.IsSafeHaven,
                i == _ladder.CurrentRungIndex && !_ladder.IsComplete,
                i < _ladder.CurrentRungIndex))
            .ToList().AsReadOnly(),
            CurrentIndex:    _ladder.CurrentRungIndex,
            GuaranteedPrize: _ladder.GuaranteedPrize,
            IsComplete:      _ladder.IsComplete);

    private List<MultipleChoiceCard> BuildQuestionPool()
    {
        var bank = _customBank ?? (IReadOnlyList<MultipleChoiceCard>)MillionaireQuestionBank.All;
        return bank
            .OrderBy(q => q.Difficulty)
            .ThenBy(_ => Random.Shared.Next())
            .ToList();
    }

    private static MultipleChoiceCard? PickQuestion(
        PrizeLadderRung rung,
        List<MultipleChoiceCard> pool)
    {
        var target = rung.QuestionNumber switch
        {
            <= 5  => Difficulty.Easy,
            <= 10 => Difficulty.Medium,
            <= 14 => Difficulty.Hard,
            _     => Difficulty.Extreme,
        };
        return pool.FirstOrDefault(q => q.Difficulty == target) ?? pool.FirstOrDefault();
    }

    /// <inheritdoc />
    public void Dispose() { /* no managed resources to release */ }
}