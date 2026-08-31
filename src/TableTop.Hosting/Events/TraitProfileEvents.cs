using TableTop.Core.Abstractions.Analysis;

namespace TableTop.Hosting.Events;

/// <summary>
/// A statement is on screen and every player should answer it.
/// </summary>
/// <param name="ItemNumber">1-based position in the bank.</param>
/// <param name="TotalItems">How many statements this session plays.</param>
/// <param name="Statement">The statement to agree or disagree with.</param>
/// <param name="Category">Thematic grouping — conventionally the dimension it loads on.</param>
public sealed record TraitItemReadyEvent(
    int ItemNumber,
    int TotalItems,
    string Statement,
    string Category);

/// <summary>
/// One statement's responses have been recorded.
/// </summary>
/// <param name="ItemNumber">1-based position of the statement just answered.</param>
/// <param name="Statement">The statement that was answered.</param>
/// <param name="Responses">Response by player name. Players who skipped are absent.</param>
public sealed record TraitItemRecordedEvent(
    int ItemNumber,
    string Statement,
    IReadOnlyDictionary<string, LikertResponse> Responses);

/// <summary>
/// The assessment finished — every profile, and the pairwise reads across them.
/// </summary>
/// <param name="Profiles">One profile per player who answered anything, in join order.</param>
/// <param name="Comparisons">Every unordered pair, empty for a single player.</param>
/// <param name="MostAlike">The closest pair, or null when fewer than two players answered.</param>
/// <param name="MostDifferent">The furthest-apart pair, or null under the same condition.</param>
/// <param name="ItemsAnswered">How many statements the session got through.</param>
public sealed record TraitAssessmentCompletedEvent(
    IReadOnlyList<TraitProfile> Profiles,
    IReadOnlyList<TraitProfileComparison> Comparisons,
    TraitProfileComparison? MostAlike,
    TraitProfileComparison? MostDifferent,
    int ItemsAnswered);
