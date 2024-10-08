namespace RTSPlanner.Notify.Roslyn.Data;

/// <summary>
/// Represents the data needed to generate a notifying property.
/// </summary>
/// <param name="TypeName">
/// A <see cref="string"/> representing the name of the type of the property.
/// </param>
/// <param name="FieldName">
/// A <see cref="string"/> representing the name of the field backing the property.
/// </param>
/// <param name="PropertyName">
/// A <see cref="string"/> representing the name of the property.
/// </param>
/// <param name="PropertyAccessibilityKeywords">
/// A <see cref="string"/> representing the accessibility keywords of the property.
/// </param>
/// <param name="SetterAccessibilityKeywords">
/// A <see cref="string"/> representing the accessibility keywords of the setter of the property.
/// If the setter has the same accessibility as the getter, this will be <see langword="null"/>. 
/// </param>
public readonly record struct NotifyPropertyInfo(
    string TypeName,
    string FieldName,
    string PropertyName,
    string PropertyAccessibilityKeywords,
    string? SetterAccessibilityKeywords);