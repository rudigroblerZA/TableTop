using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives a simultaneous-answer session. Has no knowledge of any UI — raises
/// typed events; any renderer subscribes.
///
/// <para>
/// <b>The scoring, and why it's shaped this way.</b> Matching the herd scores
/// <see cref="HerdPoints"/> per player in the largest group. But being the
/// <i>only</i> person to give an answer scores <see cref="LoneVoicePoints"/> —
/// deliberately fewer than the herd, and only when exactly one player is
/// alone.
/// </para>
///
/// <para>
/// Without the lone-voice rule the game has one dominant strategy: always name
/// the most obvious thing. That's fine for a round or two and then it's
/// solved, and the interesting players — the ones who think of something
/// better — are actively punished. Scoring the lone voice keeps a real
/// decision on every card: play the obvious answer for the safe points, or
/// back yourself to be the only one who says something. It's worth less than
/// matching, so it's a gamble rather than a better strategy — which is the
/// balance that keeps both options live.
/// </para>
///
/// <para>
/// A round where every player says something different has <b>no herd</b> and
/// no lone voice either (nobody is uniquely alone when everyone is), so it
/// scores nothing at all. That's a legitimate outcome rather than an edge case
/// to paper over: the table just proved the prompt was too open, and the
/// scoreboard should say so.
/// </para>
/// </summary>
public sealed class HerdController : IHerdController
{
    /// <summary>Points for each player in the largest answer group.</summary>
    public const int HerdPoints = 3;

    /// <summary>Points for being the only player to give an answer. Deliberately fewer than <see cref="HerdPoints"/>.</summary>
    public const int LoneVoicePoints = 2;

    private readonly IReadOnlyList<IPlayer> _players;
    private readonly IReadOnlyList<ICard> _deck;
    private readonly Dictionary<string, int> _scores = new(StringComparer.OrdinalIgnoreCase);

    private int _index = -1;

    /// <inheritdoc />
    public event EventHandler<HerdPromptReadyEvent>? PromptReady;
    /// <inheritdoc />
    public event EventHandler<HerdRoundResolvedEvent>? RoundResolved;
    /// <inheritdoc />
    public event EventHandler<HerdGameEndedEvent>? GameEnded;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public int RoundNumber => _index + 1;

    /// <inheritdoc />
    public int TotalRounds => _deck.Count;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, int> Scores => _scores;

    /// <summary>Initialises a new <see cref="HerdController"/>.</summary>
    public HerdController(IReadOnlyList<IPlayer> players, IReadOnlyList<ICard> deck)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(deck);

        // Three players is the real floor for the mechanic: with two, "the
        // largest group" is either both of them or neither, so there's no herd
        // to read and the game degenerates into agree/disagree.
        //
        // But this is NOT enforced by throwing, and that's deliberate. Every
        // other mode treats MinimumPlayers as advisory — RivalsMode declares 4
        // and the engine runs it happily with 2; PlayerSetupViewModel.CanStartGame
        // is what actually stops a player short-handing a game. Throwing here
        // made this the only controller in the codebase that could refuse to
        // construct, which broke a registry-wide sweep test and, worse, would
        // throw when resuming a saved session whose roster had shrunk.
        //
        // Degrading is the right behaviour: with two players nobody ever
        // matches, every round scores nothing, and the scoreboard says the
        // game isn't working — which is honest and recoverable, unlike a crash.
        if (deck.Count == 0)
            throw new ArgumentException("The prompt deck is empty.", nameof(deck));

        _players = players;
        _deck = deck;

        foreach (var p in players) _scores[p.DisplayName] = 0;
    }

    /// <inheritdoc />
    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        AdvanceToNextPrompt();
    }

    /// <inheritdoc />
    public void SubmitAnswers(IReadOnlyDictionary<string, string> answers)
    {
        if (!IsRunning || _index < 0 || _index >= _deck.Count) return;
        ArgumentNullException.ThrowIfNull(answers);

        var prompt = _deck[_index];

        // Group by a normalised form so "Corn Flakes", "corn flakes" and
        // " CORNFLAKES " count as the same answer — players type what they
        // type, and a scoring rule that punishes casing would be indefensible.
        var groups = answers
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .GroupBy(kv => Normalise(kv.Value), StringComparer.Ordinal)
            .Select(g => new AnswerGroup(
                g.First().Value.Trim(),
                g.Select(kv => kv.Key).ToList().AsReadOnly()))
            .OrderByDescending(g => g.PlayerNames.Count)
            .ThenBy(g => g.Answer, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var roundScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // The herd: the largest group, but only if it's actually a group.
        var largest = groups.FirstOrDefault();
        string? herdAnswer = null;
        if (largest is { PlayerNames.Count: > 1 })
        {
            herdAnswer = largest.Answer;
            foreach (var name in largest.PlayerNames)
                roundScores[name] = roundScores.GetValueOrDefault(name) + HerdPoints;
        }

        // The lone voice: scored only when exactly one player stood alone.
        // When everyone answered differently, every group has one member and
        // nobody is distinctively alone — so nobody scores, which is the
        // point of the rule rather than a gap in it.
        var singletons = groups.Where(g => g.PlayerNames.Count == 1).ToList();
        string? loneVoice = null;
        if (singletons.Count == 1 && groups.Count > 1)
        {
            loneVoice = singletons[0].PlayerNames[0];
            roundScores[loneVoice] = roundScores.GetValueOrDefault(loneVoice) + LoneVoicePoints;
        }

        foreach (var (name, delta) in roundScores)
            if (_scores.ContainsKey(name)) _scores[name] += delta;

        RoundResolved?.Invoke(this, new HerdRoundResolvedEvent(
            prompt.Description, groups.AsReadOnly(), herdAnswer, roundScores, loneVoice));

        AdvanceToNextPrompt();
    }

    /// <inheritdoc />
    public void Quit()
    {
        if (!IsRunning) return;
        End();
    }

    private void AdvanceToNextPrompt()
    {
        _index++;
        if (_index >= _deck.Count) { End(); return; }

        var card = _deck[_index];
        PromptReady?.Invoke(this, new HerdPromptReadyEvent(
            RoundNumber, TotalRounds, card.Description, card.Category));
    }

    private void End()
    {
        IsRunning = false;

        var ordered = _scores.OrderByDescending(kv => kv.Value).ToList();
        var top = ordered.Count > 0 ? ordered[0].Value : 0;

        // Ties report every leader rather than picking one — the same choice
        // ClaimedController makes for a tied deck-exhaustion ending.
        var winners = ordered.Where(kv => kv.Value == top).Select(kv => kv.Key).ToList();

        GameEnded?.Invoke(this, new HerdGameEndedEvent(
            winners.AsReadOnly(),
            ordered.AsReadOnly(),
            Math.Min(_index, _deck.Count)));
    }

    /// <summary>
    /// Comparison form for answers: trimmed, lowercased, inner whitespace
    /// collapsed, and surrounding punctuation dropped. Deliberately does not
    /// attempt anything cleverer — stemming or synonyms would start making
    /// judgement calls the players should be making themselves.
    /// </summary>
    private static string Normalise(string answer)
    {
        var trimmed = answer.Trim().ToLowerInvariant();
        var chars = trimmed.Where(c => !char.IsPunctuation(c)).ToArray();
        return string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <inheritdoc />
    public void Dispose() { /* no managed resources to release */ }
}
