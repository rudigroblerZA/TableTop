using TableTop.Core.Domain.Game;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Extension methods that give any <see cref="IGameMode"/> a manifest,
/// regardless of which interfaces it implements.
///
/// Usage:
/// <code>
/// var manifest = mode.GetManifest();
/// Console.WriteLine($"{mode.Name}: {manifest.TotalCards} cards, {manifest.PlayTimeDisplay}");
/// </code>
/// </summary>
public static class ModeManifestExtensions
{
    // One manifest per mode, built on first request. Manifests are immutable
    // value records, so sharing across threads is safe.
    //
    // ConditionalWeakTable rather than Dictionary (backlog A.3), for two reasons:
    //
    //   • A Dictionary keyed on IGameMode compares by reference and keeps a
    //     strong one, so every mode ever asked for a manifest was pinned for the
    //     life of the process. A host that builds modes per session leaked one
    //     mode and its whole card catalogue per session.
    //   • It is thread-safe on its own, so the lock below is gone.
    //
    // The entry lives exactly as long as the mode does, which is the lifetime
    // the cache actually wanted.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<IGameMode, ModeManifest> _cache = new();

    /// <summary>
    /// Returns the <see cref="ModeManifest"/> for the mode.
    ///
    /// Resolution order:
    /// <list type="number">
    ///   <item>If the mode implements <see cref="IModeManifestProvider"/> — call <c>GetManifest()</c> directly (O(1)).</item>
    ///   <item>Otherwise dispatch on <see cref="ControllerFamilies.TryFor"/> and build from
    ///         whichever deck the controller for that family will actually be handed
    ///         (O(n) once, cached).</item>
    ///   <item>If no family resolves — return an empty manifest rather than throwing,
    ///         so a picker degrades instead of crashing.</item>
    /// </list>
    ///
    /// <para>
    /// The second step deliberately does not test the capability interfaces itself.
    /// Doing so is what let this method disagree with <c>ControllerFactory</c> about
    /// which deck Herd plays — see the comment on the dispatch below.
    /// </para>
    /// </summary>
    public static ModeManifest GetManifest(this IGameMode mode)
    {
        // Fast path — mode pre-computes its own manifest
        if (mode is IModeManifestProvider provider)
            return provider.GetManifest();

        // Check cache before building
        if (_cache.TryGetValue(mode, out var cached))
            return cached;

        // Dispatch on ControllerFamilies.TryFor rather than repeating the
        // interface chain. This used to be its own if/else with its own order —
        // IGameModeDefinition FIRST — and that ordering was a live bug, not a
        // latent one.
        //
        // HerdMode is `BaseGameModeDefinition, IHerdDeckProvider`. It therefore
        // matched the IGameModeDefinition arm before ever reaching the Herd arm
        // below it, so its manifest was built from GetCards([]) while the
        // controller plays GetHerdDeck() — which is GetCards([]) minus the
        // "How To Play" category, stripped deliberately so round one doesn't ask
        // the table to simultaneously answer a page of instructions. Herd's
        // TotalCards was higher than the number of cards it can ever deal, and
        // CategoriesPinnedToStart => ["How To Play"] guaranteed at least one.
        // ArchetypeRegistry.SurpriseMe filters on TotalCards, which is the same
        // blast radius as the Claimed! bug described further down — that one
        // reported too few, this one too many.
        //
        // Deriving from the family means the manifest cannot disagree with the
        // factory about which deck a mode plays. The casts are safe by
        // construction: the family was decided by these very interfaces.
        // Explicitly typed so the switch is target-typed: the arms mix List<ICard>
        // from the .Cast().ToList() calls with IReadOnlyList<ICard> from the deck
        // providers, and the empty collection expression has no type of its own.
        IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> cards =
            ControllerFamilies.TryFor(mode) switch
            {
                ControllerFamily.Monogamy =>
                    ((IMonogamyDeckProvider)mode).GetDeck()
                        .Cast<TableTop.Core.Abstractions.Cards.ICard>().ToList(),

                ControllerFamily.Quiz =>
                    ((IQuestionBankProvider)mode).GetQuestionBank()
                        .Cast<TableTop.Core.Abstractions.Cards.ICard>().ToList(),

                // Simultaneous-answer modes supply a prompt deck. See the note
                // above: this arm existed before AND was unreachable, which are
                // two different problems and only one of them was visible.
                ControllerFamily.SimultaneousAnswer =>
                    ((IHerdDeckProvider)mode).GetHerdDeck(),

                // Area-control modes supply a territory-challenge deck.
                //
                // This arm was missing entirely when IClaimedDeckProvider was
                // added: the interface and its controller landed, nothing taught
                // the manifest about them, and Claimed! reported 0 cards for a
                // full version — silently excluded from every capped SurpriseMe
                // query. Adding a capability interface still means two edits, but
                // the second one is now here, in one place, instead of three.
                ControllerFamily.AreaControl =>
                    ((IClaimedDeckProvider)mode).GetClaimedDeck(),

                ControllerFamily.DailyCampaign =>
                    ((IDailyDeckProvider)mode).GetDailyDeck(),

                // GetCards([]) is safe for all BaseGameModeDefinition subclasses —
                // the players list is only used for restriction pre-filtering,
                // which we skip here to get the full unrestricted catalogue.
                ControllerFamily.CardTurn =>
                    ((IGameModeDefinition)mode).GetCards([]),

                // No capability interface at all — the factory would throw.
                // Degrade rather than crash a picker, which is the call the old
                // `else` made too, now reached only when it is actually right.
                _ => [],
            };

        var manifest = ModeManifestBuilder.Build(cards);

        _cache.AddOrUpdate(mode, manifest);

        return manifest;
    }

    /// <summary>
    /// Empties the manifest cache (backlog A.3).
    ///
    /// Manifests are derived purely from a mode's cards, so this is never needed
    /// for correctness in an app. It exists so a test can start from a known
    /// state: the old custom runner shared this cache across every test in one
    /// process, which is what made <c>SaveAndResume_DoesNotReplayAlreadyPlayedCards</c>
    /// flaky. Real xUnit isolates properly and the flakiness is gone, but a test
    /// that wants to be hermetic should not have to rely on that.
    /// </summary>
    public static void ClearCache() => _cache.Clear();

    /// <summary>
    /// Returns all manifests for every mode reachable from the given sequence,
    /// keyed by mode. Useful for bulk inspection at startup.
    /// </summary>
    public static IReadOnlyDictionary<IGameMode, ModeManifest> GetManifests(
        this IEnumerable<IGameMode> modes) =>
        modes.ToDictionary(m => m, m => m.GetManifest());
}
