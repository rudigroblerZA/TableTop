namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// One player's complete result: a score on every dimension of the scale they
/// were assessed against.
///
/// <para>
/// Every dimension of the scale appears, including ones no answered item loaded
/// on — those come back with <see cref="TraitScore.HasData"/> false. A profile
/// with a missing key would make every consumer write the same defensive
/// lookup, and a renderer that silently skipped a dimension would be
/// indistinguishable from one that scored it in the middle.
/// </para>
/// </summary>
public sealed class TraitProfile
{
    private readonly Dictionary<string, TraitScore> _byKey;

    /// <summary>Initialises a new <see cref="TraitProfile"/>.</summary>
    /// <param name="playerName">Who this profile belongs to.</param>
    /// <param name="scale">The instrument they were assessed against.</param>
    /// <param name="scores">One score per dimension of <paramref name="scale"/>.</param>
    /// <param name="answeredItems">How many items the player actually responded to.</param>
    public TraitProfile(
        string playerName,
        TraitScale scale,
        IEnumerable<TraitScore> scores,
        int answeredItems)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerName);
        ArgumentNullException.ThrowIfNull(scale);
        ArgumentNullException.ThrowIfNull(scores);
        ArgumentOutOfRangeException.ThrowIfNegative(answeredItems);

        PlayerName = playerName;
        Scale = scale;
        Scores = scores.ToList().AsReadOnly();
        AnsweredItems = answeredItems;

        _byKey = Scores.ToDictionary(s => s.Trait.Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Who this profile belongs to.</summary>
    public string PlayerName { get; }

    /// <summary>The instrument this player was assessed against.</summary>
    public TraitScale Scale { get; }

    /// <summary>One score per dimension, in the scale's own order.</summary>
    public IReadOnlyList<TraitScore> Scores { get; }

    /// <summary>How many items the player actually responded to.</summary>
    public int AnsweredItems { get; }

    /// <summary>The score for <paramref name="traitKey"/>, or null if the scale has no such dimension.</summary>
    public TraitScore? Find(string traitKey) =>
        string.IsNullOrWhiteSpace(traitKey) ? null : _byKey.GetValueOrDefault(traitKey);

    /// <summary>
    /// The dimensions this player scored highest on, strongest first.
    ///
    /// <para>
    /// Dimensions with no data are excluded — an unanswered dimension sitting at
    /// the midpoint is not a finding, and letting it rank would put "we know
    /// nothing about this" above a genuine result of 48.
    /// </para>
    /// </summary>
    public IReadOnlyList<TraitScore> Strongest(int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return Scores.Where(s => s.HasData)
                     .OrderByDescending(s => s.Normalized)
                     .ThenBy(s => s.Trait.Key, StringComparer.Ordinal)
                     .Take(count)
                     .ToList()
                     .AsReadOnly();
    }
}
