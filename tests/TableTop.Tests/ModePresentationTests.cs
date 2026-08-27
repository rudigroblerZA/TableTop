namespace TableTop.Tests;

/// <summary>
/// What used to be mode presentation, before the JSON deck path — and then the
/// dead <c>Presentation</c>/<c>Resolved*</c> pass-through layer it left behind —
/// were both removed.
///
/// <para>
/// This class used to prove that a deck's <c>presentation</c> block overrode a
/// mode's compiled-in title, labels, colours and palette, then (once the deck
/// path was gone) that every mode fell through to its compiled-in value
/// unconditionally. With <c>BaseGameModeDefinition.Presentation</c> and its nine
/// dependants deleted (backlog item 18), there is nothing left to prove about
/// presentation resolution — a mode's name, description and labels are just its
/// own properties again.
/// </para>
/// </summary>
public sealed class ModePresentationTests
{
    [Fact]
    public void SomeModesBypassTheBaseClass_SoUiMustNotAssumeBaseGameModeDefinition()
    {
        // Six modes implement IGameMode without deriving from
        // BaseGameModeDefinition — the Millionaire-format ones. That is why
        // MAUI's game list wraps modes in a GameModeItem that resolves in C#
        // via ModeDisplayResolver instead of binding the domain object
        // directly, with a fallback to IGameMode.Name for exactly this case.
        var plain = new TableTop.Games.Fun.SlangCheckMode();

        plain.Should().BeAssignableTo<Core.Abstractions.Game.IGameMode>();
        plain.Should().NotBeAssignableTo<TableTop.Games.Base.BaseGameModeDefinition>(
            "if this ever changes, the wrapper's fallback branch becomes dead code");
        plain.Name.Should().NotBeNullOrWhiteSpace("the fallback has to have something to fall back to");
    }
}
