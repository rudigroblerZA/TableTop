using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Lifelines;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Lifelines;

/// <summary>
/// Eliminates two of the three incorrect answers, leaving the correct one
/// and one randomly chosen wrong answer.
/// </summary>
public sealed class FiftyFiftyLifeline : ILifeline
{
    private readonly Random _random;

    /// <summary>Initialises a new <see cref="FiftyFiftyLifeline"/> instance.</summary>
    public FiftyFiftyLifeline() : this(Random.Shared) { }
    /// <summary>Initialises a new <see cref="FiftyFiftyLifeline"/> instance.</summary>
    public FiftyFiftyLifeline(Random random) => _random = random;

    /// <inheritdoc />
    public string Name => "50:50";
    /// <inheritdoc />
    public string Description => "Removes two wrong answers, leaving the correct one and one other.";
    /// <summary>Whether this lifeline has not yet been used this session.</summary>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>Initialises a new <see cref="Activate"/> instance.</summary>
    public LifelineResult Activate(IMultipleChoiceCard card, IPlayer player, IReadOnlyList<IPlayer> audience)
    {
        EnsureAvailable();

        var wrong = Enum.GetValues<AnswerLabel>()
            .Where(l => l != card.CorrectAnswer)
            .OrderBy(_ => _random.Next())
            .Take(2)
            .ToList();

        var remaining = Enum.GetValues<AnswerLabel>()
            .Except(wrong)
            .ToList()
            .AsReadOnly();

        return new LifelineResult(
            Narrative: $"Two wrong answers have been removed. Remaining: {string.Join(" and ", remaining)}.",
            RemainingOptions: remaining);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("50:50 has already been used.");
        IsAvailable = false;
    }
}

/// <summary>
/// Simulates a phone call to a knowledgeable friend.
/// The friend is correct 70% of the time on easy questions, less so on hard ones.
/// </summary>
public sealed class PhoneAFriendLifeline : ILifeline
{
    private readonly Random _random;

    private static readonly string[] FriendNames = ["Jordan", "Sam", "Riley", "Morgan", "Casey"];

    /// <summary>Initialises a new <see cref="PhoneAFriendLifeline"/> instance.</summary>
    public PhoneAFriendLifeline() : this(Random.Shared) { }
    /// <summary>Initialises a new <see cref="PhoneAFriendLifeline"/> instance.</summary>
    public PhoneAFriendLifeline(Random random) => _random = random;

    /// <inheritdoc />
    public string Name => "Phone a Friend";
    /// <inheritdoc />
    public string Description => "Call a knowledgeable friend for their best guess.";
    /// <summary>Whether this lifeline has not yet been used this session.</summary>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>Initialises a new <see cref="Activate"/> instance.</summary>
    public LifelineResult Activate(IMultipleChoiceCard card, IPlayer player, IReadOnlyList<IPlayer> audience)
    {
        EnsureAvailable();

        var friendName = FriendNames[_random.Next(FriendNames.Length)];

        // Accuracy depends on difficulty
        double accuracy = card.Difficulty switch
        {
            Difficulty.Easy    => 0.85,
            Difficulty.Medium  => 0.65,
            Difficulty.Hard    => 0.45,
            Difficulty.Extreme => 0.30,
            _                  => 0.60
        };

        var givesCorrect = _random.NextDouble() < accuracy;
        var suggestion = givesCorrect
            ? card.CorrectAnswer
            : Enum.GetValues<AnswerLabel>()
                  .Where(l => l != card.CorrectAnswer)
                  .OrderBy(_ => _random.Next())
                  .First();

        var confidence = givesCorrect
            ? _random.Next(60, 95)
            : _random.Next(30, 60);

        var hesitation = card.Difficulty >= Difficulty.Hard
            ? "Hmm, this is a tough one... "
            : "";

        var narrative =
            $"[Ringing {friendName}...]\n" +
            $"  {friendName}: \"Hello? Oh wow, okay. Let me think...\"\n" +
            $"  {hesitation}\"I'm going to say {suggestion} — '{card.Answers[suggestion]}'.\"\n" +
            $"  \"I'm about {confidence}% sure on that one. Good luck!\"";

        return new LifelineResult(
            Narrative: narrative,
            RemainingOptions: Enum.GetValues<AnswerLabel>().ToList().AsReadOnly(),
            Suggestion: suggestion);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException("Phone a Friend has already been used.");
        IsAvailable = false;
    }
}

