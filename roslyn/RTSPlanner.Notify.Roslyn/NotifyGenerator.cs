using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RTSPlanner.Notify.Roslyn.Data;
using RTSPlanner.Notify.Roslyn.Semantic;
using RTSPlanner.Notify.Roslyn.Syntactic;

namespace RTSPlanner.Notify.Roslyn;

/// <summary>
/// Generates the source code to implement the <see cref="INotifyPropertyChanged"/> interface for classes marked with
/// the <see cref="NotifyingObjectAttribute"/> and fields marked with the <see cref="NotifyingPropertyAttribute"/>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class NotifyGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{typeof(NotifyingObjectAttribute).Namespace}.{nameof(NotifyingObjectAttribute)}",
                CouldBeMarkedClass,
                TransformMarkedClass)
            .Collect();
        
        context.RegisterSourceOutput(provider, GenerateCode);
    }

    private static bool CouldBeMarkedClass(SyntaxNode node, CancellationToken token)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.IsPartial() &&
               classDeclaration.IsNotStatic();
    }

    private static NotifyClassInfo TransformMarkedClass(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token)
    {
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : classSymbol.ContainingNamespace.ToDisplayString();

        var nameFormat = new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);
        var className = classSymbol.ToDisplayString(nameFormat);

        var fieldSymbols = classSymbol.GetMembers()
            .Where(symbol => symbol.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .ToList();

        var properties = TransformFieldSymbols(fieldSymbols);
        
        return new NotifyClassInfo(namespaceName, className, properties);
    }

    private static ImmutableArray<NotifyPropertyInfo> TransformFieldSymbols(List<IFieldSymbol> fieldSymbols)
    {
        var builder = ImmutableArray.CreateBuilder<NotifyPropertyInfo>();

        foreach (var property in fieldSymbols.Select(TransformFieldSymbol))
        {
            if (property is null)
            {
                continue;
            }

            builder.Add(property.Value);
        }
        
        return builder.ToImmutable();
    }

    private static NotifyPropertyInfo? TransformFieldSymbol(IFieldSymbol fieldSymbol)
    {
        if (!fieldSymbol
            .HasAttribute(
                out var attribute,
                nameof(NotifyingPropertyAttribute),
                typeof(NotifyingPropertyAttribute).Namespace))
        {
            return null;
        }
        
        var typeName = fieldSymbol.Type.ToDisplayString();
        var fieldName = fieldSymbol.Name;

        if (!fieldName.StartsWith("_"))
        {
            return null;
        }
        
        var propertyName = ConvertFieldNameToPropertyName(fieldName);
        
        var getterAccessibility = attribute.GetNamedArgumentValueType<Accessibility>(
                nameof(NotifyingPropertyAttribute.GetterAccessibility),
                Accessibility.Public)!.Value;
        var setterAccessibility = attribute.GetNamedArgumentValueType<Accessibility>(
                nameof(NotifyingPropertyAttribute.SetterAccessibility),
                getterAccessibility)!.Value;

        if (setterAccessibility < getterAccessibility ||
            (getterAccessibility == Accessibility.Protected &&
             setterAccessibility == Accessibility.Internal))
        {
            return null;
        }

        var propertyAccessibilityKeywords = ConvertAccessibilityToKeywords(getterAccessibility);

        var setterAccessibilityKeywords = getterAccessibility == setterAccessibility
            ? null
            : ConvertAccessibilityToKeywords(setterAccessibility);

        return new NotifyPropertyInfo(
            typeName,
            fieldName,
            propertyName,
            propertyAccessibilityKeywords,
            setterAccessibilityKeywords);
    }
    
    private static string ConvertFieldNameToPropertyName(string fieldName)
    {
        var fieldNameWithoutUnderscore = fieldName[1..];
        
        return fieldNameWithoutUnderscore[0].ToString().ToUpper() + fieldNameWithoutUnderscore[1..];
    }

    private static string ConvertAccessibilityToKeywords(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.ProtectedInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.PrivateProtected => "private protected",
            Accessibility.Private => "private",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<NotifyClassInfo> classes)
    {
        foreach (var classInfo in classes)
        {
            GenerateClassCode(context, classInfo);
        }
    }

    private static void GenerateClassCode(
        SourceProductionContext context,
        NotifyClassInfo classInfo)
    {
        var hintName = $"{classInfo.NamespaceName}.{ConvertClassNameToHintNameFormat(classInfo.ClassName)}.Notify.g.cs";
        
        // lang=cs
        var header =
            $$"""
              // <auto-generated />
              #nullable enable
              
              namespace {{classInfo.NamespaceName}};
              
              partial class {{classInfo.ClassName}} : global::System.ComponentModel.INotifyPropertyChanged
              {
                  public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
              
              """;

        var stringBuilder = new StringBuilder(header);
        
        foreach (var propertyCode in classInfo.Properties.Select(GeneratePropertyCode))
        {
            stringBuilder.Append(propertyCode);
        }
        
        stringBuilder.Append("}");
        
        context.AddSource(hintName, stringBuilder.ToString());
    }

    private static string ConvertClassNameToHintNameFormat(string className)
    {
        const string genericPattern = "<([^>]+)>";
        var match = Regex.Match(className, genericPattern);

        if (!match.Success)
        {
            return className;
        }

        var genericArguments = match.Groups[1].Value;
        var formattedArguments = "." + genericArguments.Replace(",", ".");

        return Regex.Replace(className, genericPattern, formattedArguments);
    }

    private static string GeneratePropertyCode(NotifyPropertyInfo property)
    {
        var setterAccessibilityKeywords = property.SetterAccessibilityKeywords is not null
            ? $"{property.SetterAccessibilityKeywords} "
            : null;
        
        // lang=cs
        return $$"""
                     
                     {{property.PropertyAccessibilityKeywords}} {{property.TypeName}} {{property.PropertyName}}
                     {
                         get => this.{{property.FieldName}};
                         {{setterAccessibilityKeywords}}set
                         {
                             if (global::System.Collections.Generic.EqualityComparer<{{property.TypeName}}>.Default.Equals({{property.FieldName}}, value))
                             {
                                 return;
                             }
                             
                             this.{{property.FieldName}} = value;
                             this.PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs("{{property.PropertyName}}"));
                         }
                     }
                 
                 """;
    }
}