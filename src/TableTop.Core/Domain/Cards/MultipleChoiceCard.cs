using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// Concrete implementation of <see cref="IMultipleChoiceCard"/>.
/// </summary>
public sealed class MultipleChoiceCard : BaseCard, IMultipleChoiceCard
{
    private readonly Dictionary<AnswerLabel, string> _answers;

    /// <summary>Initialises a new <see cref="MultipleChoiceCard"/> instance.</summary>
    public MultipleChoiceCard(
        Guid id,
        string question,
        string description,
        Dictionary<AnswerLabel, string> answers,
        AnswerLabel correctAnswer,
        Difficulty difficulty,
        string category = "Question",
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
        : base(id, question, description, difficulty, category, tags, restriction)
    {
        if (answers.Count != 4)
            throw new ArgumentException("Exactly four answers (A–D) are required.", nameof(answers));
        if (!answers.ContainsKey(AnswerLabel.A) || !answers.ContainsKey(AnswerLabel.B) ||
            !answers.ContainsKey(AnswerLabel.C) || !answers.ContainsKey(AnswerLabel.D))
            throw new ArgumentException("Answers must include all four labels A, B, C, D.", nameof(answers));

        _answers = new Dictionary<AnswerLabel, string>(answers);
        CorrectAnswer = correctAnswer;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<AnswerLabel, string> Answers => _answers;

    /// <inheritdoc />
    public AnswerLabel CorrectAnswer { get; }

    /// <inheritdoc />
    public bool IsCorrect(AnswerLabel label) => label == CorrectAnswer;

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Convenience factory that takes answers positionally (a, b, c, d).
    /// </summary>
    public static MultipleChoiceCard Create(
        string question,
        string answerA,
        string answerB,
        string answerC,
        string answerD,
        AnswerLabel correctAnswer,
        Difficulty difficulty,
        string category = "Question",
        IEnumerable<string>? tags = null)
    {
        return new MultipleChoiceCard(
            Guid.NewGuid(),
            question,
            $"{answerA} / {answerB} / {answerC} / {answerD}",  // base description for ICard consumers
            new Dictionary<AnswerLabel, string>
            {
                [AnswerLabel.A] = answerA,
                [AnswerLabel.B] = answerB,
                [AnswerLabel.C] = answerC,
                [AnswerLabel.D] = answerD,
            },
            correctAnswer,
            difficulty,
            category,
            tags);
    }
}