// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Analyzer.CodeFixes;

/// <summary>
/// Rewrites the <c>[ObservableAsProperty]</c> forms ReactiveUI's older source generator accepted (a field, a method or an
/// observable property) as partial properties the generator implements (RXUIBIND018).
/// </summary>
/// <remarks>
/// <para>
/// Every marked member of a type is rewritten in one pass, because a method or an observable property adds a statement
/// to the same <c>InitializeOAPH</c> method. That method assigns each helper with <c>ToProperty</c>, and the constructor
/// calls it, as it called the one the older generator wrote.
/// </para>
/// <para>
/// A field becomes a property named after it, without its <c>_</c> or <c>m_</c> prefix. Its initializer becomes the
/// attribute's <c>InitialValue</c>, and its <c>Inheritance</c> a modifier. A method or an observable property keeps its
/// declaration, and gains a property named by <c>PropertyName</c>, or by its own name followed by <c>Property</c>.
/// Partial properties need C# 13, so the fix is offered from C# 13.
/// </para>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class ObservableAsPropertyCodeFixProvider : CodeFixProvider
{
    /// <summary>The title of the code action, which also identifies it for fix-all.</summary>
    internal const string Title = "Convert [ObservableAsProperty] members to partial properties";

    /// <summary>The <c>LanguageVersion</c> value of C# 13, which Roslyn 4.8 does not name.</summary>
    private const int CSharp13 = 1300;

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        new[] { DiagnosticWarnings.ObservableAsPropertyNeedsPartialProperty.Id }.ToImmutableArray();

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(FixAllInDocumentAsync);

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];
        if (!IsFixable(diagnostic, context.Document))
        {
            return;
        }

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var type = root!.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<TypeDeclarationSyntax>()!;
        context.RegisterCodeFix(
            CodeAction.Create(Title, ct => RewriteTypesAsync(context.Document, [type], ct), Title),
            diagnostic);
    }

    /// <summary>Checks whether a diagnostic names a form the fix rewrites, in a project that has partial properties.</summary>
    /// <param name="diagnostic">The diagnostic.</param>
    /// <param name="document">The document it is in.</param>
    /// <returns><see langword="true"/> when the fix applies.</returns>
    internal static bool IsFixable(Diagnostic diagnostic, Document document) =>
        diagnostic.Properties.ContainsKey(ObservableAsPropertyAnalyzer.FormProperty)
        && document.Project.ParseOptions is CSharpParseOptions { LanguageVersion: var version }
        && (int)version >= CSharp13;

    /// <summary>Rewrites every type in a document that holds a diagnosed member.</summary>
    /// <param name="context">The fix-all context.</param>
    /// <param name="document">The document.</param>
    /// <param name="diagnostics">The diagnostics in the document.</param>
    /// <returns>The rewritten document, or null when nothing applies.</returns>
    private static async Task<Document?> FixAllInDocumentAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var types = new List<TypeDeclarationSyntax>();
        for (var i = 0; i < diagnostics.Length; i++)
        {
            if (IsFixable(diagnostics[i], document)
                && root!.FindNode(diagnostics[i].Location.SourceSpan).FirstAncestorOrSelf<TypeDeclarationSyntax>() is { } type
                && !types.Contains(type))
            {
                types.Add(type);
            }
        }

        return types.Count == 0 ? null : await RewriteTypesAsync(document, types, context.CancellationToken).ConfigureAwait(false);
    }

    /// <summary>Rewrites the marked members of each type, and makes each type and every type around it partial.</summary>
    /// <param name="document">The document.</param>
    /// <param name="types">The type declarations to rewrite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rewritten document.</returns>
    private static async Task<Document> RewriteTypesAsync(Document document, List<TypeDeclarationSyntax> types, CancellationToken ct)
    {
        var root = (await document.GetSyntaxRootAsync(ct).ConfigureAwait(false))!;
        var model = (await document.GetSemanticModelAsync(ct).ConfigureAwait(false))!;
        var newLine = LineEnding(root);
        var rewritten = new Dictionary<SyntaxNode, SyntaxNode>(types.Count);
        for (var i = 0; i < types.Count; i++)
        {
            rewritten[types[i]] = ObservableAsPropertyRewriter.RewriteType(types[i], model, newLine, ct);
        }

        root = root.ReplaceNodes(rewritten.Keys, (original, _) => rewritten[original]);
        return document.WithSyntaxRoot(MakeContainersPartial(root));
    }

    /// <summary>Reads the line ending a document uses.</summary>
    /// <param name="root">The document root.</param>
    /// <returns>The first line ending in the document, or <c>\n</c> when it has none.</returns>
    private static string LineEnding(SyntaxNode root)
    {
        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return trivia.ToString();
            }
        }

        return "\n";
    }

    /// <summary>Adds <c>partial</c> to every type that holds a written property, or holds a type that does.</summary>
    /// <param name="root">The document root.</param>
    /// <returns>The root with the modifiers added.</returns>
    private static SyntaxNode MakeContainersPartial(SyntaxNode root)
    {
        var needed = new HashSet<TypeDeclarationSyntax>();
        foreach (var annotated in root.GetAnnotatedNodes(ObservableAsPropertyRewriter.ConvertedKind))
        {
            foreach (var ancestor in annotated.Ancestors())
            {
                if (ancestor is TypeDeclarationSyntax type && !type.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    _ = needed.Add(type);
                }
            }
        }

        return needed.Count == 0 ? root : root.ReplaceNodes(needed, static (_, type) => AddPartial(type));
    }

    /// <summary>Adds the <c>partial</c> modifier just before a type's keyword.</summary>
    /// <param name="type">The type declaration.</param>
    /// <returns>The declaration with the modifier.</returns>
    private static TypeDeclarationSyntax AddPartial(TypeDeclarationSyntax type)
    {
        if (type.Modifiers.Count != 0)
        {
            return type.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
        }

        var leading = type.Keyword.LeadingTrivia;
        return type
            .WithKeyword(type.Keyword.WithLeadingTrivia())
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(leading, SyntaxKind.PartialKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
    }
}
