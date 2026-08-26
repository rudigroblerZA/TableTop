using TableTop.Core.Abstractions.Presentation;
using TableTop.Games.Couples;

namespace TableTop.Tests;

/// <summary>
/// What is left of mode presentation after the JSON deck path was removed.
///
/// <para>
/// This class used to prove that a deck's <c>presentation</c> block overrode a
/// mode's compiled-in title, labels, colours and palette, and that the 90 decks
/// carrying no block behaved exactly as they had before the feature existed.
/// With no deck files and no loader, only that second half can still be true —
/// so it is now the whole contract: <c>Presentation</c> is always
/// <see cref="ModePresentation.None"/> and every <c>Resolved*</c> member is a
/// pass-through to the C# value.
/// </para>
///
/// <para>
/// The <c>CardDeckFile</c> half went in 1.21.0 with the deck file format
/// itself. What is left is entirely about compiled-in modes.
/// </para>
/// </summary>
public sealed class ModePresentationTests
{
    // ── presentation is now always absent ─────────────────────────────────────

    [Fact]
    public void EveryMode_ResolvesToItsCompiledInValues()
    {
        // Was asserted of SlowBurnMode alone, as the mode declaring no deck
        // resource. Every mode is that mode now, so assert it of one that used
        // to be the counter-example: Undivided shipped a full presentation
        // block and a palette, and must now be indistinguishable from any other.
        var mode = new UndividedMode();

        mode.Presentation.Should().BeSameAs(ModePresentation.None);
        mode.Presentation.IsEmpty.Should().BeTrue();
        mode.DisplayName.Should().Be(mode.Name);
        mode.DisplayDescription.Should().Be(mode.Description);
        mode.ResolvedCompleteLabel.Should().Be(mode.CompleteLabel);
        mode.ResolvedSkipLabel.Should().Be(mode.SkipLabel);
        mode.ResolvedMinimumPlayers.Should().Be(mode.MinimumPlayers);
        mode.Theme.Should().BeNull();

        // Value equality, not reference. CategoryColours and the pinned lists are
        // expression-bodied properties that build a fresh collection per call, so
        // two reads are never the same instance even though the resolved member
        // does nothing but return the raw one.
        mode.ResolvedCategoryColours.Should().BeEquivalentTo(mode.CategoryColours);
        mode.ResolvedCategoriesPinnedToStart.Should().BeEquivalentTo(mode.CategoriesPinnedToStart);
        mode.ResolvedCategoriesPinnedToEnd.Should().BeEquivalentTo(mode.CategoriesPinnedToEnd);
    }

    [Fact]
    public void SlowBurn_TheModeThatNeverHadAPresentationBlock_IsUnchanged()
    {
        var mode = new SlowBurnMode();

        mode.Presentation.Should().BeSameAs(ModePresentation.None);
        mode.DisplayName.Should().Be(mode.Name);
        mode.Theme.Should().BeNull();
    }

    // Four tests lived here covering the CardDeckFile side of presentation:
    // DeckExporter preserving a presentation block, a deck file round-tripping
    // fields an editor has no UI for, WhenWritingNull keeping deck diffs
    // readable, and the two-argument export overload. They went with the deck
    // file format in 1.21.0 — there is no CardDeckFile to export to.
    //
    // The bug they were written against is worth restating in case a file format
    // ever returns: the original export rebuilt a file from name and cards alone,
    // so anything the exporter had no UI for — prompts, break cards, reward
    // effects, the whole theme — was silently dropped on save. An editor that
    // loses data it cannot display is worse than one that refuses to open it.

    // ── the fallback chain the UI wrappers depend on ──────────────────────────

    [Fact]
    public void SomeModesBypassTheBaseClass_SoUiMustNotBindDisplayNameDirectly()
    {
        // Six modes implement IGameMode without deriving from
        // BaseGameModeDefinition — the Millionaire-format ones. IGameMode has no
        // DisplayName, so a XAML binding to DisplayName renders blank for them
        // and does so silently. That is why MAUI's game list wraps modes in a
        // GameModeItem that resolves in C# instead of binding the domain object.
        var plain = new TableTop.Games.Fun.SlangCheckMode();

        plain.Should().BeAssignableTo<Core.Abstractions.Game.IGameMode>();
        plain.Should().NotBeAssignableTo<TableTop.Games.Base.BaseGameModeDefinition>(
            "if this ever changes, the wrapper's fallback branch becomes dead code");
        plain.Name.Should().NotBeNullOrWhiteSpace("the fallback has to have something to fall back to");
    }

    // AModeWithJsonPresentation_ResolvesDifferentlyFromItsCompiledValues lived
    // here. It asserted that UndividedMode's ResolvedCompleteLabel differed from
    // its CompleteLabel and that its Theme was non-null, on the grounds that
    // "the wiring in the UI is only worth anything if resolved and raw actually
    // differ somewhere."
    //
    // That was the right instinct and it is now simply false: with no deck files
    // and no loader, nothing can make them differ, and the test contradicted
    // EveryMode_ResolvesToItsCompiledInValues above word for word. Both could not
    // pass. Deleted rather than inverted, because the assertion it would become
    // is already made up there.
    //
    // What it was protecting is worth restating for whoever collapses the
    // Resolved* members: they are pass-throughs today, and the only reason to
    // keep them is that both heads and the API snapshot bind to them.

}
