// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="ReactiveAttributeMisuseCodeFixProvider" />.</summary>
public sealed class ReactiveAttributeMisuseCodeFixProviderTests
{
    /// <summary>Identifies the diagnostic validated by this test class.</summary>
    private const string ReactivePartialDiagnosticId = "RXUISG0020";

    /// <summary>Validates the code fix provider advertises the expected diagnostic ID.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task FixableDiagnosticIdsIncludesReactivePartialRule()
    {
        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        await Assert.That(provider.FixableDiagnosticIds.Contains(ReactivePartialDiagnosticId)).IsTrue();
    }

    /// <summary>Validates the code fix provider exposes a fix-all implementation.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task GetFixAllProviderReturnsBatchFixer()
    {
        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        await Assert.That(provider.GetFixAllProvider()).IsNotNull();
    }

    /// <summary>Verifies `required` stays before `partial` when applying the code fix.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenRequiredPropertyThenPartialInsertedAfterRequired()
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

        var fixedSource = await ApplyFix(source);

        await Assert.That(fixedSource.Contains("public required partial string? PartialRequiredPropertyTest", StringComparison.Ordinal)).IsTrue();
        await Assert.That(fixedSource.Contains("public partial required string? PartialRequiredPropertyTest", StringComparison.Ordinal)).IsFalse();
    }

    /// <summary>Verifies no code fix is registered when the diagnostic location is outside a property declaration.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task WhenDiagnosticDoesNotTargetAPropertyThenNoCodeFixIsRegistered()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public class TestVM : ReactiveObject
            {
            }
            """;

        using var workspace = new AdhocWorkspace();
        var project = workspace.CurrentSolution
            .AddProject("p", "p", LanguageNames.CSharp)
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13))
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

        var document = project.AddDocument("t.cs", source);
        var root = (await document.GetSyntaxRootAsync(CancellationToken.None))!;
        Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax? classDeclaration = null;
        foreach (var node in root.DescendantNodes())
        {
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax declaration)
            {
                classDeclaration = declaration;
                break;
            }
        }

        await Assert.That(classDeclaration).IsNotNull();

        DiagnosticDescriptor? diagnosticDescriptor = null;
        foreach (var descriptor in new ReactiveAttributeMisuseAnalyzer().SupportedDiagnostics)
        {
            if (descriptor.Id == ReactivePartialDiagnosticId)
            {
                diagnosticDescriptor = descriptor;
                break;
            }
        }

        await Assert.That(diagnosticDescriptor).IsNotNull();
        var diagnostic = Diagnostic.Create(diagnosticDescriptor!, classDeclaration!.Identifier.GetLocation());
        var actions = new List<Microsoft.CodeAnalysis.CodeActions.CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, _) => actions.Add(a), CancellationToken.None);

        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        await provider.RegisterCodeFixesAsync(context);
        await Assert.That(actions.Count).IsEqualTo(0);
    }

    /// <summary>Applies the code fix to the supplied source.</summary>
    /// <param name="source">The source to fix.</param>
    /// <returns>A task that resolves to the fixed source.</returns>
    private static async Task<string> ApplyFix(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));
        var analyzer = new ReactiveAttributeMisuseAnalyzer();
        var compilation = CSharpCompilation.Create(
            "CodeFixTests",
            syntaxTrees: [tree],
            references: [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ],
            options: new(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
        var diagnostic = diagnostics.Single(static d => d.Id == ReactivePartialDiagnosticId);

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

        await provider.RegisterCodeFixesAsync(context);

        var operation = (await actions[0].GetOperationsAsync(CancellationToken.None))[0];
        operation.Apply(document.Project.Solution.Workspace, CancellationToken.None);

        var updatedDoc = document.Project.Solution.Workspace.CurrentSolution.GetDocument(document.Id);
        return (await updatedDoc!.GetTextAsync(CancellationToken.None)).ToString();
    }
}
