namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Which controller shape a mode produces, and therefore which kind of screen
/// it needs.
///
/// <para>
/// <b>Why this exists.</b> Adding a controller used to require edits in the
/// factory, the manifest, every UI router and every renderer — with nothing
/// enforcing that you'd found them all. The consequence was live and
/// user-facing: when <c>HerdController</c> and <c>ClaimedController</c> were
/// added, MAUI's router fell through to its generic card-turn page (which
/// rejects them) and Console's <c>switch</c> had no default arm at all, so
/// two modes silently did nothing. Console was worse still — <b>four</b>
/// modes, including Monogamy and Day One, which had shipped that way for
/// several versions without anyone noticing.
/// </para>
///
/// <para>
/// A head can now enumerate the families it renders and compare that against
/// what modes actually need — see <c>ControllerFamilyInfo.UnsupportedIn</c>.
/// The compiler still can't force a head to handle a new family, but the gap
/// becomes a value you can assert on in a test rather than a missing switch
/// arm nobody notices until a player picks the mode.
/// </para>
/// </summary>
public enum ControllerFamily
{
    /// <summary>Sequential card-turn: one player, one card, an outcome. Most of the catalogue.</summary>
    CardTurn = 0,

    /// <summary>Hot-seat quiz with lifelines and a prize ladder (Millionaire).</summary>
    Quiz = 1,

    /// <summary>Dice-driven zones for two partners (Monogamy).</summary>
    Monogamy = 2,

    /// <summary>Clock-gated daily campaign (Day One).</summary>
    DailyCampaign = 3,

    /// <summary>Area control — claim and steal territories (Claimed!).</summary>
    AreaControl = 4,

    /// <summary>Everyone answers at once; scoring turns on agreement (Herd).</summary>
    SimultaneousAnswer = 5,

    /// <summary>
    /// Everyone answers the same statements; the session ends in a profile per
    /// player rather than a winner (Big Five).
    ///
    /// <para>
    /// The first family whose controller produces no score at all. That is
    /// exactly why it is a family and not a card-turn mode with an unusual
    /// scoring strategy: <c>IScoringStrategy</c> returns an <c>int</c>, and a
    /// trait assessment needs one running total per dimension plus the bounds
    /// each one could have fallen between.
    /// </para>
    /// </summary>
    TraitProfile = 6,
}

/// <summary>
/// What a head needs to know about a controller family, and how to ask whether
/// it can actually play a given mode.
/// </summary>
public static class ControllerFamilies
{
    /// <summary>
    /// The family a mode will produce, or <c>null</c> when nothing can build a
    /// controller for it.
    ///
    /// <para>
    /// <b>This method is the single source of truth for capability dispatch.</b>
    /// The same set of interfaces used to be tested in three places, in three
    /// different orders — here, in <c>ControllerFactory.CreateAsync</c>, and in
    /// <c>ModeManifestExtensions.GetManifest</c> — while a comment on this method
    /// claimed the first two were "deliberately the same order, so the two cannot
    /// disagree". They were not: Monogamy and Quiz were transposed. It went
    /// unnoticed because no mode implements two capability interfaces, so the
    /// parity test passed on every input it could actually be given.
    /// </para>
    ///
    /// <para>
    /// The manifest now dispatches on this method's result rather than repeating
    /// the chain, which makes that divergence structurally impossible rather than
    /// merely tested for. The order below matches <c>ControllerFactory</c> arm for
    /// arm; <c>DeclaredFamily_MatchesWhatTheFactoryActuallyBuilds_ForEveryMode</c>
    /// still asserts it, and <c>TwoCapabilityMode_ResolvesIdentically...</c> now
    /// asserts it for a mode that implements two, which the catalogue cannot
    /// currently supply.
    /// </para>
    ///
    /// <para>
    /// Returns null rather than guessing. The factory throws
    /// <see cref="NotSupportedException"/> for a mode satisfying none of these,
    /// so any non-null answer here would be a promise the factory won't keep —
    /// which is precisely what the old <c>_ =&gt; CardTurn</c> fallback did: a
    /// head asked, got a confident CardTurn, opened its card-turn screen, and
    /// blew up on create.
    /// </para>
    /// </summary>
    public static ControllerFamily? TryFor(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        // Order matches ControllerFactory.CreateAsync exactly. If you change one,
        // change both — the parity tests will tell you, but only for shapes the
        // catalogue actually contains, so read them as a floor and not a ceiling.
        return mode switch
        {
            IMonogamyDeckProvider => ControllerFamily.Monogamy,
            IQuestionBankProvider => ControllerFamily.Quiz,
            IHerdDeckProvider => ControllerFamily.SimultaneousAnswer,
            IClaimedDeckProvider => ControllerFamily.AreaControl,
            IDailyDeckProvider => ControllerFamily.DailyCampaign,
            ITraitAssessmentProvider => ControllerFamily.TraitProfile,

            // Everything the factory can still build a card-turn controller for.
            // IFlowAwareMode and IDiceProgressionMode change the progression
            // strategy, not the controller type, so they are CardTurn too.
            IGameModeDefinition => ControllerFamily.CardTurn,

            _ => null,
        };
    }

    /// <summary>
    /// The family a mode will produce.
    ///
    /// <para>
    /// Throws <see cref="NotSupportedException"/> when the mode satisfies no
    /// capability interface, matching what <c>ControllerFactory.CreateAsync</c>
    /// does with the same input. Every mode in the catalogue resolves, so this
    /// is unreachable in practice — but a caller that would rather degrade than
    /// throw should use <see cref="TryFor"/> and handle null.
    /// </para>
    /// </summary>
    public static ControllerFamily For(IGameMode mode) =>
        TryFor(mode) ?? throw new NotSupportedException(
            $"No controller family for mode '{mode.Name}' (type: {mode.GetType().Name}). " +
            "Implement IGameModeDefinition, IQuestionBankProvider, IMonogamyDeckProvider, " +
            "IDailyDeckProvider, IClaimedDeckProvider, IHerdDeckProvider or " +
            "ITraitAssessmentProvider on the mode.");

    /// <summary>Every family a mode in the catalogue can currently produce.</summary>
    public static IReadOnlyList<ControllerFamily> All { get; } =
        Enum.GetValues<ControllerFamily>();

    /// <summary>
    /// Returns the modes a head cannot play, given the families it renders.
    ///
    /// <para>
    /// This is the check that would have caught Herd and Claimed before they
    /// shipped unplayable, and it's why it returns the modes rather than just
    /// a bool: a head's test can assert the set is empty and name exactly
    /// what's missing when it isn't.
    /// </para>
    /// </summary>
    public static IReadOnlyList<IGameMode> UnsupportedIn(
        IEnumerable<IGameMode> modes, IEnumerable<ControllerFamily> supportedFamilies)
    {
        ArgumentNullException.ThrowIfNull(modes);
        ArgumentNullException.ThrowIfNull(supportedFamilies);

        var supported = supportedFamilies.ToHashSet();

        // TryFor, not For: a mode no factory can build is unsupported by every
        // head, and listing it is more useful than throwing out of a query whose
        // whole job is to report what a head cannot play.
        return modes
            .Where(m => TryFor(m) is not { } family || !supported.Contains(family))
            .ToList()
            .AsReadOnly();
    }
}
