using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Domain.Decks;
using TableTop.Core.Domain.Rules;
using TableTop.Core.Engine;

namespace TableTop.Core.Extensions;

/// <summary>
/// Extension methods for registering engine services with Microsoft DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core engine services with default implementations.
    /// </summary>
    public static IServiceCollection AddTableTop(
        this IServiceCollection services)
    {
        // Default random source — non-deterministic, thread-safe singleton.
        // Replace with UseSeededRandom() for reproducible sessions.
        services.AddSingleton<IRandomSource>(_ => SharedRandomSource.Instance);

        services.AddSingleton<IGameFactory, GameFactory>();
        services.AddTransient<IDeckBuilder, DeckBuilder>();
        services.AddTransient<IShuffleStrategy, FisherYatesShuffleStrategy>();

        // Default rules — callers can add more via AddRule<T>()
        services.AddSingleton<IRule, RestrictionRule>();
        services.AddSingleton<IRule, NoDuplicateCardRule>();
        services.AddSingleton<IRule, SkipPlayerRule>();

        services.AddSingleton<IRuleEvaluator>(sp =>
            new RuleEvaluator(sp.GetServices<IRule>()));

        return services;
    }

    /// <summary>
    /// Replaces the default <see cref="IRandomSource"/> with a seeded, deterministic one.
    /// All shuffles, dice rolls, and random card selection become reproducible.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="seed">The seed value. Log or expose this to enable session replay.</param>
    public static IServiceCollection UseSeededRandom(
        this IServiceCollection services, int seed)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRandomSource));
        if (descriptor is not null) services.Remove(descriptor);

        // Seeded Random is NOT thread-safe — register as scoped so each request/session
        // gets its own instance, or as transient for single-threaded console apps.
        services.AddSingleton<IRandomSource>(_ => new SeededRandomSource(seed));
        return services;
    }

    /// <summary>
    /// Adds an additional <see cref="IRule"/> implementation to the engine pipeline.
    /// </summary>
    public static IServiceCollection AddRule<TRule>(this IServiceCollection services)
        where TRule : class, IRule
    {
        services.AddSingleton<IRule, TRule>();
        return services;
    }

    /// <summary>
    /// Registers a custom <see cref="ICardProvider"/> for deck building.
    /// </summary>
    public static IServiceCollection AddCardProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, ICardProvider
    {
        services.AddTransient<ICardProvider, TProvider>();
        return services;
    }
}
