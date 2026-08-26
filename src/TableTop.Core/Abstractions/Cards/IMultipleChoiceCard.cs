namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card that presents a question with exactly four labelled answer choices (A–D),
/// exactly one of which is correct.
/// Extends <see cref="ICard"/> so all existing engine machinery handles it transparently (LSP).
/// </summary>
public interface IMultipleChoiceCard : ICard
{
    /// <summary>The four answer options keyed by label A, B, C, D.</summary>
    IReadOnlyDictionary<AnswerLabel, string> Answers { get; }

    /// <summary>The label of the single correct answer.</summary>
    AnswerLabel CorrectAnswer { get; }

    /// <summary>
    /// Returns true when <paramref name="label"/> is the correct answer.
    /// </summary>
    bool IsCorrect(AnswerLabel label) => label == CorrectAnswer;
}

/// <summary>The four answer slot labels used in a multiple-choice card.</summary>
public enum AnswerLabel
{
    /// <summary>First answer choice (A).</summary>
    A,
    /// <summary>Second answer choice (B).</summary>
    B,
    /// <summary>Third answer choice (C).</summary>
    C,
    /// <summary>Fourth answer choice (D).</summary>
    D,
}