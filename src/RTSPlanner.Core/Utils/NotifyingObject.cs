using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RTSPlanner.Core.Utils;

/// <summary>
/// An abstract base class that implements the INotifyPropertyChanged interface.
/// Provides a method to update property values and notify listeners of changes.
/// </summary>
public abstract class NotifyingObject : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// Updates the value of a property and raises the PropertyChanged event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="backingField">A reference to the field storing the property's value.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="propertyName">
    /// The name of the property. This is optional and will be automatically set by the compiler if not provided.
    /// </param>
    /// <returns>The new value of the property.</returns>
    protected T UpdateValue<T>(ref T backingField, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, newValue))
        {
            return newValue;
        }

        backingField = newValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return newValue;
    }
}