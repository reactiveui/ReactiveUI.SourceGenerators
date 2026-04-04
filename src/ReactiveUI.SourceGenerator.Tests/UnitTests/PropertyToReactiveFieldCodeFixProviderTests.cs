// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="PropertyToReactiveFieldCodeFixProvider" />.
/// </summary>
public sealed class PropertyToReactiveFieldCodeFixProviderTests
{
    /// <summary>
    /// Validates the code fix provider advertises the expected diagnostic ID.
    /// </summary>
    [Test]
    public void FixableDiagnosticIdsIncludesReactiveFieldRule()
    {
        var provider = new PropertyToReactiveFieldCodeFixProvider();
        if (!provider.FixableDiagnosticIds.Contains("RXUISG0016"))
        {
            throw new InvalidOperationException("Expected RXUISG0016 to be fixable.");
        }
    }

    /// <summary>
    /// Validates the code fix provider exposes a fix-all implementation.
    /// </summary>
    [Test]
    public void GetFixAllProviderReturnsBatchFixer()
    {
        var provider = new PropertyToReactiveFieldCodeFixProvider();
        if (provider.GetFixAllProvider() is null)
        {
            throw new InvalidOperationException("Expected a fix-all provider.");
        }
    }

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

        AssertContains(fixedSource, "[ReactiveUI.SourceGenerators.Reactive]");
        AssertContains(fixedSource, "private bool _isVisible");
        AssertDoesNotContain(fixedSource, "public bool IsVisible");
    }

    private static string ApplyFix(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var analyzer = new PropertyToReactiveFieldAnalyzer();
        var compilation = CSharpCompilation.Create(
            "CodeFixTests",
            syntaxTrees: [tree],
            references: TestCompilationReferences.CreateDefault(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostic = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter().GetResult()
            .Single(d => d.Id == "RXUISG0016");

        using var workspace = new AdhocWorkspace();
        var project = workspace.CurrentSolution
            .AddProject("p", "p", LanguageNames.CSharp)
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13))
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var reference in TestCompilationReferences.CreateDefault())
        {
            project = project.AddMetadataReference(reference);
        }

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

    private static void AssertContains(string actual, string expected)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected output to contain '{expected}'.");
        }
    }

    private static void AssertDoesNotContain(string actual, string unexpected)
    {
        if (actual.Contains(unexpected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected output not to contain '{unexpected}'.");
        }
    }
}
