using TableTop.Hosting.Abstractions;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Tests.Helpers;

/// <summary>
/// Minimal <see cref="INavigator"/> that records whether it was asked to go
/// back, instead of actually navigating anywhere.
/// </summary>
public sealed class FakeNavigator : INavigator
{
    /// <summary>Number of times <see cref="GoBack"/> was called.</summary>
    public int GoBackCount { get; private set; }

    /// <inheritdoc />
    public void GoBack() => GoBackCount++;
}

/// <summary>
/// In-memory <see cref="IAppSettings"/> backed by plain auto-properties — no
/// persistence, no head-specific storage, just the shape both real
/// implementations (MAUI's <c>AppSettings</c>, WinUI's <c>WinUIAppSettings</c>)
/// already satisfy.
/// </summary>
public sealed class FakeAppSettings : IAppSettings
{
    /// <inheritdoc />
    public event EventHandler<string>? Changed;

    /// <inheritdoc />
    public string Theme { get; set; } = "dark";
    /// <inheritdoc />
    public int CardFontSize { get; set; } = 15;
    /// <inheritdoc />
    public bool ShuffleCards { get; set; } = true;
    /// <inheritdoc />
    public int MinDifficulty { get; set; }
    /// <inheritdoc />
    public int MaxDifficulty { get; set; } = 3;
    /// <inheritdoc />
    public int MinAgeRating { get; set; }
    /// <inheritdoc />
    public int CardsPerPlayer { get; set; }
    /// <inheritdoc />
    public bool AutoNextPlayer { get; set; } = true;
    /// <inheritdoc />
    public bool EnableTimer { get; set; }
    /// <inheritdoc />
    public int TimerSeconds { get; set; } = 60;
    /// <inheritdoc />
    public bool ShowCardCount { get; set; } = true;
    /// <inheritdoc />
    public bool ShowDifficultyBadge { get; set; } = true;
    /// <inheritdoc />
    public bool ShowCategoryBadge { get; set; } = true;
    /// <inheritdoc />
    public IReadOnlyList<SavedPlayer> RecentPlayers { get; set; } = [];

    /// <summary>How many times <see cref="ResetToDefaults"/> was called.</summary>
    public int ResetCount { get; private set; }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        ResetCount++;
        Theme = "dark"; CardFontSize = 15; ShuffleCards = true;
        MinDifficulty = 0; MaxDifficulty = 3; MinAgeRating = 0; CardsPerPlayer = 0;
        AutoNextPlayer = true; EnableTimer = false; TimerSeconds = 60;
        ShowCardCount = true; ShowDifficultyBadge = true; ShowCategoryBadge = true;
        Changed?.Invoke(this, "*");
    }
}

/// <summary>
/// Mutable <see cref="IClock"/> for testing day-gated controllers without
/// sleeping for real days — exactly what the interface's own doc comment
/// says it exists for.
///
/// Named MutableClock rather than FakeClock: a same-named internal
/// IClock fake already exists in NewArchetypeModesTests.cs, in the same
/// TableTop.Tests namespace — same-namespace lookup wins over `using`, so a
/// second FakeClock here would have silently shadowed with no compile
/// error, only a confusing missing-member one at the first call to a method
/// only this version has (found exactly that way, writing DayOneGameViewModelTests.cs).
/// </summary>
public sealed class MutableClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Advances the clock by the given span.</summary>
    public void Advance(TimeSpan span) => UtcNow += span;
}
