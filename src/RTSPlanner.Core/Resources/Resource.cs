using System;
using RTSPlanner.Notify;

namespace RTSPlanner.Core.Resources;

/// <summary>
/// Represents a game resource within the simulation.
/// </summary>
[NotifyingObject]
public partial class Resource
{
    private readonly int _maximum;
    private readonly int _minimum;
    private readonly int _starting;

    [NotifyingProperty(SetterAccessibility = Accessibility.Protected)]
    private int _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource"/> class.
    /// </summary>
    /// <param name="maximum">
    /// An <see cref="int"/> representing the maximum amount of the resource that can be stored.
    /// </param>
    /// <param name="starting">
    /// An <see cref="int"/> representing the starting amount of the resource.
    /// </param>
    /// <param name="minimum">
    /// An <see cref="int"/> representing the minimum amount of the resource that can be stored.
    /// </param>
    public Resource(int maximum = int.MaxValue, int starting = 0, int minimum = 0)
    {
        if (maximum <= minimum)
        {
            throw new ArgumentException(
                "The maximum amount of the resource must be greater than the minimum amount of the resource.",
                nameof(maximum));
        }
        
        if (starting < minimum || starting > maximum)
        {
            throw new ArgumentException(
                "The starting amount of the resource must be greater than or equal to the minimum amount of the resource and less than or equal to the maximum amount of the resource.",
                nameof(starting));
        }
        
        _maximum = maximum;
        _minimum = minimum;
        _starting = starting;
        _current = starting;
    }

    /// <summary>
    /// Resets the resource to its starting value.
    /// </summary>
    public void Reset()
    {
        Current = _starting;
    }
    
    /// <summary>
    /// Whether the resource can be added to.
    /// </summary>
    /// <returns>
    /// A <see cref="bool"/> representing whether the resource can be added to.
    /// </returns>
    public bool CanAdd()
    {
        return Current < _maximum;
    }
    
    /// <summary>
    /// Whether the resource can be added to without exceeding the maximum.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to add.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> representing whether the resource can be added to without exceeding the maximum.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the amount of the resource to add is less than zero.
    /// </exception>
    public bool CanAddWithoutExceedingMaximum(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "The amount of the resource to add must be greater than or equal to zero.",
                nameof(amount));
        }
        
        return Current + amount <= _maximum;
    }

    /// <summary>
    /// Adds the specified amount of the resource to the current amount.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the amount of the resource to add is less than zero.
    /// </exception>
    public void Add(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "The amount of the resource to add must be greater than or equal to zero.",
                nameof(amount));
        }
        
        Current = Math.Min(_maximum, Current + amount);
    }

    /// <summary>
    /// Whether the resource can be spent.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to spend.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> representing whether the resource can be spent.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the amount of the resource to spend is less than zero.
    /// </exception>
    public bool CanSpend(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "The amount of the resource to spend must be greater than or equal to zero.",
                nameof(amount));
        }
        
        return Current - amount >= _minimum;
    }
    
    /// <summary>
    /// Spends the specified amount of the resource from the current amount.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to spend.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the amount of the resource to spend is less than zero.
    /// </exception>
    public void Spend(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "The amount of the resource to spend must be greater than or equal to zero.",
                nameof(amount));
        }
        
        Current = Math.Max(_minimum, Current - amount); 
    }
}