namespace TableTop.Hosting.Abstractions;

/// <summary>
/// The current instant, abstracted so day-gated controllers (see
/// <see cref="IDayOneController"/>) can be tested by advancing a fake clock
/// instead of sleeping for real days.
/// </summary>
public interface IClock
{
    /// <summary>The current UTC instant.</summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>The real system clock. Default for every production controller.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
