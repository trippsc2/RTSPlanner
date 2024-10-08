using System.Collections.Immutable;

namespace RTSPlanner.Notify.Roslyn.Data;

/// <summary>
/// Represents the data needed to generate notifying properties for a class.
/// </summary>
/// <param name="NamespaceName">
/// A <see cref="string"/> representing the name of the namespace in which the class is defined.
/// If the class is not in a namespace, this will be <see langword="null"/>.
/// </param>
/// <param name="ClassName">
/// A <see cref="string"/> representing the name of the class.
/// </param>
/// <param name="Properties">
/// An <see cref="ImmutableArray{T}"/> of <see cref="NotifyPropertyInfo"/> representing the properties to generate.
/// </param>
public readonly record struct NotifyClassInfo(
    string? NamespaceName,
    string ClassName,
    ImmutableArray<NotifyPropertyInfo> Properties);