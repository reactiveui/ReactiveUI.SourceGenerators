// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ReactiveAttributeMisuseAnalyzer" />.
/// </summary>
public sealed class ReactiveAttributeMisuseAnalyzerTests
{
    /// <summary>
    /// Validates the analyzer rejects a null analysis context.
    /// </summary>
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

    /// <summary>
    /// Verifies a non-partial property annotated with <c>[Reactive]</c> produces a warning.
    /// </summary>
    [Test]
    public void WhenReactiveOnNonPartialPropertyThenWarn()
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

        AssertContainsDiagnostic(diagnostics, "RXUISG0020");
    }

    /// <summary>
    /// Verifies a non-partial containing type annotated with a <c>[Reactive]</c> property produces a warning.
    /// </summary>
    [Test]
    public void WhenReactiveOnNonPartialContainingTypeThenWarn()
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

        var diagnostics = GetDiagnostics(source);

        AssertContainsDiagnostic(diagnostics, "RXUISG0020");
    }

    /// <summary>
    /// Verifies no warning is produced when both property and containing type are partial.
    /// </summary>
    [Test]
    public void WhenReactiveOnPartialPropertyAndTypeThenNoWarn()
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

        var diagnostics = GetDiagnostics(source);

        AssertDoesNotContainDiagnostic(diagnostics, "RXUISG0020");
    }

    /// <summary>
    /// Verifies the analyzer recognizes the explicit ReactiveAttribute name.
    /// </summary>
    [Test]
    public void WhenReactiveAttributeSuffixUsedThenWarns()
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

        var diagnostics = GetDiagnostics(source);

        AssertContainsDiagnostic(diagnostics, "RXUISG0020");
    }

    /// <summary>
    /// Verifies unrelated attributes do not trigger the diagnostic.
    /// </summary>
    [Test]
    public void WhenOnlyNonReactiveAttributesExistThenDoesNotWarn()
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

        var diagnostics = GetDiagnostics(source);

        AssertDoesNotContainDiagnostic(diagnostics, "RXUISG0020");
    }

    private static Diagnostic[] GetDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ReactiveAttributeMisuseAnalyzer();

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
