using Microsoft.CodeAnalysis;

namespace RTSPlanner.Notify.Roslyn.Semantic;

/// <summary>
/// Extends <see cref="ITypeSymbol"/> with additional methods.
/// </summary>
public static class TypeSymbolExtensions
{
    /// <summary>
    /// Returns whether the type symbol inherits from the type.
    /// </summary>
    /// <param name="typeSymbol">
    ///     The <see cref="ITypeSymbol"/> to be evaluated.
    /// </param>
    /// <param name="ancestorTypeSymbol">
    ///     The <see cref="ISymbol"/> representing the ancestor type.
    /// </param>
    /// <returns>
    ///     A <see cref="bool"/> representing whether the type symbol inherits from the specified type.
    /// </returns>
    public static bool InheritsFrom(this ITypeSymbol typeSymbol, ISymbol ancestorTypeSymbol)
    {
        var currentBaseTypeSymbol = typeSymbol.BaseType;

        while (currentBaseTypeSymbol is not null)
        {
            if (currentBaseTypeSymbol.Equals(ancestorTypeSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }

            currentBaseTypeSymbol = currentBaseTypeSymbol.BaseType;
        }

        return false;
    }
}