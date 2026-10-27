// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias analyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Runs the <c>[ObservableAsProperty]</c> analyzer and its code fix over a document, as an IDE would.</summary>
internal static class CodeFixRunner
{
    /// <summary>The name the document is given in the workspace.</summary>
    private const string DocumentName = "Test.cs";

    /// <summary>The name of the project and its assembly.</summary>
    private const string ProjectName = "TestAssembly";

    /// <summary>The code action's title, which is also its fix-all equivalence key.</summary>
    private const string FixTitle = "Convert [ObservableAsProperty] members to partial properties";

    /// <summary>The runtime package a test project references.</summary>
    internal enum Runtime
    {
        /// <summary>The lean <c>ReactiveUI.Binding</c> package.</summary>
        Lean = 0,

        /// <summary>The <c>ReactiveUI.Binding.Reactive</c> package.</summary>
        Reactive = 1,

        /// <summary>Neither package.</summary>
        None = 2,
    }

    /// <summary>Returns the diagnostics the analyzer reports for a source.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <param name="runtime">The runtime package the project references.</param>
    /// <returns>The diagnostics, in source order.</returns>
    internal static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, LanguageVersion languageVersion, Runtime runtime = Runtime.Lean)
    {
        var document = CreateDocument(source, languageVersion, runtime);
        return await DiagnosticsAsync(document).ConfigureAwait(false);
    }

    /// <summary>Returns the diagnostics another analyzer reports for a source.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <param name="analyzer">The analyzer to run.</param>
    /// <param name="runtime">The runtime package the project references.</param>
    /// <returns>The diagnostics, in source order.</returns>
    internal static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, LanguageVersion languageVersion, DiagnosticAnalyzer analyzer, Runtime runtime = Runtime.Lean)
    {
        var document = CreateDocument(source, languageVersion, runtime);
        return await DiagnosticsAsync(document, analyzer).ConfigureAwait(false);
    }

    /// <summary>Applies the code fix offered for the first fixable diagnostic.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <returns>The fixed source, or null when no fix was offered.</returns>
    internal static async Task<string?> FixFirstAsync(string source, LanguageVersion languageVersion)
    {
        var document = CreateDocument(source, languageVersion);
        var diagnostics = await DiagnosticsAsync(document).ConfigureAwait(false);
        var provider = new analyzer::ReactiveUI.Binding.Analyzer.CodeFixes.ObservableAsPropertyCodeFixProvider();
        var actions = new List<CodeAction>();
        for (var i = 0; i < diagnostics.Length && actions.Count == 0; i++)
        {
            if (!provider.FixableDiagnosticIds.Contains(diagnostics[i].Id))
            {
                continue;
            }

            var context = new CodeFixContext(document, diagnostics[i], (action, _) => actions.Add(action), CancellationToken.None);
            await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);
        }

        return actions.Count == 0 ? null : await ApplyAsync(document, actions[0]).ConfigureAwait(false);
    }

    /// <summary>Applies the code fix to every diagnostic in the document at once.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <returns>The fixed source, or null when nothing was fixed.</returns>
    internal static async Task<string?> FixAllAsync(string source, LanguageVersion languageVersion)
    {
        var document = CreateDocument(source, languageVersion);
        var diagnostics = await DiagnosticsAsync(document).ConfigureAwait(false);
        var provider = new analyzer::ReactiveUI.Binding.Analyzer.CodeFixes.ObservableAsPropertyCodeFixProvider();
        var context = new FixAllContext(
            document,
            provider,
            FixAllScope.Document,
            FixTitle,
            provider.FixableDiagnosticIds,
            new FixedDiagnostics(diagnostics),
            CancellationToken.None);
        var action = await provider.GetFixAllProvider().GetFixAsync(context).ConfigureAwait(false);
        return action is null ? null : await ApplyAsync(document, action).ConfigureAwait(false);
    }

    /// <summary>Creates a document in a project referencing a runtime package.</summary>
    /// <param name="source">The document text.</param>
    /// <param name="languageVersion">The language version.</param>
    /// <param name="runtime">The runtime package the project references.</param>
    /// <returns>The document.</returns>
    private static Document CreateDocument(string source, LanguageVersion languageVersion, Runtime runtime = Runtime.Lean)
    {
        var parseOptions = TestHelper.ParseOptionsFor(languageVersion);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, runtime == Runtime.Reactive, ProjectName, []);
        if (runtime == Runtime.None)
        {
            compilation = compilation.RemoveReferences(compilation.References.Where(static r => r.Display?.Contains("ReactiveUI.Binding", StringComparison.Ordinal) == true));
        }

        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var solution = workspace.CurrentSolution
            .AddProject(ProjectInfo.Create(
                projectId,
                VersionStamp.Default,
                ProjectName,
                ProjectName,
                LanguageNames.CSharp,
                compilationOptions: compilation.Options,
                parseOptions: parseOptions,
                metadataReferences: compilation.References));
        var documentId = DocumentId.CreateNewId(projectId);
        return solution.AddDocument(documentId, DocumentName, source).GetDocument(documentId)!;
    }

    /// <summary>Runs an analyzer over a document's project.</summary>
    /// <param name="document">The document.</param>
    /// <param name="analyzer">The analyzer, or null for the <c>[ObservableAsProperty]</c> one.</param>
    /// <returns>The analyzer's diagnostics, in source order.</returns>
    private static async Task<ImmutableArray<Diagnostic>> DiagnosticsAsync(Document document, DiagnosticAnalyzer? analyzer = null)
    {
        var compilation = (await document.Project.GetCompilationAsync().ConfigureAwait(false))!;
        analyzer ??= new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.ObservableAsPropertyAnalyzer();
        var diagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
        return [.. diagnostics.OrderBy(static d => d.Location.SourceSpan.Start)];
    }

    /// <summary>Applies a code action and returns the changed document's text.</summary>
    /// <param name="document">The document the action changes.</param>
    /// <param name="action">The code action.</param>
    /// <returns>The document's new text.</returns>
    private static async Task<string> ApplyAsync(Document document, CodeAction action)
    {
        var operations = await action.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var text = await solution.GetDocument(document.Id)!.GetTextAsync().ConfigureAwait(false);
        return text.ToString();
    }

    /// <summary>Hands fix-all the diagnostics already computed.</summary>
    /// <param name="diagnostics">The diagnostics.</param>
    private sealed class FixedDiagnostics(ImmutableArray<Diagnostic> diagnostics) : FixAllContext.DiagnosticProvider
    {
        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Diagnostic>>([]);
    }
}
