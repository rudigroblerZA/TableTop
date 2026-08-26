namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Supplies the prompt deck for a simultaneous-answer mode — one where
/// <b>everyone answers at once</b> and scoring turns on agreement rather than
/// correctness.
///
/// <para>
/// This is the first mode shape in the engine where there is no single active
/// player. Every other controller asks "whose turn is it?"; this one asks
/// "what did all of you say?". That's why it needs its own controller rather
/// than another progression strategy — the turn-based assumption runs too deep
/// in <c>CardTurnController</c> to parameterise around.
/// </para>
/// </summary>
public interface IHerdDeckProvider
{
    /// <summary>
    /// The prompt deck. Each card is a question everyone answers at the same
    /// time, e.g. "name a breakfast cereal".
    /// </summary>
    IReadOnlyList<Cards.ICard> GetHerdDeck();
}
