// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="PropertyToReactiveFieldAnalyzer" />.</summary>
public sealed class PropertyToReactiveFieldAnalyzerTests
{
    /// <summary>Identifies the diagnostic validated by this test class.</summary>
    private const string ReactiveFieldDiagnosticId = "RXUISG0016";

    /// <summary>Validates the analyzer rejects a null analysis context.</summary>
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

    /// <summary>Validates a public auto-property triggers the suggestion to convert it into a reactive field.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenPublicAutoPropertyThenReportsDiagnostic()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates a property already annotated with <c>[Reactive]</c> is ignored.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveAttributePresentThenDoesNotReportDiagnostic()
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

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates the syntax-based Reactive attribute fallback handles qualified names.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenQualifiedReactiveAttributePresentThenDoesNotReportDiagnostic()
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

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates the analyzer recognizes a fully qualified ReactiveObject base type.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenQualifiedReactiveBaseTypeThenReportsDiagnostic()
    {
        const string source = """
            namespace TestNs;

            public partial class TestVM : ReactiveUI.ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Gets diagnostics produced by the analyzer for the supplied source.</summary>
    /// <param name="source">The source to analyze.</param>
    /// <returns>A task that resolves to the diagnostics.</returns>
    private static async Task<Diagnostic[]> GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: TestCompilationReferences.CreateDefault(),
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new PropertyToReactiveFieldAnalyzer();

        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer]);
        return (await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()).ToArray();
    }

    /// <summary>Asserts that the diagnostics contain the specified ID.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The expected diagnostic ID.</param>
    /// <returns>A task that represents the assertion.</returns>
    private static async Task AssertContainsDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        var found = false;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    /// <summary>Asserts that the diagnostics do not contain the specified ID.</summary>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <param name="diagnosticId">The unexpected diagnostic ID.</param>
    /// <returns>A task that represents the assertion.</returns>
    private static async Task AssertDoesNotContainDiagnostic(IEnumerable<Diagnostic> diagnostics, string diagnosticId)
    {
        var found = false;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == diagnosticId)
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsFalse();
    }
}
