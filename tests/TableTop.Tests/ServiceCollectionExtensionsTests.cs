using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions;
using TableTop.Core.Extensions;

namespace TableTop.Tests;

/// <summary>
/// Covers the fix for the "High" finding on <c>UseSeededRandom</c>: it documents
/// that <see cref="SeededRandomSource"/> wraps a non-thread-safe <see cref="Random"/>,
/// but registered it <c>AddSingleton</c> anyway — one mutable instance shared by
/// every session in the process, and by every transient consumer (DeckBuilder,
/// FisherYatesShuffleStrategy) resolved from any of them.
///
/// Only <c>Microsoft.Extensions.DependencyInjection.Abstractions</c> is referenced
/// here (same as <c>TableTop.Core</c> itself) — no concrete <c>ServiceCollection</c>
/// or <c>ServiceProvider</c> is available to this test project, so
/// <see cref="FakeServiceCollection"/> stands in for the former and the descriptor's
/// factory is invoked directly instead of resolving through the latter.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void UseSeededRandom_RegistersIRandomSource_AsScoped()
    {
        var services = new FakeServiceCollection();
        services.UseSeededRandom(seed: 42);

        var descriptor = services.Single(d => d.ServiceType == typeof(IRandomSource));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped,
            "SeededRandomSource wraps a non-thread-safe Random — a singleton lets " +
            "every concurrently-resolved consumer share, and race on, one mutable instance");
    }

    [Fact]
    public void UseSeededRandom_ReplacesTheDefaultRegistration_NotAddsASecondOne()
    {
        var services = new FakeServiceCollection();
        services.AddTableTop();
        services.UseSeededRandom(seed: 7);

        services.Count(d => d.ServiceType == typeof(IRandomSource)).Should().Be(1,
            "UseSeededRandom must replace AddTableTop's default IRandomSource, not sit alongside it");
    }

    [Fact]
    public void UseSeededRandom_EachResolution_RetainsTheConfiguredSeed()
    {
        const int seed = 12345;
        var services = new FakeServiceCollection();
        services.UseSeededRandom(seed);

        var descriptor = services.Single(d => d.ServiceType == typeof(IRandomSource));
        var factory = descriptor.ImplementationFactory!;

        // Scoped means a fresh instance per session/scope, not a fresh RANDOM
        // seed per resolution — every instance this factory produces must still
        // carry the exact seed the caller configured, so a session's sequence
        // stays reproducible regardless of how many times DI resolves it.
        var first = (SeededRandomSource)factory(null!);
        var second = (SeededRandomSource)factory(null!);

        first.Seed.Should().Be(seed);
        second.Seed.Should().Be(seed);
        first.Should().NotBeSameAs(second,
            "each scope must own its own instance, since Random is not thread-safe to share");
    }
}

/// <summary>
/// Minimal <see cref="IServiceCollection"/> implementation. The interface is
/// exactly <c>IList&lt;ServiceDescriptor&gt;</c> with no members of its own, so a
/// plain list satisfies it — this repo has no reference to the concrete
/// <c>Microsoft.Extensions.DependencyInjection</c> package that ships one.
/// </summary>
internal sealed class FakeServiceCollection : List<ServiceDescriptor>, IServiceCollection
{
}
