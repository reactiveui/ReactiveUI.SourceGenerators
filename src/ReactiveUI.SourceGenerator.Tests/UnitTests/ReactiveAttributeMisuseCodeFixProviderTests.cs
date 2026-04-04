// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="ReactiveAttributeMisuseCodeFixProvider" />.
/// </summary>
public sealed class ReactiveAttributeMisuseCodeFixProviderTests
{
    /// <summary>
    /// Validates the code fix provider advertises the expected diagnostic ID.
    /// </summary>
    [Test]
    public void FixableDiagnosticIdsIncludesReactivePartialRule()
    {
        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        if (!provider.FixableDiagnosticIds.Contains("RXUISG0020"))
        {
            throw new InvalidOperationException("Expected RXUISG0020 to be fixable.");
        }
    }

    /// <summary>
    /// Validates the code fix provider exposes a fix-all implementation.
    /// </summary>
    [Test]
    public void GetFixAllProviderReturnsBatchFixer()
    {
        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        if (provider.GetFixAllProvider() is null)
        {
            throw new InvalidOperationException("Expected a fix-all provider.");
        }
    }

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

        AssertContains(fixedSource, "public required partial string? PartialRequiredPropertyTest");
        AssertDoesNotContain(fixedSource, "public partial required string? PartialRequiredPropertyTest");
    }

    /// <summary>
    /// Verifies no code fix is registered when the diagnostic location is outside a property declaration.
    /// </summary>
    [Test]
    public void WhenDiagnosticDoesNotTargetAPropertyThenNoCodeFixIsRegistered()
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
        var root = document.GetSyntaxRootAsync(CancellationToken.None).GetAwaiter().GetResult()!;
        var classDeclaration = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().Single();
        var diagnosticDescriptor = new ReactiveAttributeMisuseAnalyzer().SupportedDiagnostics.Single(d => d.Id == "RXUISG0020");
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, classDeclaration.Identifier.GetLocation());
        var actions = new List<Microsoft.CodeAnalysis.CodeActions.CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, _) => actions.Add(a), CancellationToken.None);

        var provider = new ReactiveAttributeMisuseCodeFixProvider();
        provider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();

        if (actions.Count != 0)
        {
            throw new InvalidOperationException("Expected no code fixes to be registered.");
        }
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
