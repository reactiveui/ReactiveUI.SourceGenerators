// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Provides fixes for incomplete partial declarations using <c>Reactive</c>.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReactiveAttributeMisuseCodeFixProvider))]
public sealed class ReactiveAttributeMisuseCodeFixProvider : CodeFixProvider
{
    /// <summary>Gets a list of diagnostic IDs that this provider can fix.</summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(ReactiveAttributeRequiresPartialRule.Id);

    /// <summary>Gets the batch fix provider.</summary>
    /// <returns>The batch fix provider.</returns>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>Registers the available fixes.</summary>
    /// <param name="context">The code-fix context.</param>
    /// <returns>A task that completes after the fixes are registered.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var propertyDeclaration = FindPropertyDeclaration(root, diagnostic.Location.SourceSpan.Start);
        if (propertyDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make property and containing type partial",
                createChangedDocument: cancellationToken => MakePartialAsync(context.Document, propertyDeclaration, cancellationToken),
                equivalenceKey: "Make property and containing type partial"),
            diagnostic);
    }

    /// <summary>Makes a property and its containing type partial.</summary>
    /// <param name="document">The source document.</param>
    /// <param name="property">The property to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated document.</returns>
    private static async Task<Document> MakePartialAsync(Document document, PropertyDeclarationSyntax property, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(property, AddPartialModifier(property));

        var containingType = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is not null)
        {
            editor.ReplaceNode(containingType, AddPartialModifier(containingType));
        }

        return editor.GetChangedDocument();
    }

    /// <summary>Adds a partial modifier when a declaration does not already have one.</summary>
    /// <typeparam name="T">The declaration type.</typeparam>
    /// <param name="declaration">The declaration to update.</param>
    /// <returns>The declaration with a partial modifier.</returns>
    private static T AddPartialModifier<T>(T declaration)
        where T : MemberDeclarationSyntax
    {
        var modifiers = declaration switch
        {
            TypeDeclarationSyntax typeDeclaration => typeDeclaration.Modifiers,
            PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Modifiers,
            _ => throw new InvalidOperationException("Unsupported declaration type")
        };

        if (HasModifier(in modifiers, SyntaxKind.PartialKeyword))
        {
            return declaration;
        }

        var insertIndex = 0;
        for (var i = 0; i < modifiers.Count; i++)
        {
            if (IsAccessibilityModifier(modifiers[i]) || modifiers[i].IsKind(SyntaxKind.RequiredKeyword))
            {
                insertIndex = i + 1;
            }
        }

        var newModifiers = modifiers.Insert(insertIndex, SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        return declaration switch
        {
            TypeDeclarationSyntax typeDeclaration => (T)(MemberDeclarationSyntax)typeDeclaration.WithModifiers(newModifiers),
            PropertyDeclarationSyntax propertyDeclaration => (T)(MemberDeclarationSyntax)propertyDeclaration.WithModifiers(newModifiers),
            _ => declaration
        };
    }

    /// <summary>Finds the property declaration containing a source position.</summary>
    /// <param name="root">The syntax-tree root.</param>
    /// <param name="position">The source position.</param>
    /// <returns>The containing property declaration, when present.</returns>
    private static PropertyDeclarationSyntax? FindPropertyDeclaration(SyntaxNode root, int position)
    {
        for (SyntaxNode? current = root.FindToken(position).Parent; current is not null; current = current.Parent)
        {
            if (current is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration;
            }
        }

        return null;
    }

    /// <summary>Determines whether a modifier list contains a token of the specified kind.</summary>
    /// <param name="modifiers">The modifiers to inspect.</param>
    /// <param name="kind">The token kind to find.</param>
    /// <returns><see langword="true"/> when the token is present.</returns>
    private static bool HasModifier(in SyntaxTokenList modifiers, SyntaxKind kind)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a modifier is an accessibility modifier.</summary>
    /// <param name="modifier">The modifier to inspect.</param>
    /// <returns><see langword="true"/> when the modifier controls accessibility.</returns>
    private static bool IsAccessibilityModifier(SyntaxToken modifier) =>
        modifier.IsKind(SyntaxKind.PublicKeyword)
        || modifier.IsKind(SyntaxKind.InternalKeyword)
        || modifier.IsKind(SyntaxKind.PrivateKeyword)
        || modifier.IsKind(SyntaxKind.ProtectedKeyword);
}
