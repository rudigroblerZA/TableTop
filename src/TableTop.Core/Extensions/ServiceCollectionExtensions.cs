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

        // SeededRandomSource wraps System.Random, which is NOT thread-safe. It used
        // to be registered AddSingleton — a comment right here said "register as
        // scoped", but the line below it did the opposite, so every consumer in the
        // process (every session, potentially running concurrently) shared one
        // mutable Random instance. Scoped ties one instance, and the one seed it was
        // built from, to one session: every transient consumer resolved within a
        // session's scope (DeckBuilder, FisherYatesShuffleStrategy, …) shares that
        // session's continuing sequence — a shuffle and a later dice roll draw from
        // the same reproducible stream instead of two independent ones seeded
        // identically — while two different sessions never share, and so can never
        // race on, the same instance. A host with no scope-per-session concept yet
        // (nothing in this repo creates an IServiceScope today) still resolves this
        // exactly once from the root scope, same as the singleton did — this only
        // starts mattering, safely, the moment a host adopts one scope per session.
        services.AddScoped<IRandomSource>(_ => new SeededRandomSource(seed));
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
