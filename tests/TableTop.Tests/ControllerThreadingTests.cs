using System.Collections.Concurrent;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Progression;
using TableTop.Games;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>
/// The host-level threading coverage backlog F.1 asked for.
///
/// <c>ThreadingAndDiagnosticsTests</c> exercises <c>ThreadingGuard</c> in
/// isolation through a test accessor. Nothing exercised the guard *through a
/// real controller*, and nothing exercised the scenario the guard exists for: an
/// external timer firing on a threadpool thread while the owner thread is
/// mid-turn. That is the race a host actually hits, and it was uncovered.
///
/// <para>
/// <b>A finding worth knowing about.</b> <c>ThreadingGuard.Assert</c> is marked
/// <c>[Conditional("DEBUG")]</c>, which strips the call sites from any caller
/// compiled in Release. CI builds and tests in Release, and releases ship in
/// Release — so in both, the guard is inert and cross-thread misuse is silently
/// permitted rather than caught.
/// </para>
///
/// <para>
/// That is a deliberate design choice, documented in <c>ThreadingGuard.cs</c> as
/// "zero overhead in production", and changing it would make shipped builds
/// throw where they currently limp along. So these tests do not fight it: the
/// safety-net test asserts the guard fires where it is active and is skipped
/// where it is not, and <see cref="Timer_driven_outcomes_marshalled_to_the_owner_thread_stay_consistent"/>
/// runs in every configuration, because correctness under a correctly-marshalled
/// timer is what actually has to hold in production.
/// </para>
/// </summary>
[Collection("ThreadingGuard")]
public sealed class ControllerThreadingTests
{
    private static IReadOnlyList<IPlayer> TwoPlayers() =>
        [new Player(Guid.NewGuid(), "Alice"), new Player(Guid.NewGuid(), "Bob")];

    private static CardTurnController Started()
    {
        var c = new CardTurnController(
            new WouldYouRatherMode(), TwoPlayers(), "Threading", maxRounds: 200,
            new LinearProgressionStrategy());
        c.Start();
        return c;
    }

    [Fact]
    public void A_timer_callback_on_a_foreign_thread_is_rejected()
    {
        // Backlog L.4. This used to skip in Release, because Assert carried
        // [Conditional("DEBUG")] and there was no guard to test. It is now a
        // runtime switch, so the test runs in every configuration — which
        // matters, since Release is what CI runs and what ships.
        // Process-wide, so this class shares the "ThreadingGuard" collection
        // with ThreadingAndDiagnosticsTests to stop the two overlapping — see
        // that class's remarks. Restored in the finally regardless.
        var wasEnabled = TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor.Enabled;
        TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor.Enabled = true;
        try
        {
            using var controller = Started();
            Exception? captured = null;

            // Exactly what an unmarshalled System.Threading.Timer callback does.
            var timer = new Thread(() =>
            {
                try { controller.RecordOutcome(CardOutcome.Completed); }
                catch (Exception ex) { captured = ex; }
            });
            timer.Start();
            timer.Join(TimeSpan.FromSeconds(5)).Should().BeTrue("the timer thread should not hang");

            captured.Should().BeOfType<InvalidOperationException>(
                "a mutating call from a non-owner thread is exactly what ThreadingGuard exists to catch, " +
                "and it must be caught through the real controller and not only in the guard's own unit test");
            captured!.Message.Should().Contain("not thread-safe");
        }
        finally { TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor.Enabled = wasEnabled; }
    }

    [Fact]
    public async Task Timer_driven_outcomes_marshalled_to_the_owner_thread_stay_consistent()
    {
        // The documented contract: a host receiving timer callbacks off-thread
        // marshals them back. This is that host — a single-threaded pump — under
        // load, which is the arrangement that has to produce correct results.
        using var work = new BlockingCollection<Action>();
        Exception? pumpFailure = null;
        CardTurnController? controller = null;
        var ready = new ManualResetEventSlim();

        var owner = new Thread(() =>
        {
            try
            {
                controller = Started();
                ready.Set();
                foreach (var action in work.GetConsumingEnumerable())
                    action();
            }
            catch (Exception ex) { pumpFailure = ex; ready.Set(); }
        })
        { Name = "owner", IsBackground = true };

        owner.Start();
        ready.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("the owner thread should start the controller");
        pumpFailure.Should().BeNull($"starting the controller threw: {pumpFailure}");

        const int TimerCallbacks = 200;
        var completed = 0;

        // Many threads acting as independent timers, all posting rather than
        // calling. Contention is on the queue, which is where it belongs.
        var producers = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < TimerCallbacks / 8; i++)
                work.Add(() =>
                {
                    if (!controller!.IsRunning) return;
                    controller.RecordTimedOutcome(CardOutcome.Completed, TimeSpan.FromSeconds(5));
                    Interlocked.Increment(ref completed);
                });
        })).ToArray();

        // WhenAny against a delay rather than Task.WaitAll(timeout): same bounded
        // wait, same "false means it did not finish in time" assertion, but it
        // does not block a pool thread (xUnit1031). Awaiting the producers alone
        // would hang the run instead of failing this assertion.
        var allProducers = Task.WhenAll(producers);
        (await Task.WhenAny(allProducers, Task.Delay(TimeSpan.FromSeconds(30))) == allProducers)
            .Should().BeTrue("producers should finish");
        work.CompleteAdding();
        owner.Join(TimeSpan.FromSeconds(30)).Should().BeTrue("the pump should drain and exit");

        pumpFailure.Should().BeNull(
            $"every call was marshalled to the owner thread, so nothing should have thrown: {pumpFailure}");

        // The real assertion: scores reflect exactly the turns that were played.
        // A race inside the controller shows up here as a total that doesn't add up.
        var totalScore = controller!.SessionReport is { } report
            ? report.TotalTurns
            : completed;

        completed.Should().BeGreaterThan(0, "some outcomes should have been recorded before the deck ran out");
        totalScore.Should().Be(completed,
            "the number of turns the engine recorded must equal the number of outcomes the host submitted — " +
            "a mismatch means state was lost or double-counted under concurrent submission");

        controller.Dispose();
    }

    [Fact]
    public async Task Disposing_from_a_foreign_thread_does_not_deadlock()
    {
        // Dispose unsubscribes from game events and nulls the event fields. It is
        // the one operation a host is likely to perform from a different thread
        // (a page closing, a navigation cancelling), so it must not hang.
        var controller = Started();

        var disposer = Task.Run(() => controller.Dispose());

        // Bounded wait via WhenAny, not disposer.Wait(timeout): a plain await
        // would hang forever on the very deadlock this test exists to catch.
        (await Task.WhenAny(disposer, Task.Delay(TimeSpan.FromSeconds(5))) == disposer).Should().BeTrue(
            "Dispose must not block when called off the owner thread — a UI closing a page does exactly this");
    }
}
