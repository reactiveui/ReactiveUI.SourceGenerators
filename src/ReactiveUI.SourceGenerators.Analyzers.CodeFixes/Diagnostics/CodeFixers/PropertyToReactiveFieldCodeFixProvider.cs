// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Provides fixes that convert eligible properties to reactive fields.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToReactiveFieldCodeFixProvider))]
public sealed class PropertyToReactiveFieldCodeFixProvider : CodeFixProvider
{
    /// <summary>Gets a list of diagnostic IDs that this provider can fix.</summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(PropertyToReactiveFieldRule.Id);

    /// <summary>Gets the batch fix provider.</summary>
    /// <returns>The batch fix provider.</returns>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>Registers the available fixes.</summary>
    /// <param name="context">The code-fix context.</param>
    /// <returns>A task that completes after the fixes are registered.</returns>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var propertyDeclaration = FindPropertyDeclaration(root, diagnostic.Location.SourceSpan.Start);
        if (propertyDeclaration is null)
        {
            return;
        }

        var propertyName = propertyDeclaration.Identifier.Text;
        var fieldName = $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Remove(0, 1)}";
        var attributeSyntaxes = new List<AttributeListSyntax>();
        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            attributeSyntaxes.Add(AttributeList(attributeList.Attributes));
        }

        attributeSyntaxes.Add(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("ReactiveUI.SourceGenerators.Reactive")))));

        var fieldDeclaration = FieldDeclaration(
            VariableDeclaration(propertyDeclaration.Type)
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(fieldName).WithInitializer(propertyDeclaration.Initializer))))
            .WithAttributeLists(new(attributeSyntaxes))
            .WithLeadingTrivia(propertyDeclaration.GetLeadingTrivia())
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        var newRoot = root?.ReplaceNode(propertyDeclaration, fieldDeclaration);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Convert to Reactive field",
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot!)),
                "Convert to Reactive field"),
            diagnostic);
    }

    /// <summary>Finds the property declaration containing a source position.</summary>
    /// <param name="root">The syntax-tree root.</param>
    /// <param name="position">The source position.</param>
    /// <returns>The containing property declaration, when present.</returns>
    private static PropertyDeclarationSyntax? FindPropertyDeclaration(SyntaxNode? root, int position)
    {
        for (SyntaxNode? current = root?.FindToken(position).Parent; current is not null; current = current.Parent)
        {
            if (current is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration;
            }
        }

        return null;
    }
}
