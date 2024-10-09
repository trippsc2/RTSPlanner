using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RTSPlanner.Roslyn.Semantic;
using RTSPlanner.Roslyn.Syntactic;

namespace RTSPlanner.Roslyn.Notify;

/// <summary>
/// Analyzes the use of the NotifyingObjectAttribute and NotifyingPropertyAttribute attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NotifyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for when the NotifyingObjectAttribute is used on a non-partial type.
    /// </summary>
    public static DiagnosticDescriptor NotifyingObjectNotPartialType { get; } = new(
        "RTSPlannerNotify001",
        "NotifyingObjectAttribute cannot be used on a non-partial type.",
        "The class '{0}' is marked with the NotifyingObjectAttribute but is not a partial type.",
        "RTSPlanner.Roslyn",
        DiagnosticSeverity.Error,
        true);
    
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for when the NotifyingObjectAttribute is used on a non-partial type.
    /// </summary>
    public static DiagnosticDescriptor NotifyingObjectStaticType { get; } = new(
        "RTSPlannerNotify002",
        "NotifyingObjectAttribute cannot be used on a static type.",
        "The class '{0}' is marked with the NotifyingObjectAttribute but is a static type.",
        "RTSPlanner.Roslyn",
        DiagnosticSeverity.Error,
        true);

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for when the NotifyingPropertyAttribute is used outside a class marked
    /// with NotifyingObjectAttribute.
    /// </summary>
    public static DiagnosticDescriptor NotifyingPropertyOutsideOfNotifyingObject { get; } = new(
        "RTSPlannerNotify003",
        "NotifyingPropertyAttribute cannot be used outside of a class marked with NotifyingObjectAttribute.",
        "The field '{0}' is marked with the NotifyingPropertyAttribute but is not a member of a class marked with NotifyingObjectAttribute. The property will not be generated.",
        "RTSPlanner.Roslyn",
        DiagnosticSeverity.Warning,
        true);
    
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for when the NotifyingPropertyAttribute is used with a
    /// SetterAccessibility that is less accessible than the GetterAccessibility.
    /// </summary>
    public static DiagnosticDescriptor NotifyingPropertySetterLessAccessibleThanGetter { get; } = new(
        "RTSPlannerNotify004",
        "The SetterAccessibility of the field marked with NotifyingPropertyAttribute cannot be less accessible than the GetterAccessibility.",
        "The SetterAccessibility of the field '{0}' is less accessible than the GetterAccessibility.",
        "RTSPlanner.Roslyn",
        DiagnosticSeverity.Error,
        true);
    
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for when the field marked with NotifyingPropertyAttribute is not
    /// prefixed with an underscore.
    /// </summary>
    public static DiagnosticDescriptor NotifyingFieldNameMustStartWithUnderscore { get; } = new(
        "RTSPlannerNotify005",
        "The field marked with NotifyingPropertyAttribute must start with an underscore.",
        "The field '{0}' marked with NotifyingPropertyAttribute must start with an underscore.",
        "RTSPlanner.Roslyn",
        DiagnosticSeverity.Error,
        true);
    
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        NotifyingObjectNotPartialType,
        NotifyingObjectStaticType,
        NotifyingPropertyOutsideOfNotifyingObject,
        NotifyingPropertySetterLessAccessibleThanGetter,
        NotifyingFieldNameMustStartWithUnderscore
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (!context.Symbol
                .HasAttribute(
                    out var attribute,
                    "NotifyingObjectAttribute",
                    "RTSPlanner.Roslyn.Notify"))
        {
            return;
        }

        var attributeSyntax = (AttributeSyntax)attribute
            .ApplicationSyntaxReference!
            .GetSyntax();
        
        foreach (var syntaxReference in context.Symbol.DeclaringSyntaxReferences)  
        {
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration)
            {
                continue;
            }
            
            if (classDeclaration.IsNotPartial())
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NotifyingObjectNotPartialType,
                        attributeSyntax.GetLocation(),
                        classDeclaration.Identifier.Text));
            }
            
            if (classDeclaration.IsStatic())
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NotifyingObjectStaticType,
                        attributeSyntax.GetLocation(),
                        classDeclaration.Identifier.Text));
            }
        }
    }

    private static void AnalyzeField(SymbolAnalysisContext context)
    {
        if (!context.Symbol
                .HasAttribute(
                    out var attribute,
                    "NotifyingPropertyAttribute",
                    "RTSPlanner.Roslyn.Notify"))
        {
            return;
        }
        
        var attributeSyntax = (AttributeSyntax)attribute
            .ApplicationSyntaxReference!
            .GetSyntax();

        if (!context.Symbol.ContainingType
                .HasAttribute(
                    out _,
                    "NotifyingObjectAttribute",
                    "RTSPlanner.Roslyn.Notify"))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    NotifyingPropertyOutsideOfNotifyingObject,
                    attributeSyntax.GetLocation(),
                    context.Symbol.Name));
            return;
        }

        var getterAccessibility = attribute.GetNamedArgumentValueType<Accessibility>(
            "GetterAccessibility",
            Accessibility.Public)!.Value;
        var setterAccessibility = attribute.GetNamedArgumentValueType<Accessibility>(
            "SetterAccessibility",
            getterAccessibility)!.Value;

        if (setterAccessibility < getterAccessibility ||
            (getterAccessibility == Accessibility.Protected &&
             setterAccessibility == Accessibility.Internal))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    NotifyingPropertySetterLessAccessibleThanGetter,
                    attributeSyntax.GetLocation(),
                    context.Symbol.Name));
        }
        
        if (!context.Symbol.Name.StartsWith("_"))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    NotifyingFieldNameMustStartWithUnderscore,
                    attributeSyntax.GetLocation(),
                    context.Symbol.Name));
        }
    }
}