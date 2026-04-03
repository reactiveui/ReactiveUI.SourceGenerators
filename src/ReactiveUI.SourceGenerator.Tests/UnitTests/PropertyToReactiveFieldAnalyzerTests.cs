// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="PropertyToReactiveFieldAnalyzer" />.
/// </summary>
public sealed class PropertyToReactiveFieldAnalyzerTests
{
    /// <summary>
    /// Validates the analyzer rejects a null analysis context.
    /// </summary>
    [Test]
    public void InitializeWithNullContextThrows()
    {
        var analyzer = new PropertyToReactiveFieldAnalyzer();

        try
        {
            analyzer.Initialize(null!);
            throw new InvalidOperationException("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "context")
        {
        }
    }

    /// <summary>
    /// Validates a public auto-property triggers the suggestion to convert it into a reactive field.
    /// </summary>
    [Test]
    public void WhenPublicAutoPropertyThenReportsDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        AssertContainsDiagnostic(diagnostics, "RXUISG0016");
    }

    /// <summary>
    /// Validates a property already annotated with <c>[Reactive]</c> is ignored.
    /// </summary>
    [Test]
    public void WhenReactiveAttributePresentThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        AssertDoesNotContainDiagnostic(diagnostics, "RXUISG0016");
    }

    /// <summary>
    /// Validates the syntax-based Reactive attribute fallback handles qualified names.
    /// </summary>
    [Test]
    public void WhenQualifiedReactiveAttributePresentThenDoesNotReportDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveUI.SourceGenerators.Reactive]
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        AssertDoesNotContainDiagnostic(diagnostics, "RXUISG0016");
    }

    /// <summary>
    /// Validates the analyzer recognizes a fully qualified ReactiveObject base type.
    /// </summary>
    [Test]
    public void WhenQualifiedReactiveBaseTypeThenReportsDiagnostic()
    {
        const string source = """
            namespace TestNs;

            public partial class TestVM : ReactiveUI.ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = GetDiagnostics(source);

        AssertContainsDiagnostic(diagnostics, "RXUISG0016");
    }

    private static Diagnostic[] GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new PropertyToReactiveFieldAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
    }

    private static void AssertContainsDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        if (!diagnostics.Any(d => d.Id == diagnosticId))
        {
            throw new InvalidOperationException($"Expected diagnostic '{diagnosticId}' was not reported.");
        }
    }

    private static void AssertDoesNotContainDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        if (diagnostics.Any(d => d.Id == diagnosticId))
        {
            throw new InvalidOperationException($"Diagnostic '{diagnosticId}' was reported unexpectedly.");
        }
    }
}
