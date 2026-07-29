// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="PropertyToReactiveFieldCodeFixProvider" />.</summary>
public sealed class PropertyToReactiveFieldCodeFixProviderTests
{
    /// <summary>Validates the code fix provider advertises the expected diagnostic ID.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task FixableDiagnosticIdsIncludesReactiveFieldRule()
    {
        var provider = new PropertyToReactiveFieldCodeFixProvider();
        await Assert.That(provider.FixableDiagnosticIds.Contains("RXUISG0016")).IsTrue();
    }

    /// <summary>Validates the code fix provider exposes a fix-all implementation.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProviderReturnsBatchFixer()
    {
        var provider = new PropertyToReactiveFieldCodeFixProvider();
        await Assert.That(provider.GetFixAllProvider()).IsNotNull();
    }

    /// <summary>Validates a public auto-property is converted to a private field annotated with <c>[Reactive]</c>.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenApplyingFixThenConvertsPropertyToReactiveField()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public bool IsVisible { get; set; }
            }
            """;

        var fixedSource = await ApplyFix(source);

        await Assert.That(fixedSource.Contains("[ReactiveUI.SourceGenerators.Reactive]", StringComparison.Ordinal)).IsTrue();
        await Assert.That(fixedSource.Contains("private bool _isVisible", StringComparison.Ordinal)).IsTrue();
        await Assert.That(fixedSource.Contains("public bool IsVisible", StringComparison.Ordinal)).IsFalse();
    }

    /// <summary>Applies the code fix to the supplied source.</summary>
    /// <param name="source">The source to fix.</param>
    /// <returns>A task that resolves to the fixed source.</returns>
    private static async Task<string> ApplyFix(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));

        var analyzer = new PropertyToReactiveFieldAnalyzer();
        var compilation = CSharpCompilation.Create(
            "CodeFixTests",
            syntaxTrees: [tree],
            references: TestCompilationReferences.CreateDefault(),
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
        var diagnostic = diagnostics.Single(static d => d.Id == "RXUISG0016");

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

        await provider.RegisterCodeFixesAsync(context);

        var operation = (await actions[0].GetOperationsAsync(CancellationToken.None))[0];
        operation.Apply(document.Project.Solution.Workspace, CancellationToken.None);

        var updatedDoc = document.Project.Solution.Workspace.CurrentSolution.GetDocument(document.Id);
        return (await updatedDoc!.GetTextAsync(CancellationToken.None)).ToString();
    }
}