/// <summary>
/// Simulates an audience vote. The audience is generally reliable on easy questions
/// but less so as difficulty increases.
/// </summary>
public sealed class AskTheAudienceLifeline : ILifeline
{
    private readonly Random _random;

    /// <summary>Initialises a new <see cref="AskTheAudienceLifeline"/> instance.</summary>
    public AskTheAudienceLifeline() : this(Random.Shared) { }
    /// <summary>Initialises a new <see cref="AskTheAudienceLifeline"/> instance.</summary>
    public AskTheAudienceLifeline(Random random) => _random = random;

    /// <inheritdoc />
    public string Name => "Ask the Audience";
    /// <inheritdoc />
    public string Description => "The audience votes for what they think the answer is.";
    /// <summary>Whether this lifeline has not yet been used this session.</summary>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>Initialises a new <see cref="Activate"/> instance.</summary>
    public LifelineResult Activate(IMultipleChoiceCard card, IPlayer player, IReadOnlyList<IPlayer> audience)
    {
        EnsureAvailable();

        // Simulate audience vote distribution
        var votes = GenerateVotes(card);
        var topLabel = votes.OrderByDescending(kv => kv.Value).First().Key;

        var lines = votes
            .OrderBy(kv => kv.Key)
            .Select(kv => $"    {kv.Key}: {kv.Value,3}%  {Bar(kv.Value)}");

        var narrative =
            "  [Audience voting...]\n\n" +
            string.Join("\n", lines) +
            $"\n\n  The audience leans towards {topLabel}.";

        return new LifelineResult(
            Narrative: narrative,
            RemainingOptions: Enum.GetValues<AnswerLabel>().ToList().AsReadOnly(),
            Suggestion: topLabel);
    }

    private Dictionary<AnswerLabel, int> GenerateVotes(IMultipleChoiceCard card)
    {
        // Correct answer gets a majority share that shrinks with difficulty
        double correctShare = card.Difficulty switch
        {
            Difficulty.Easy    => 0.55 + _random.NextDouble() * 0.25,  // 55–80%
            Difficulty.Medium  => 0.40 + _random.NextDouble() * 0.20,  // 40–60%
            Difficulty.Hard    => 0.28 + _random.NextDouble() * 0.20,  // 28–48%
            Difficulty.Extreme => 0.20 + _random.NextDouble() * 0.20,  // 20–40%
            _                  => 0.50
        };

        var remaining = 1.0 - correctShare;
        var wrongLabels = Enum.GetValues<AnswerLabel>()
            .Where(l => l != card.CorrectAnswer)
            .OrderBy(_ => _random.Next())
            .ToList();

        // Split remaining share randomly among the three wrong answers
        var split = SplitIntoThree(remaining);

        var votes = new Dictionary<AnswerLabel, int>
        {
            [card.CorrectAnswer] = (int)Math.Round(correctShare * 100)
        };

        for (var i = 0; i < wrongLabels.Count; i++)
            votes[wrongLabels[i]] = (int)Math.Round(split[i] * 100);

        // Normalise to exactly 100%
        var total = votes.Values.Sum();
        var diff = 100 - total;
        votes[card.CorrectAnswer] += diff;

        return votes;
    }

    private double[] SplitIntoThree(double value)
    {
        var a = _random.NextDouble() * value;
        var b = _random.NextDouble() * (value - a);
        var c = value - a - b;
        return [a, b, c];
    }

    private static string Bar(int percent)
    {
        var filled = percent / 5;
        return "[" + new string('█', filled) + new string('░', 20 - filled) + "]";
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException("Ask the Audience has already been used.");
        IsAvailable = false;
    }
}