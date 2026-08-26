namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Marker interface for modes played in teams rather than as individuals.
///
/// <para>
/// A mode that implements this gets alternating-team turn order automatically
/// (<c>TeamAlternatingPlayerManager</c> instead of the round-robin default),
/// the same way <see cref="IFlowAwareMode"/> and <see cref="IDiceProgressionMode"/>
/// select their progression strategies. Nothing else about the mode has to
/// change: scoring stays per-player and team totals are derived by summing,
/// so every existing scoring strategy keeps working untouched.
/// </para>
///
/// <para>
/// Team membership itself lives in <c>IPlayer.Attributes["team"]</c> — see
/// <c>Teams</c> for the helpers. That keeps it out of <c>IPlayer</c>'s
/// contract and out of the save format, both of which already handle
/// arbitrary attributes.
/// </para>
/// </summary>
public interface ITeamMode
{
    /// <summary>
    /// How many teams this mode wants when a host hasn't assigned them
    /// manually. Two unless a mode has a real reason otherwise.
    /// </summary>
    int PreferredTeamCount => 2;

    /// <summary>
    /// Smallest number of players the mode is playable with. Defaults to four
    /// — two teams of two — because most team games collapse into a
    /// one-versus-one with any fewer, which the individual modes already do
    /// better.
    /// </summary>
    int MinimumPlayersForTeams => 4;
}
