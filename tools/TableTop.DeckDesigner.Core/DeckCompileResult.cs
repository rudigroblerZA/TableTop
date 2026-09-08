using TableTop.Core.Abstractions.Cards;

namespace TableTop.DeckDesigner.Core;

/// <summary>Outcome of <see cref="DeckCompiler.Compile(DeckDraft)"/>.</summary>
public sealed class DeckCompileResult
{
    private DeckCompileResult(IReadOnlyList<ICard>? cards, IReadOnlyList<string> errors)
    {
        Cards = cards;
        Errors = errors;
    }

    /// <summary>The compiled cards, in the same order they were built. Null when compilation failed.</summary>
    public IReadOnlyList<ICard>? Cards { get; }

    /// <summary>Human-readable problems, empty when compilation succeeded.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>True when <see cref="Cards"/> is populated and <see cref="Errors"/> is empty.</summary>
    public bool Succeeded => Errors.Count == 0;

    /// <summary>Builds a successful result.</summary>
    public static DeckCompileResult Success(IReadOnlyList<ICard> cards) => new(cards, []);

    /// <summary>Builds a failed result carrying one or more problems.</summary>
    public static DeckCompileResult Failure(IReadOnlyList<string> errors) => new(null, errors);
}
