using System.Collections.Generic;

namespace RTSPlanner.Core.Events;

/// <summary>
/// Represents the queue of simulation events.
/// </summary>
public sealed class SimEventQueue
{
    private readonly List<ISimEvent> _events = [];
    
    /// <summary>
    /// Adds an event to the queue and places it appropriately.
    /// </summary>
    /// <param name="simEvent">
    ///     The <see cref="ISimEvent"/> to add to the queue.
    /// </param>
    public void AddEvent(ISimEvent simEvent)
    {
        _events.Add(simEvent);
        _events.Sort(new SimEventComparer());
    }
    
    /// <summary>
    /// Adds multiple events to the queue and places them appropriately.
    /// </summary>
    /// <param name="events">
    ///     The <see cref="IEnumerable{T}"/> of <see cref="ISimEvent"/> to add to the queue.
    /// </param>
    public void AddEvents(IEnumerable<ISimEvent> events)
    {
        _events.AddRange(events);
        _events.Sort(new SimEventComparer());
    }
    
    /// <summary>
    /// Removes the specified event from the queue.
    /// </summary>
    /// <param name="simEvent">
    ///     The <see cref="ISimEvent"/> to remove from the queue.
    /// </param>
    public void RemoveEvent(ISimEvent simEvent)
    {
        _events.Remove(simEvent);
    }
    
    /// <summary>
    /// Returns the next event in the queue and then removes it from the queue.
    /// </summary>
    /// <returns>
    ///     The next <see cref="ISimEvent"/> in the queue, if one exists.
    /// </returns>
    public ISimEvent? DequeueNextEvent()
    {
        if (_events.Count == 0)
        {
            return null;
        }
        
        var nextEvent = _events[0];
        _events.RemoveAt(0);
        
        return nextEvent;
    }
    
    /// <summary>
    /// Clears the queue of all events.
    /// </summary>
    public void Clear()
    {
        _events.Clear();
    }
}