// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>
/// ReactiveAttributeMisuseCodeFixProvider.
/// </summary>
/// <seealso cref="Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider" />
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReactiveAttributeMisuseCodeFixProvider))]
[Shared]
public sealed class ReactiveAttributeMisuseCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets a list of diagnostic IDs that this provider can provide fixes for.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(ReactiveAttributeRequiresPartialRule.Id);

    /// <summary>
    /// Gets an optional <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
    /// Return null if the provider doesn't support fix all/multiple occurrences.
    /// Otherwise, you can return any of the well known fix all providers from <see cref="T:Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders" /> or implement your own fix all provider.
    /// </summary>
    /// <returns>A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> instance if the provider supports fix all/multiple occurrences; otherwise, null.</returns>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
    /// </summary>
    /// <param name="context">A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
    /// The context must only contain diagnostics with a <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make property and containing type partial",
                createChangedDocument: c => MakePartialAsync(context.Document, propertyDeclaration, c),
                equivalenceKey: "Make property and containing type partial"),
            diagnostic);
    }

    private static async Task<Document> MakePartialAsync(Document document, PropertyDeclarationSyntax property, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var newProperty = AddPartialModifier(property);
        editor.ReplaceNode(property, newProperty);

        var containingType = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is not null)
        {
            var newContainingType = AddPartialModifier(containingType);
            editor.ReplaceNode(containingType, newContainingType);
        }

        return editor.GetChangedDocument();
    }

    private static T AddPartialModifier<T>(T declaration)
        where T : MemberDeclarationSyntax
    {
        var modifiers = declaration switch
        {
            TypeDeclarationSyntax t => t.Modifiers,
            PropertyDeclarationSyntax p => p.Modifiers,
            _ => throw new InvalidOperationException("Unsupported declaration type")
        };

        if (modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return declaration;
        }

        var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword);

        // Insert `partial` after the last accessibility modifier (if present).
        var insertIndex = 0;
        for (var i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].IsKind(SyntaxKind.PublicKeyword)
                || modifiers[i].IsKind(SyntaxKind.InternalKeyword)
                || modifiers[i].IsKind(SyntaxKind.PrivateKeyword)
                || modifiers[i].IsKind(SyntaxKind.ProtectedKeyword))
            {
                insertIndex = i + 1;
            }

            // `required` must precede `partial` for required members.
            if (modifiers[i].IsKind(SyntaxKind.RequiredKeyword))
            {
                insertIndex = i + 1;
            }
        }

        var newModifiers = modifiers.Insert(insertIndex, partialToken);

        return declaration switch
        {
            TypeDeclarationSyntax t => (T)(MemberDeclarationSyntax)t.WithModifiers(newModifiers),
            PropertyDeclarationSyntax p => (T)(MemberDeclarationSyntax)p.WithModifiers(newModifiers),
            _ => declaration
        };
    }
}
