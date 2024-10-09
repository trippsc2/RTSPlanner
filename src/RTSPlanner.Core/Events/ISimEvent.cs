using System;

namespace RTSPlanner.Core.Events;

/// <summary>
/// Represents an event to be simulated.
/// </summary>
public interface ISimEvent
{
    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representing the time at which the event should be executed.
    /// </summary>
    TimeSpan Time { get; }

    /// <summary>
    /// Executes simulating the event.
    /// </summary>
    void Execute();
}