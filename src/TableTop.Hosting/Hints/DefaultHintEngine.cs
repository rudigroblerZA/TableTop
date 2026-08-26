using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Hosting.Hints;

/// <summary>
/// Analyses up to the last N outcomes and current flow position to produce
/// actionable, gender-aware hints.
///
/// Decision rules (in priority order):
/// <list type="number">
///   <item>Repeated failure/skip (≥2 of last 3) → ease down, slow pace, Strong urgency</item>
///   <item>Repeated success on Hard/Extreme (≥3 of last 3) → push up, Strong urgency</item>
///   <item>Mixed results at current difficulty → stay, Gentle urgency</item>
///   <item>First completion at new level → affirm current level, Gentle urgency</item>
/// </list>
/// </summary>
public sealed class DefaultHintEngine : IHintEngine
{
    private const int WindowSize = Core.TableTopDefaults.Hints.WindowSize;

    /// <summary>Initialises a new <see cref="GenerateHint"/> instance.</summary>
    public NextTurnHint? GenerateHint(IPlayer player, HintContext ctx)
    {
        if (ctx.RecentOutcomes.Count == 0) return null;

        var window = ctx.RecentOutcomes.Take(WindowSize).ToList();
        var difficulties = ctx.RecentDifficulties.Take(WindowSize).ToList();
        var current = ctx.CurrentFlow?.CurrentDifficulty
                          ?? (difficulties.Count > 0 ? difficulties[0] : Difficulty.Easy);

        var completions = window.Count(o => o == CardOutcome.Completed);
        var failures = window.Count(o => o == CardOutcome.Failed);
        var skips = window.Count(o => o == CardOutcome.Skipped);
        var struggling = failures + skips;

        // ── Rule 1: struggling — ease down ────────────────────────────────────
        if (struggling >= 2 && window.Count >= 2)
        {
            var target = DecreaseDifficulty(current);
            return MakeHint(
                player, target,
                ctx.CurrentFlow is not null ? PaceHint.SlowDown : null,
                neutral: $"You've had a tough run — {target} cards might feel better right now.",
                him: $"No shame in stepping back — {target} gives you room to build momentum again.",
                her: $"Give yourself some grace — {target} is where the good stuff lives too.",
                urgency: HintUrgency.Strong,
                reason: "Struggling");
        }

        // ── Rule 2: excelling on hard/extreme — push up ───────────────────────
        if (completions >= 3 && difficulties.Count >= 3
            && difficulties.All(d => d >= Difficulty.Hard))
        {
            var target = IncreaseDifficulty(current);
            if (target != current)
                return MakeHint(
                    player, target,
                    ctx.CurrentFlow is not null ? PaceHint.SpeedUp : null,
                    neutral: $"You're on fire — ready to try {target}?",
                    him: $"You're clearly ready for {target}. Step it up.",
                    her: $"You're absolutely nailing this — {target} is your next move.",
                    urgency: HintUrgency.Strong,
                    reason: "Excelling");
        }

        // ── Rule 3: consistent completions at current level ───────────────────
        if (completions >= 2 && window.Count >= 2)
        {
            var target = IncreaseDifficulty(current);
            if (target != current)
                return MakeHint(
                    player, target,
                    null,
                    neutral: $"You're doing well — a {target} card could be interesting.",
                    him: $"Solid run — {target} might be worth a shot.",
                    her: $"You're doing really well — why not try {target}?",
                    urgency: HintUrgency.Gentle,
                    reason: "ConsistentSuccess");
        }

        // ── Rule 4: single failure after a run of success ─────────────────────
        if (window.Count >= 2 && window[0] != CardOutcome.Completed
            && window.Skip(1).Any(o => o == CardOutcome.Completed))
        {
            // One stumble doesn't mean back off — just acknowledge
            return MakeHint(
                player, current,
                null,
                neutral: $"One blip — {current} is still your level.",
                him: "Shake it off. Same level, fresh start.",
                her: "One off moment doesn't define you — carry on.",
                urgency: HintUrgency.Gentle,
                reason: "OneStumble");
        }

        // ── Rule 5: heavy skipping ────────────────────────────────────────────
        if (ctx.SkipCount >= Core.TableTopDefaults.Hints.HeavySkipThreshold)
        {
            var target = DecreaseDifficulty(current);
            return MakeHint(
                player, target,
                ctx.CurrentFlow is not null ? PaceHint.SlowDown : null,
                neutral: $"You've been skipping a lot — {target} might be a better fit.",
                him: $"No pressure — {target} lets you actually engage rather than skip.",
                her: $"It's worth finding your level — {target} might open things up.",
                urgency: HintUrgency.Moderate,
                reason: "HeavySkipping");
        }

        return null; // No meaningful hint right now
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Difficulty IncreaseDifficulty(Difficulty d) => d switch
    {
        Difficulty.Easy => Difficulty.Medium,
        Difficulty.Medium => Difficulty.Hard,
        Difficulty.Hard => Difficulty.Extreme,
        _ => d,
    };

    private static Difficulty DecreaseDifficulty(Difficulty d) => d switch
    {
        Difficulty.Extreme => Difficulty.Hard,
        Difficulty.Hard => Difficulty.Medium,
        Difficulty.Medium => Difficulty.Easy,
        _ => d,
    };

    private static NextTurnHint MakeHint(
        IPlayer player, Difficulty target, PaceHint? pace,
        string neutral, string him, string her,
        HintUrgency urgency, string reason) =>
        new(
            SuggestedDifficulty: target,
            SuggestedPaceChange: pace,
            NeutralHint: neutral,
            HimHint: him == neutral ? null : him,
            HerHint: her == neutral ? null : her,
            Urgency: urgency,
            Reason: reason);
}