using Microsoft.Extensions.Logging;
using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Hosting.Diagnostics;

/// <summary>
/// An <see cref="IEngineDiagnostics"/> implementation that writes to an
/// <see cref="ILogger"/>.
///
/// Log levels used:
/// <list type="bullet">
///   <item><see cref="LogLevel.Debug"/> — normal flow (card selected, turn recorded, game start/end)</item>
///   <item><see cref="LogLevel.Trace"/> — rule evaluations (verbose, enable when debugging rule chains)</item>
///   <item><see cref="LogLevel.Warning"/> — no card available after exhausting candidates</item>
/// </list>
///
/// Register in DI:
/// <code>
/// services.AddSingleton&lt;IEngineDiagnostics, LoggerEngineDiagnostics&gt;();
/// // or pass the ILoggerFactory to the constructor directly:
/// new LoggerEngineDiagnostics(loggerFactory.CreateLogger&lt;LoggerEngineDiagnostics&gt;())
/// </code>
/// </summary>
public sealed class LoggerEngineDiagnostics : IEngineDiagnostics
{
    private readonly ILogger _logger;

    /// <summary>Initialises a new <see cref="LoggerEngineDiagnostics"/> instance.</summary>
    public LoggerEngineDiagnostics(ILogger<LoggerEngineDiagnostics> logger) =>
        _logger = logger;

    // overload for direct construction without generic
    /// <summary>Initialises a new <see cref="LoggerEngineDiagnostics"/> instance.</summary>
    public LoggerEngineDiagnostics(ILogger logger) =>
        _logger = logger;

    // ── Rule evaluation ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public void RuleAllowed(IRule rule, ICard card, IPlayer player, int scoreDelta)
    {
        if (!_logger.IsEnabled(LogLevel.Trace)) return;
        _logger.LogTrace(
            "Rule {Rule} allowed card '{Card}' for {Player} (+{Delta} pts)",
            rule.Name, card.Title, player.DisplayName, scoreDelta);
    }

    /// <inheritdoc />
    public void RuleDenied(IRule rule, ICard card, IPlayer player, string reason)
    {
        if (!_logger.IsEnabled(LogLevel.Trace)) return;
        _logger.LogTrace(
            "Rule {Rule} denied card '{Card}' for {Player}: {Reason}",
            rule.Name, card.Title, player.DisplayName, reason);
    }

    // ── Card selection ────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void CardSelected(ICard card, IPlayer player, int round)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        _logger.LogDebug(
            "Round {Round}: '{Card}' ({Difficulty}/{Category}) selected for {Player}",
            round, card.Title, card.Difficulty, card.Category, player.DisplayName);
    }

    /// <inheritdoc />
    public void NoCardAvailable(IPlayer player, int candidatesExhausted, int round)
    {
        _logger.LogWarning(
            "Round {Round}: no eligible card for {Player} after {Count} candidate(s) exhausted — " +
            "turn skipped. Check rule configuration if this happens repeatedly.",
            round, player.DisplayName, candidatesExhausted);
    }

    // ── Turn lifecycle ────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void TurnRecorded(IPlayer player, ICard card, CardOutcome outcome, int scoreDelta, int round)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        _logger.LogDebug(
            "Round {Round}: {Player} → '{Card}' {Outcome} ({Delta:+#;-#;0} pts)",
            round, player.DisplayName, card.Title, outcome, scoreDelta);
    }

    /// <inheritdoc />
    public void TurnUndone(IPlayer player, ICard card, CardOutcome reversed, int scoreRestored)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        _logger.LogDebug(
            "Undo: {Player}'s '{Card}' ({Outcome}) reversed; {Points} pts restored",
            player.DisplayName, card.Title, reversed, scoreRestored);
    }

    // ── Session ───────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void GameStarted(string modeName, int playerCount)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        _logger.LogDebug("Game started: {Mode} with {Players} player(s)", modeName, playerCount);
    }

    /// <inheritdoc />
    public void GameEnded(string modeName, int totalRounds, int totalTurns)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        _logger.LogDebug(
            "Game ended: {Mode} — {Rounds} round(s), {Turns} turn(s) total",
            modeName, totalRounds, totalTurns);
    }
}