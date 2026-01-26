// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ReactiveAttributeMisuseCodeFixProvider" />.
/// </summary>
[TestFixture]
public sealed class ReactiveAttributeMisuseCodeFixProviderTests
{
    /// <summary>
    /// Verifies `required` stays before `partial` when applying the code fix.
    /// </summary>
    [Test]
    public void WhenRequiredPropertyThenPartialInsertedAfterRequired()
    {
        const string source = """
            using ReactiveUI;
            using ReactiveUI.SourceGenerators;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                [Reactive(UseRequired = true)]
                public required string? PartialRequiredPropertyTest { get; set; }
            }
            """;

        var fixedSource = ApplyFix(source);

        Assert.That(fixedSource, Does.Contain("public required partial string? PartialRequiredPropertyTest"));
        Assert.That(fixedSource, Does.Not.Contain("public partial required string? PartialRequiredPropertyTest"));
    }

    private static string ApplyFix(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));
        var root = tree.GetRoot();

        var analyzer = new ReactiveAttributeMisuseAnalyzer();
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
            .Single(d => d.Id == "RXUISG0020");

        using var workspace = new AdhocWorkspace();
        var project = workspace.CurrentSolution
            .AddProject("p", "p", LanguageNames.CSharp)
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13))
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

        var document = project.AddDocument("t.cs", source);

        CodeFixProvider provider = new ReactiveAttributeMisuseCodeFixProvider();

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
