// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using ReactiveUI.SourceGenerators.CodeFixers;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="PropertyToReactiveFieldCodeFixProvider" />.
/// </summary>
[TestFixture]
public sealed class PropertyToReactiveFieldCodeFixProviderTests
{
    /// <summary>
    /// Validates a public auto-property is converted to a private field annotated with <c>[Reactive]</c>.
    /// </summary>
    [Test]
    public void WhenApplyingFixThenConvertsPropertyToReactiveField()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var fixedSource = ApplyFix(source);

        Assert.That(fixedSource, Does.Contain("[ReactiveUI.SourceGenerators.Reactive]"));
        Assert.That(fixedSource, Does.Contain("private bool _isVisible"));
        Assert.That(fixedSource, Does.Not.Contain("public bool IsVisible"));
    }

    private static string ApplyFix(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var analyzer = new PropertyToReactiveFieldAnalyzer();
        var compilation = CSharpCompilation.Create(
            "CodeFixTests",
            syntaxTrees: [tree],
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostic = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter().GetResult()
            .Single(d => d.Id == "RXUISG0016");

        using var workspace = new AdhocWorkspace();
        var project = workspace.CurrentSolution
            .AddProject("p", "p", LanguageNames.CSharp)
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13))
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

        var document = project.AddDocument("t.cs", source);

        CodeFixProvider provider = new PropertyToReactiveFieldCodeFixProvider();

        var actions = new List<Microsoft.CodeAnalysis.CodeActions.CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostic,
            (a, _) => actions.Add(a),
            CancellationToken.None);

        provider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();

        var operation = actions.Single().GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult().Single();
        operation.Apply(document.Project.Solution.Workspace, CancellationToken.None);

        var updatedDoc = document.Project.Solution.Workspace.CurrentSolution.GetDocument(document.Id);
        return updatedDoc!.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult().ToString();
    }
}
