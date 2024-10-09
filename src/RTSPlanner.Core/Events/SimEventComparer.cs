using System.Collections.Generic;

namespace RTSPlanner.Core.Events;

/// <summary>
/// Compares <see cref="ISimEvent"/> instances by their <see cref="ISimEvent.Time"/> value.
/// </summary>
public sealed class SimEventComparer : IComparer<ISimEvent>
{
    /// <inheritdoc/>
    public int Compare(ISimEvent? x, ISimEvent? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }
        
        if (ReferenceEquals(null, y))
        {
            return -1;
        }
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (ReferenceEquals(null, x))
        {
            return 1;
        }
        
        return x.Time.CompareTo(y.Time);
    }
}