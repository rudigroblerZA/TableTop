namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// Provides cards from an external or registered source (database, file, AI, etc.).
/// Implement this to add new deck data providers without touching engine internals (OCP).
/// </summary>
public interface ICardProvider
{
    /// <summary>Returns all cards available from this provider.</summary>
    Task<IReadOnlyList<ICard>> GetCardsAsync(CancellationToken cancellationToken = default);
}
