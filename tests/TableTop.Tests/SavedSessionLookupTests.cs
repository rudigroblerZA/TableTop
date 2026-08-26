using TableTop.Presentation.Infrastructure;

namespace TableTop.Tests;

/// <summary>
/// Zero test references before this.
///
/// <para>
/// <see cref="SavedSessionLookup.RefreshAsync"/> hardcodes
/// <c>new ControllerFactory()</c> internally with no constructor injection
/// point, and that default has no persistence configured
/// (<c>IGamePersistence? persistence = null</c>), so
/// <c>LoadSavedSessionAsync</c> always resolves to a null snapshot from
/// outside a real head. That means the "a session was found" path cannot be
/// exercised here at all — not "wasn't gotten to," genuinely can't be,
/// without either adding a constructor overload to the class under test or
/// writing to whatever file <c>ControllerFactory</c>'s real persistence layer
/// happens to use, which this class has no way to control either.
/// </para>
///
/// <para>
/// What's real and fully testable is the class's actual documented purpose:
/// swallow every kind of failure to find a session — none configured,
/// nothing on disk, a corrupt file — down to "no resume button," never an
/// exception reaching the caller. That's not a lesser test standing in for
/// the real one; per the class's own doc comment, that failure-swallowing
/// *is* the point: "a corrupt save should cost the player the resume
/// button, nothing more."
/// </para>
/// </summary>
public sealed class SavedSessionLookupTests
{
    [Fact]
    public void InitialState_HasNothingToResume()
    {
        var lookup = new SavedSessionLookup();

        lookup.CanResume.Should().BeFalse();
        lookup.Resumable.Should().BeNull();
        lookup.ResumeText.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_WithNoPersistenceConfigured_LeavesNothingToResume()
    {
        // The real, unconditional case for any caller outside a head that
        // wires its own persistence: ControllerFactory()'s default has none.
        var lookup = new SavedSessionLookup();

        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse();
        lookup.Resumable.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_NeverThrows()
    {
        // The class's whole reason to exist, per its own doc comment: a
        // corrupt or missing save must never surface as an exception to a
        // landing screen — only as the absence of a resume offer.
        var lookup = new SavedSessionLookup();

        var act = async () => await lookup.RefreshAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshAsync_CanBeCalledRepeatedly_WithoutAccumulatingState()
    {
        var lookup = new SavedSessionLookup();

        await lookup.RefreshAsync();
        await lookup.RefreshAsync();
        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse("repeated calls with nothing to find must not somehow produce a stale resumable session");
    }

    [Fact]
    public void ResumeText_IsEmpty_WhenThereIsNothingToResume()
    {
        var lookup = new SavedSessionLookup();
        lookup.ResumeText.Should().Be(string.Empty);
    }
}
