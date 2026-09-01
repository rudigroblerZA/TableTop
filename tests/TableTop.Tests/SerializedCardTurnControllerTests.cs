using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>
/// Covers the fix for the "Critical" finding on <c>CardTurnController</c>'s
/// threading contract: <see cref="ThreadingGuard.Enabled"/> defaults off in
/// Release, so nothing actually stopped a caller that never marshals onto the
/// owner thread from corrupting session state in the build that ships.
/// <see cref="SerializedCardTurnController"/> closes that gap by serialising
/// access unconditionally, regardless of the guard.
///
/// <para>
/// Shares <c>[Collection("ThreadingGuard")]</c> with <c>ControllerThreadingTests</c>
/// and <c>ThreadingAndDiagnosticsTests</c> for the same reason those two do:
/// <see cref="ThreadingGuard.Enabled"/> is a process-wide static, and these tests
/// explicitly run with it off — the production default this type exists to
/// protect when it is off.
/// </para>
/// </summary>
[Collection("ThreadingGuard")]
public sealed class SerializedCardTurnControllerTests
{
    private static IReadOnlyList<IPlayer> TwoPlayers() =>
        [TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob")];

    [Fact]
    public void Same_thread_usage_behaves_exactly_like_the_unwrapped_controller()
    {
        var wasEnabled = ThreadingGuard_TestAccessor.Enabled;
        ThreadingGuard_TestAccessor.Enabled = false;
        try
        {
            ICardTurnController controller = new SerializedCardTurnController(
                TestFactory.BuildController(TestFactory.MakeCards(10), TwoPlayers(), maxRounds: 3));

            var cardReadyCount = 0;
            controller.CardReady += (_, _) => cardReadyCount++;

            var ex = Record.Exception(() =>
            {
                controller.Start();
                controller.RecordOutcome(CardOutcome.Completed);
            });

            ex.Should().BeNull("a single-threaded caller must see no behavioural change");
            cardReadyCount.Should().BeGreaterThan(0, "events must still fire for a same-thread caller");

            controller.Dispose();
        }
        finally { ThreadingGuard_TestAccessor.Enabled = wasEnabled; }
    }

    [Fact]
    public async Task Concurrent_foreign_thread_calls_are_serialised_without_a_host_supplied_pump()
    {
        // ControllerThreadingTests's own "Timer_driven_outcomes..." test proves the
        // pattern is safe when the HOST builds a single-threaded pump around the raw
        // controller. Most hosts don't. This proves the same safety holds when
        // nothing but this adapter stands between many threads and the controller —
        // and, critically, with ThreadingGuard.Enabled at its Release default (off),
        // which is exactly the configuration the raw controller offered no
        // protection in.
        var wasEnabled = ThreadingGuard_TestAccessor.Enabled;
        ThreadingGuard_TestAccessor.Enabled = false;
        try
        {
            var controller = new SerializedCardTurnController(
                TestFactory.BuildController(TestFactory.MakeCards(400), TwoPlayers(), maxRounds: 200));
            controller.Start();

            const int CallsPerProducer = 25;
            var completed = 0;

            var producers = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            {
                for (var i = 0; i < CallsPerProducer; i++)
                {
                    if (!controller.IsRunning) return;
                    controller.RecordTimedOutcome(CardOutcome.Completed, TimeSpan.FromSeconds(1));
                    Interlocked.Increment(ref completed);
                }
            })).ToArray();

            // Bounded wait via WhenAny — same assertion as Task.WaitAll(timeout)
            // without blocking a pool thread (xUnit1031).
            var allProducers = Task.WhenAll(producers);
            (await Task.WhenAny(allProducers, Task.Delay(TimeSpan.FromSeconds(30))) == allProducers)
                .Should().BeTrue("producers should finish");

            var totalTurns = controller.SessionReport is { } report ? report.TotalTurns : completed;
            totalTurns.Should().Be(completed,
                "every outcome submitted from every thread must be recorded exactly once — " +
                "a mismatch means concurrent calls corrupted state instead of being serialised");

            controller.Dispose();
        }
        finally { ThreadingGuard_TestAccessor.Enabled = wasEnabled; }
    }

    [Fact]
    public async Task Disposing_from_a_foreign_thread_does_not_deadlock()
    {
        var controller = new SerializedCardTurnController(
            TestFactory.BuildController(TestFactory.MakeCards(10), TwoPlayers(), maxRounds: 3));
        controller.Start();

        var disposer = Task.Run(() => controller.Dispose());

        // Bounded wait via WhenAny: a plain await would hang forever on the very
        // deadlock this test exists to catch.
        (await Task.WhenAny(disposer, Task.Delay(TimeSpan.FromSeconds(5))) == disposer).Should().BeTrue(
            "Dispose must not block when called off the owner thread — a UI closing a page does exactly this");
    }
}
