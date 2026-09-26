// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="PropertyToReactiveFieldCodeFixProvider" />.</summary>
public sealed class PropertyToReactiveFieldCodeFixProviderTests
{
    /// <summary>A documented property in a view model nested in another type, neither of them partial.</summary>
    private const string PropertySource = """
        using ReactiveUI;

        namespace TestNs;

        public static class Outer
        {
            public class TestVM : ReactiveObject
            {
                /// <summary>Gets or sets a value indicating whether it is visible.</summary>
                public bool IsVisible { get; set; }
            }
        }
        """;

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

    /// <summary>Before C# 13 a public auto-property is converted to a private field annotated with <c>[Reactive]</c>.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task BeforeCSharp13ConvertsPropertyToReactiveField()
    {
        var fixedSource = await ApplyFix(PropertySource, LanguageVersion.CSharp12);

        await Assert.That(fixedSource).Contains("[ReactiveUI.SourceGenerators.Reactive]");
        await Assert.That(fixedSource).Contains("private bool _isVisible");
        await Assert.That(fixedSource).DoesNotContain("public bool IsVisible");
    }

    /// <summary>From C# 13 the property becomes a <c>[Reactive]</c> partial property, in a type made partial, and generates.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task FromCSharp13ConvertsPropertyToReactivePartialProperty()
    {
        var fixedSource = await ApplyFix(PropertySource, LanguageVersion.CSharp13);

        await Assert.That(fixedSource).Contains("[ReactiveUI.SourceGenerators.Reactive]");
        await Assert.That(fixedSource).Contains("/// <summary>Gets or sets a value indicating whether it is visible.</summary>");
        await Assert.That(fixedSource).Contains("public partial bool IsVisible { get; set; }");
        await Assert.That(fixedSource).Contains("public partial class TestVM");
        await Assert.That(fixedSource).Contains("public static partial class Outer");
        await Assert.That(fixedSource).DoesNotContain("_isVisible");
        await Assert.That(GetErrorsAfterGeneration(fixedSource, LanguageVersion.CSharp13)).IsEmpty();
    }

    /// <summary>A property with an initializer becomes a partial property only where C# allows it one.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task PropertyWithInitializerBecomesPartialOnlyWhereTheLanguageAllowsIt()
    {
        const string source = """
            using ReactiveUI;

            namespace TestNs;

            public partial class TestVM : ReactiveObject
            {
                public string Name { get; set; } = "none";
            }
            """;

        var csharp13 = await ApplyFix(source, LanguageVersion.CSharp13);
        var preview = await ApplyFix(source, LanguageVersion.Preview);

        await Assert.That(csharp13).Contains("private string _name = \"none\";");
        await Assert.That(preview).Contains("public partial string Name { get; set; } = \"none\";");
    }

    /// <summary>A diagnostic that is not on a property offers no fix.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DiagnosticOutsideAPropertyOffersNoFix()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.CurrentSolution
            .AddProject("p", "p", LanguageNames.CSharp)
            .AddDocument("t.cs", PropertySource);
        var tree = await document.GetSyntaxTreeAsync();
        const string className = "Outer";
        var classLocation = Location.Create(tree!, new(PropertySource.IndexOf(className, StringComparison.Ordinal), className.Length));
        var actions = new List<Microsoft.CodeAnalysis.CodeActions.CodeAction>();
        var context = new CodeFixContext(
            document,
            Diagnostic.Create(new PropertyToReactiveFieldAnalyzer().SupportedDiagnostics[0], classLocation),
            (action, _) => actions.Add(action),
            CancellationToken.None);

        await new PropertyToReactiveFieldCodeFixProvider().RegisterCodeFixesAsync(context);

        await Assert.That(actions).IsEmpty();
    }

    /// <summary>Compiles fixed source with the <c>[Reactive]</c> generator and gets the errors.</summary>
    /// <param name="source">The fixed source.</param>
    /// <param name="languageVersion">The C# version.</param>
    /// <returns>The errors, one per line.</returns>
    private static string GetErrorsAfterGeneration(string source, LanguageVersion languageVersion)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
        var compilation = CSharpCompilation.Create(
            "CodeFixGenerationTests",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            TestCompilationReferences.CreateDefault(),
            new(OutputKind.DynamicallyLinkedLibrary));
        _ = CSharpGeneratorDriver.Create([new ReactiveGenerator().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        var errors = new List<string>();
        foreach (var diagnostic in output.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic.ToString());
            }
        }

        return string.Join(Environment.NewLine, errors);
    }

    /// <summary>Applies the code fix to the supplied source.</summary>
    /// <param name="source">The source to fix.</param>
    /// <param name="languageVersion">The C# version the project uses.</param>
    /// <returns>A task that resolves to the fixed source.</returns>
    private static async Task<string> ApplyFix(string source, LanguageVersion languageVersion)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(languageVersion));

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
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(languageVersion))
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
