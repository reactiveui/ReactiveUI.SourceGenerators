// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    /// <summary>Validates the syntax fallback recognizes an unresolved identifier-form ReactiveObject base.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenUnresolvedIdentifierReactiveBaseTypeThenReportsDiagnostic()
    {
        const string source = """
            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var diagnostics = await GetDiagnostics(source);

        await AssertContainsDiagnostic(diagnostics, ReactiveFieldDiagnosticId);
    }

    /// <summary>Validates every property-shape rejection while retaining syntax fallback candidates.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task CandidateFiltersRejectUnsupportedPropertyShapes()
    {
        const string source = """
            using System;

            namespace TestNs;

            public sealed class ObservableAsPropertyAttribute : Attribute { } public sealed class ReactiveAttribute : Attribute { }
            public sealed class ReactiveCommand<TInput, TOutput> { } public sealed class ReactiveProperty<T> { }
            public sealed class ViewModelActivator { }

            public partial class CandidateViewModel : ReactiveUI.ReactiveObject
            {
                public int Eligible { get; set; } public int ReadOnly { get; }
                public int Expression => 42; internal int InternalProperty { get; set; }
                public static int StaticProperty { get; set; }
                public int BodyProperty { get { return 1; } set { } }
                public int ExpressionAccessor { get => 1; set => _ = value; }
                public int PrivateSetter { get; private set; }
                public int InternalSetter { get; internal set; }
                public ReactiveCommand<int, int>? Command { get; set; }
                public ReactiveProperty<int>? ReactiveValue { get; set; }
                public ViewModelActivator? Activator { get; set; }

                [ObservableAsProperty]
                public int ObservableProperty { get; set; }

                [Reactive]
                public int ReactiveProperty { get; set; }
            }

            public class PlainViewModel
            {
                public int Plain { get; set; }
            }

            public class QualifiedNonReactiveViewModel : global::System.IDisposable
            {
                public int QualifiedNonReactive { get; set; }
                public void Dispose() { }
            }

            public class GenericNonReactiveViewModel : IComparable<GenericNonReactiveViewModel>
            {
                public int GenericNonReactive { get; set; }
                public int CompareTo(GenericNonReactiveViewModel? other) => 0;
            }

            public class SyntaxFallbackViewModel : Missing.ReactiveObject
            {
                public int SyntaxFallback { get; set; }
            }

            """;

        var diagnostics = await GetDiagnostics(source);
        var propertyNames = await GetDiagnosticPropertyNames(source, diagnostics);

        await Assert.That(propertyNames).IsEquivalentTo(["Eligible", "SyntaxFallback"]);
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

    /// <summary>Gets property names associated with diagnostics.</summary>
    /// <param name="source">The analyzed source.</param>
    /// <param name="diagnostics">The diagnostics to inspect.</param>
    /// <returns>A task that resolves to the diagnostic property names.</returns>
    private static async Task<string[]> GetDiagnosticPropertyNames(string source, IEnumerable<Diagnostic> diagnostics)
    {
        var root = await CSharpSyntaxTree.ParseText(source).GetRootAsync();
        var names = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == ReactiveFieldDiagnosticId
                && root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax property)
            {
                names.Add(property.Identifier.ValueText);
            }
        }

        return names.ToArray();
    }
}
