using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Analysis;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives a trait-assessment session. Has no knowledge of any UI — raises typed
/// events; any renderer subscribes.
///
/// <para>
/// <b>The one structural difference from every other controller here: there is
/// no score.</b> Others accumulate an <c>int</c> per player and end by naming a
/// winner. This accumulates a vector per player and ends by handing back
/// profiles and the comparisons between them. That is why it is its own
/// <c>ControllerFamily</c> rather than a <c>CardTurnController</c> with a
/// different scoring strategy — <c>IScoringStrategy</c> returns a scalar, and
/// no amount of configuring one produces five running totals.
/// </para>
///
/// <para>
/// <b>Skips are absences, not neutral answers.</b> A player left out of a
/// <see cref="SubmitResponses"/> call contributes nothing to that item, and —
/// the part that matters — the item does not widen their score's denominator
/// either. Recording a skip as <see cref="LikertResponse.Neutral"/> would be
/// easier and would be wrong: neutral is a stated opinion that pulls a
/// dimension toward its midpoint, while a skip is missing data. A player who
/// skips forty of fifty items should show ten items' worth of a real profile,
/// not fifty items' worth of a profile flattened toward the middle.
/// </para>
/// </summary>
public sealed class TraitProfileController : ITraitProfileController
{
    private readonly IReadOnlyList<TraitItemCard> _items;
    private readonly IReadOnlyList<string> _playerNames;
    private readonly TraitProfileBuilder _builder;

    private int _index = -1;

    /// <inheritdoc />
    public event EventHandler<TraitItemReadyEvent>? ItemReady;
    /// <inheritdoc />
    public event EventHandler<TraitItemRecordedEvent>? ItemRecorded;
    /// <inheritdoc />
    public event EventHandler<TraitAssessmentCompletedEvent>? AssessmentCompleted;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public int ItemNumber => _index + 1;

    /// <inheritdoc />
    public int TotalItems => _items.Count;

    /// <inheritdoc />
    public TraitScale Scale { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> PlayerNames => _playerNames;

    /// <summary>Initialises a new <see cref="TraitProfileController"/>.</summary>
    /// <param name="players">The roster. One profile is produced per player who answers anything.</param>
    /// <param name="scale">The instrument to score against.</param>
    /// <param name="items">The item bank, in presentation order.</param>
    /// <param name="scoring">Optional scoring model; defaults to <see cref="WeightedLikertScoring"/>.</param>
    public TraitProfileController(
        IReadOnlyList<IPlayer> players,
        TraitScale scale,
        IReadOnlyList<TraitItemCard> items,
        ITraitScoringStrategy? scoring = null)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(scale);
        ArgumentNullException.ThrowIfNull(items);

        Scale = scale;
        _items = items;
        _playerNames = players.Select(p => p.DisplayName).ToList().AsReadOnly();
        _builder = new TraitProfileBuilder(scale, scoring);
    }

    /// <inheritdoc />
    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        AdvanceToNextItem();
    }

    /// <inheritdoc />
    public void SubmitResponses(IReadOnlyDictionary<string, LikertResponse> responses)
    {
        if (!IsRunning || _index < 0 || _index >= _items.Count) return;
        ArgumentNullException.ThrowIfNull(responses);

        var item = _items[_index];
        var recorded = new Dictionary<string, LikertResponse>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, response) in responses)
        {
            // Names not on the roster are dropped rather than trusted. A head
            // that mis-keys its response dictionary would otherwise silently
            // create a profile for a player who does not exist, and that shows
            // up as a stray column on the results screen rather than as an
            // error anyone can trace back.
            if (!_playerNames.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;

            // An out-of-range value would sail through the arithmetic and
            // produce a score outside the range the bounds describe, which
            // TraitScore.Normalize then clamps — so the symptom would be a
            // dimension stuck at 0 or 100, not an obvious fault.
            if (!Enum.IsDefined(response)) continue;

            _builder.Record(name, item, response);
            recorded[name] = response;
        }

        ItemRecorded?.Invoke(this, new TraitItemRecordedEvent(
            ItemNumber, item.Description, recorded));

        AdvanceToNextItem();
    }

    /// <inheritdoc />
    public void Skip()
    {
        if (!IsRunning || _index < 0 || _index >= _items.Count) return;

        ItemRecorded?.Invoke(this, new TraitItemRecordedEvent(
            ItemNumber,
            _items[_index].Description,
            new Dictionary<string, LikertResponse>(StringComparer.OrdinalIgnoreCase)));

        AdvanceToNextItem();
    }

    /// <inheritdoc />
    public void Quit()
    {
        if (!IsRunning) return;
        End();
    }

    private void AdvanceToNextItem()
    {
        _index++;
        if (_index >= _items.Count) { End(); return; }

        var item = _items[_index];
        ItemReady?.Invoke(this, new TraitItemReadyEvent(
            ItemNumber, TotalItems, item.Description, item.Category));
    }

    private void End()
    {
        IsRunning = false;

        // Roster order, not answer order: a results screen listing players in
        // whatever sequence they happened to first respond reads as arbitrary.
        // Players who answered nothing are excluded — an all-midpoint profile
        // built from zero items is not a result, and printing one next to a
        // real profile invites it to be read as one.
        var profiles = _playerNames
            .Where(n => _builder.AnsweredCount(n) > 0)
            .Select(_builder.Build)
            .ToList();

        var comparisons = TraitProfileComparer.CompareAll(profiles);

        AssessmentCompleted?.Invoke(this, new TraitAssessmentCompletedEvent(
            profiles.AsReadOnly(),
            comparisons,
            TraitProfileComparer.MostAlike(profiles),
            TraitProfileComparer.MostDifferent(profiles),
            Math.Min(_index, _items.Count)));
    }

    /// <inheritdoc />
    public void Dispose() { /* no managed resources to release */ }
}
