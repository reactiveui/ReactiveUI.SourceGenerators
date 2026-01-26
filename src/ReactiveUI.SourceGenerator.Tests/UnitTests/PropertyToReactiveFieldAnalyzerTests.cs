// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="PropertyToReactiveFieldAnalyzer" />.
/// </summary>
[TestFixture]
public sealed class PropertyToReactiveFieldAnalyzerTests
{
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

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.True);
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

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0016"), Is.False);
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
}
