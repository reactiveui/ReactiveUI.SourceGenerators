// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="ReactiveAttributeMisuseAnalyzer" />.</summary>
public sealed class ReactiveAttributeMisuseAnalyzerTests
{
    /// <summary>Identifies the diagnostic validated by this test class.</summary>
    private const string ReactivePartialDiagnosticId = "RXUISG0020";

    /// <summary>Validates the analyzer rejects a null analysis context.</summary>
    [Test]
    public void InitializeWithNullContextThrows()
    {
        var analyzer = new ReactiveAttributeMisuseAnalyzer();

        try
        {
            analyzer.Initialize(null!);
            throw new InvalidOperationException("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "context")
        {
        }
    }

    /// <summary>Verifies a non-partial property annotated with <c>[Reactive]</c> produces a warning.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnNonPartialPropertyThenWarn()
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

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies a non-partial containing type annotated with a <c>[Reactive]</c> property produces a warning.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnNonPartialContainingTypeThenWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public class TestVM : ReactiveObject
            {
                [Reactive]
                public partial bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies no warning is produced when both property and containing type are partial.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveOnPartialPropertyAndTypeThenNoWarn()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive]
                public partial bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies the analyzer recognizes the explicit ReactiveAttribute name.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenReactiveAttributeSuffixUsedThenWarns()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [ReactiveAttribute]
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactivePartialDiagnosticId);
    }

    /// <summary>Verifies unrelated attributes do not trigger the diagnostic.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenOnlyNonReactiveAttributesExistThenDoesNotWarn()
    {
        const string source = """
            using System;
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Obsolete]
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertDoesNotContainDiagnostic(diagnostics, ReactivePartialDiagnosticId);
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
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ReactiveAttributeMisuseAnalyzer();

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
