using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using RTSPlanner.Roslyn.Common.Notify;

namespace RTSPlanner.Roslyn.UnitTests;

[ExcludeFromCodeCoverage]
public sealed class AnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    // ReSharper disable once StaticMemberInGenericType
    private static (string filename, string content)[] EmptyGeneratedSources { get; } = [];
    
    public static DiagnosticResult Diagnostic()
    {
        return new DiagnosticResult();
    }

    public static DiagnosticResult Diagnostic(string id, DiagnosticSeverity severity)
    {
        return new DiagnosticResult(id, severity);
    }

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
    {
        return new DiagnosticResult(descriptor);
    }

    public static async Task VerifyAnalyzerAsync(string source)
    {
        await VerifyAnalyzerAsync(source, DiagnosticResult.EmptyDiagnosticResults);
    }

    public static async Task VerifyAnalyzerAsync(string source, (string filename, string content) generatedSource)
    {
        await VerifyAnalyzerAsync(
            source,
            DiagnosticResult.EmptyDiagnosticResults,
            [generatedSource]);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        params (string filename, string content)[] generatedSources)
    {
        await VerifyAnalyzerAsync(source, DiagnosticResult.EmptyDiagnosticResults, generatedSources);
    }

    public static async Task VerifyAnalyzerAsync(string source, DiagnosticResult diagnostic)
    {
        await VerifyAnalyzerAsync(source, new[] { diagnostic }, EmptyGeneratedSources);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] diagnostics)
    {
        await VerifyAnalyzerAsync(source, diagnostics, EmptyGeneratedSources);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        DiagnosticResult diagnostic,
        (string filename, string content) generatedSource)
    {
        await VerifyAnalyzerAsync(source, new[] { diagnostic }, [generatedSource]);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        IEnumerable<DiagnosticResult> diagnostics,
        (string filename, string content) generatedSource)
    {
        await VerifyAnalyzerAsync(source, diagnostics, [generatedSource]);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        DiagnosticResult diagnostic,
        params (string filename, string content)[] generatedSources)
    {
        await VerifyAnalyzerAsync(source, new[] { diagnostic }, generatedSources);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static async Task VerifyAnalyzerAsync(
        string source,
        IEnumerable<DiagnosticResult> diagnostics,
        params (string filename, string content)[] generatedSources)
    {
        var test = new AnalyzerTest<TAnalyzer>
        {
            TestState =
            {
                Sources = { source },
                AdditionalReferences = { typeof(NotifyingPropertyAttribute).Assembly },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        foreach (var (filename, content) in generatedSources)
        {
            test.TestState
                .GeneratedSources
                .Add((typeof(TAnalyzer), filename, SourceText.From(content, Encoding.UTF8)));
        }

        test.ExpectedDiagnostics.AddRange(diagnostics);

        await test.RunAsync(CancellationToken.None);
    }
    
    protected override CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = base.CreateCompilationOptions();
        return compilationOptions
            .WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions
                    .SetItems(GetNullableWarnings()));
    }

    protected override ParseOptions CreateParseOptions()
    {
        return ((CSharpParseOptions)base.CreateParseOptions())
            .WithLanguageVersion(LanguageVersion.Latest);
    }

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarnings()
    {
        var args = new[] { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default
            .Parse(
                args,
                Environment.CurrentDirectory,
                Environment.CurrentDirectory);

        return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
    }
}