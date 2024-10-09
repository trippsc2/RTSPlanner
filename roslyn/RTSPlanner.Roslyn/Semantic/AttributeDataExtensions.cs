using System.Linq;
using Microsoft.CodeAnalysis;

namespace RTSPlanner.Roslyn.Semantic;

/// <summary>
/// Extends <see cref="AttributeData"/> with additional methods.
/// </summary>
public static class AttributeDataExtensions
{
    /// <summary>
    /// Returns the value of a named argument that is a value type.
    /// </summary>
    /// <param name="attributeData">
    /// The <see cref="AttributeData"/> to be evaluated.
    /// </param>
    /// <param name="argumentName">
    /// A <see cref="string"/> representing the name of the argument.
    /// </param>
    /// <param name="defaultValue">
    /// A nullable <typeparamref name="T"/> representing the default value of the argument.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    /// <returns>
    /// A nullable <typeparamref name="T"/> representing the value of the argument.
    /// </returns>
    public static T? GetNamedArgumentValueType<T>(
        this AttributeData attributeData,
        string argumentName,
        T? defaultValue = default)
        where T : struct
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(
            namedArgument => namedArgument.Key == argumentName);

        if (argument.Value.IsNull)
        {
            return defaultValue;
        }
        
        if (argument.Value.Kind == TypedConstantKind.Enum)
        {
            return (T)argument.Value.Value!;
        }

        return argument.Value.Value is T value
            ? value
            : defaultValue;
    }
}