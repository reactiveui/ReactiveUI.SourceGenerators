// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using ReactiveUI.SourceGenerators.CodeFixers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ReactiveAttributeMisuseAnalyzer" />.
/// </summary>
[TestFixture]
public sealed class ReactiveAttributeMisuseAnalyzerTests
{
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

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
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

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.True);
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

        Assert.That(diagnostics.Any(d => d.Id == "RXUISG0020"), Is.False);
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
}
