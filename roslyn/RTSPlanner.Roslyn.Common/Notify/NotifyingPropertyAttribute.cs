using System;

namespace RTSPlanner.Roslyn.Common.Notify;

/// <summary>
/// An attribute that marks a field as a property that should notify listeners of changes.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class NotifyingPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets an <see cref="Accessibility"/> representing the access modifier of the property getter.
    /// </summary>
    public Accessibility GetterAccessibility { get; init; } = Accessibility.Public;
    
    /// <summary>
    /// Gets an <see cref="Accessibility"/> representing the access modifier of the property setter.
    /// </summary>
    public Accessibility SetterAccessibility { get; init; } = Accessibility.Public;
}