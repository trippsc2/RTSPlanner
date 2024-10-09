using System;
using System.ComponentModel;
using RTSPlanner.Roslyn.Notify;

namespace RTSPlanner.Core.Resources;

/// <summary>
/// Represents a capacity (Food/Supply/etc.) resource in the simulation.
/// </summary>
[NotifyingObject]
public partial class CapacityResource
{
    private readonly int _startingUsed;
    private readonly int _startingMaximum;
    private readonly int _startingMaximumInProduction;
    private readonly int _hardMaximum;
    
    private int _maximumInProduction;

    [NotifyingProperty(SetterAccessibility = Accessibility.Protected)]
    private int _used;
    
    [NotifyingProperty(GetterAccessibility = Accessibility.Protected)]
    private int _rawMaximum;

    [NotifyingProperty(SetterAccessibility = Accessibility.Protected)]
    private int _maximum;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityResource"/> class.
    /// </summary>
    /// <param name="startingUsed">
    /// An <see cref="int"/> representing the starting amount of the resource used.
    /// </param>
    /// <param name="startingMaximum">
    /// An <see cref="int"/> representing the starting maximum amount of the resource.
    /// </param>
    /// <param name="hardMaximum">
    /// An <see cref="int"/> representing the maximum amount of the resource that can be stored.
    /// </param>
    /// <param name="startingMaximumInProduction">
    /// An <see cref="int"/> representing the starting maximum amount of the resource in production.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the starting amount of the resource used is less than zero.
    /// Thrown when the starting maximum amount of the resource is less than zero.
    /// Thrown when the hard maximum amount of the resource that can be stored is less than the starting maximum amount.
    /// Thrown when the starting maximum amount of the resource in production is less than zero.
    /// Thrown when the hard maximum amount of the resource that can be stored is less than the starting maximum amount plus the starting maximum amount in production.
    /// </exception>
    public CapacityResource(
        int startingUsed,
        int startingMaximum,
        int hardMaximum,
        int startingMaximumInProduction = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startingMaximum);
        ArgumentOutOfRangeException.ThrowIfNegative(startingUsed);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startingUsed, startingMaximum);
        ArgumentOutOfRangeException.ThrowIfNegative(startingMaximumInProduction);
        ArgumentOutOfRangeException.ThrowIfLessThan(hardMaximum, startingMaximum);
        
        _startingUsed = startingUsed;
        _startingMaximum = startingMaximum;
        _startingMaximumInProduction = startingMaximumInProduction;
        _hardMaximum = hardMaximum;
        
        _used = startingUsed;
        _rawMaximum = startingMaximum;
        _maximum = _rawMaximum;
        _maximumInProduction = startingMaximumInProduction;
        
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Resets the resource to its starting state.
    /// </summary>
    public void Reset()
    {
        Used = _startingUsed;
        RawMaximum = _startingMaximum;
        _maximumInProduction = _startingMaximumInProduction;
    }
    
    /// <summary>
    /// Whether the resource can be used.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to use.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> representing whether the resource can be used.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to use is less than zero.
    /// </exception>
    public bool CanUse(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        return Used + amount <= Math.Min(_hardMaximum, RawMaximum + _maximumInProduction);
    }

    /// <summary>
    /// Whether the resource can be used now.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to use.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> representing whether the resource can be used now.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to use is less than zero.
    /// </exception>
    public bool CanUseNow(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        return Used + amount <= Maximum;
    }
    
    /// <summary>
    /// Consumes the specified amount of the resource.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to consume.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to consume is less than zero.
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when the amount of the resource to consume would consume greater than the current maximum.
    /// </exception>
    public void Use(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (amount + Used > Maximum)
        {
            throw new ApplicationException(
                "The amount of the resource to use must be less than or equal to the current maximum.");
        }
        
        Used += amount;
    }

    /// <summary>
    /// Removes the consumed amount of the resource.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to free.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to free is less than zero.
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when the amount of the resource to free is greater than the amount of the resource used.
    /// </exception>
    public void Free(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        if (Used < amount)
        {
            throw new ApplicationException(
                "The amount of the resource to free must be less than or equal to the amount of the resource used.");
        }
        
        Used -= amount;
    }

    /// <summary>
    /// Starts the production of the resource.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to produce.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to produce is less than zero.
    /// </exception>
    public void StartMaximumProduction(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        _maximumInProduction += amount;
    }
    
    /// <summary>
    /// Cancels the production of the resource.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to cancel.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to cancel is less than zero.
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when the amount of the resource to cancel is greater than the amount of the resource in production.
    /// </exception>
    public void CancelMaximumProduction(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        if (amount > _maximumInProduction)
        {
            throw new ApplicationException(
                "The amount of the resource to cancel must be less than or equal to the amount of the resource in production.");
        }
        
        _maximumInProduction -= amount;
    }
    
    /// <summary>
    /// Finishes the production of the resource.
    /// </summary>
    /// <param name="amount">
    /// An <see cref="int"/> representing the amount of the resource to finish.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the amount of the resource to finish is less than zero.
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when the amount of the resource to finish is greater than the amount of the resource in production.
    /// </exception>
    public void FinishMaximumProduction(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        if (amount > _maximumInProduction)
        {
            throw new ApplicationException(
                "The amount of the resource to finish must be less than or equal to the amount of the resource in production.");
        }
        
        _maximumInProduction -= amount;
        RawMaximum += amount;
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RawMaximum))
        {
            return;
        }
        
        Maximum = Math.Min(_hardMaximum, RawMaximum);
    }
}