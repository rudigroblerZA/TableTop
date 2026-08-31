using TableTop.Core.Abstractions.Analysis;

namespace TableTop.Core.Domain.Analysis;

/// <summary>
/// Accumulates player responses and turns them into <see cref="TraitProfile"/>s.
///
/// <para>
/// <b>Responses are stored, not summed on arrival.</b> Keeping one entry per
/// (player, item) and folding only at <see cref="Build"/> makes re-answering an
/// item last-answer-wins for free, which is exactly what a back button needs. A
/// running-total design would double-count the second answer, and the symptom —
/// one dimension quietly inflated for players who changed their mind — is
/// invisible without knowing the right answer in advance. Item banks here are
/// tens of items, so the memory this costs is not worth the class of bug it
/// removes.
/// </para>
/// </summary>
public sealed class TraitProfileBuilder
{
    private readonly TraitScale _scale;
    private readonly ITraitScoringStrategy _scoring;

    // player → item id → (item, response).
    private readonly Dictionary<string, Dictionary<Guid, (ITraitItemCard Item, LikertResponse Response)>> _responses
        = new(StringComparer.OrdinalIgnoreCase);

    // First-response order, kept separately because Dictionary enumeration order
    // is explicitly not part of its contract. BuildAll and Players both surface
    // it, and a results screen whose player order shuffles between runs is the
    // kind of defect that gets written off as "just the UI" — the same class of
    // non-determinism backlog L.2 found in check-mvvm-method-parity.py, where a
    // set iteration made the answer vary per process.
    private readonly List<string> _playerOrder = [];

    /// <summary>Initialises a new <see cref="TraitProfileBuilder"/>.</summary>
    /// <param name="scale">The instrument to score against.</param>
    /// <param name="scoring">
    /// Scoring model. Defaults to <see cref="WeightedLikertScoring"/>.
    ///
    /// <para>
    /// This optional-with-a-default shape is the one backlog X.2 retired for
    /// <c>IControllerFactory</c>, so it is worth saying why it is safe here.
    /// That default was dangerous because a <c>ControllerFactory</c> built on
    /// the spot silently carried no persistence, no diagnostics sink and no DI
    /// registration — the substitute was missing host configuration the caller
    /// had already supplied elsewhere. <see cref="WeightedLikertScoring"/> is
    /// stateless, has no configuration to drop, and two instances of it are
    /// indistinguishable. There is nothing here for a default to lose.
    /// </para>
    /// </param>
    public TraitProfileBuilder(TraitScale scale, ITraitScoringStrategy? scoring = null)
    {
        ArgumentNullException.ThrowIfNull(scale);

        _scale = scale;
        _scoring = scoring ?? new WeightedLikertScoring();
    }

    /// <summary>The instrument being scored against.</summary>
    public TraitScale Scale => _scale;

    /// <summary>Every player who has recorded at least one response, in first-response order.</summary>
    public IReadOnlyList<string> Players => _playerOrder.AsReadOnly();

    /// <summary>
    /// Records <paramref name="response"/> for <paramref name="playerName"/> on
    /// <paramref name="item"/>, replacing any previous answer to that item.
    /// </summary>
    public void Record(string playerName, ITraitItemCard item, LikertResponse response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerName);
        ArgumentNullException.ThrowIfNull(item);

        if (!_responses.TryGetValue(playerName, out var forPlayer))
        {
            forPlayer = new Dictionary<Guid, (ITraitItemCard Item, LikertResponse Response)>();
            _responses[playerName] = forPlayer;
            _playerOrder.Add(playerName);
        }

        forPlayer[item.Id] = (item, response);
    }

    /// <summary>How many items <paramref name="playerName"/> has answered.</summary>
    public int AnsweredCount(string playerName) =>
        string.IsNullOrWhiteSpace(playerName) ? 0
        : _responses.TryGetValue(playerName, out var forPlayer) ? forPlayer.Count
        : 0;

    /// <summary>
    /// Builds <paramref name="playerName"/>'s profile. A player with no
    /// recorded responses gets a profile of empty scores rather than null, so a
    /// results screen renders the same shape for everyone.
    /// </summary>
    public TraitProfile Build(string playerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerName);

        _responses.TryGetValue(playerName, out var forPlayer);
        var answers = forPlayer is null
            ? new List<(ITraitItemCard Item, LikertResponse Response)>()
            : forPlayer.Values.ToList();

        var scores = new List<TraitScore>(_scale.Traits.Count);

        foreach (var trait in _scale.Traits)
        {
            double raw = 0, min = 0, max = 0;
            var itemCount = 0;

            foreach (var (item, response) in answers)
            {
                // Null means "this item does not load on this trait" — the
                // common case. It must not widen the bounds; see the remarks on
                // ITraitScoringStrategy.Contribute.
                if (_scoring.Contribute(item, trait.Key, response) is not { } c) continue;

                raw += c.Value;
                min += c.Minimum;
                max += c.Maximum;
                itemCount++;
            }

            scores.Add(new TraitScore(trait, raw, min, max, itemCount));
        }

        return new TraitProfile(playerName, _scale, scores, answers.Count);
    }

    /// <summary>Builds a profile for every player who has recorded a response.</summary>
    public IReadOnlyList<TraitProfile> BuildAll() =>
        _playerOrder.Select(Build).ToList().AsReadOnly();

    /// <summary>Discards every recorded response.</summary>
    public void Clear()
    {
        _responses.Clear();
        _playerOrder.Clear();
    }
}
