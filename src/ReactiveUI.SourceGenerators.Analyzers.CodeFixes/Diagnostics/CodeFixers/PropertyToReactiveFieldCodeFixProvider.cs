// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>Provides fixes that convert eligible properties to <c>[Reactive]</c> properties.</summary>
/// <remarks>
/// Where the project's language version allows it, the property becomes a <c>[Reactive]</c> partial property, and its
/// containing types become partial: the property stays the one the code declares, so other generators can bind to it.
/// Otherwise it becomes a <c>[Reactive]</c> field, from which the generator writes the property.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToReactiveFieldCodeFixProvider))]
public sealed class PropertyToReactiveFieldCodeFixProvider : CodeFixProvider
{
    /// <summary>The C# version that allows partial properties.</summary>
    private const int PartialPropertyLanguageVersion = 1300;

    /// <summary>The C# version that allows a partial property to have an initializer.</summary>
    private const int PartialPropertyInitializerLanguageVersion = 1400;

    /// <summary>The first Roslyn release whose generator builds generate partial properties.</summary>
    private static readonly Version PartialPropertyRoslynVersion = new(4, 14);

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
        if (root is null || FindPropertyDeclaration(root, diagnostic.Location.SourceSpan.Start) is not { } propertyDeclaration)
        {
            return;
        }

        if (CanUsePartialProperty(propertyDeclaration))
        {
            const string title = "Convert to Reactive partial property";
            var partialRoot = ConvertToPartialProperty(root, propertyDeclaration);
            context.RegisterCodeFix(
                CodeAction.Create(title, _ => Task.FromResult(context.Document.WithSyntaxRoot(partialRoot)), title),
                diagnostic);
            return;
        }

        const string fieldTitle = "Convert to Reactive field";
        var fieldRoot = root.ReplaceNode(propertyDeclaration, ConvertToField(propertyDeclaration));
        context.RegisterCodeFix(
            CodeAction.Create(fieldTitle, _ => Task.FromResult(context.Document.WithSyntaxRoot(fieldRoot)), fieldTitle),
            diagnostic);
    }

    /// <summary>Determines whether a property can become a <c>[Reactive]</c> partial property.</summary>
    /// <param name="propertyDeclaration">The property.</param>
    /// <returns>
    /// <see langword="true"/> when the language version allows the partial property, and its initializer if it has one,
    /// and the running compiler's generator build generates partial properties.
    /// </returns>
    private static bool CanUsePartialProperty(PropertyDeclarationSyntax propertyDeclaration)
    {
        var languageVersion = (int)((CSharpParseOptions)propertyDeclaration.SyntaxTree.Options).LanguageVersion;
        var requiredVersion = propertyDeclaration.Initializer is null ? PartialPropertyLanguageVersion : PartialPropertyInitializerLanguageVersion;
        return languageVersion >= requiredVersion
            && typeof(CSharpSyntaxTree).Assembly.GetName().Version >= PartialPropertyRoslynVersion;
    }

    /// <summary>Makes a property a <c>[Reactive]</c> partial property, and its containing types partial.</summary>
    /// <param name="root">The syntax root.</param>
    /// <param name="propertyDeclaration">The property.</param>
    /// <returns>The updated syntax root.</returns>
    private static SyntaxNode ConvertToPartialProperty(SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration)
    {
        var nodes = new List<SyntaxNode> { propertyDeclaration };
        foreach (var type in propertyDeclaration.Ancestors())
        {
            if (type is TypeDeclarationSyntax typeDeclaration && !typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                nodes.Add(typeDeclaration);
            }
        }

        // The nodes are the property and its containing types, so anything but the property is a type.
        return root.ReplaceNodes(nodes, static (_, rewritten) => rewritten is PropertyDeclarationSyntax property
            ? property
                .WithoutLeadingTrivia()
                .AddModifiers(Token(SyntaxKind.PartialKeyword))
                .AddAttributeLists(ReactiveAttributeList())
                .WithLeadingTrivia(property.GetLeadingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation)
            : ((TypeDeclarationSyntax)rewritten).AddModifiers(Token(SyntaxKind.PartialKeyword)));
    }

    /// <summary>Makes a <c>[Reactive]</c> field for a property.</summary>
    /// <param name="propertyDeclaration">The property.</param>
    /// <returns>The field declaration.</returns>
    private static FieldDeclarationSyntax ConvertToField(PropertyDeclarationSyntax propertyDeclaration)
    {
        var propertyName = propertyDeclaration.Identifier.Text;
        var fieldName = $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Remove(0, 1)}";
        var attributeSyntaxes = new List<AttributeListSyntax>();
        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            attributeSyntaxes.Add(AttributeList(attributeList.Attributes));
        }

        attributeSyntaxes.Add(ReactiveAttributeList());

        return FieldDeclaration(
            VariableDeclaration(propertyDeclaration.Type)
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(fieldName).WithInitializer(propertyDeclaration.Initializer))))
            .WithAttributeLists(new(attributeSyntaxes))
            .WithLeadingTrivia(propertyDeclaration.GetLeadingTrivia())
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
    }

    /// <summary>Makes the <c>[ReactiveUI.SourceGenerators.Reactive]</c> attribute list.</summary>
    /// <returns>The attribute list.</returns>
    private static AttributeListSyntax ReactiveAttributeList() =>
        AttributeList(SingletonSeparatedList(Attribute(IdentifierName("ReactiveUI.SourceGenerators.Reactive"))));

    /// <summary>Finds the property declaration containing a source position.</summary>
    /// <param name="root">The syntax-tree root.</param>
    /// <param name="position">The source position.</param>
    /// <returns>The containing property declaration, when present.</returns>
    private static PropertyDeclarationSyntax? FindPropertyDeclaration(SyntaxNode root, int position)
    {
        for (var current = root.FindToken(position).Parent; current is not null; current = current.Parent)
        {
            if (current is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration;
            }
        }

        return null;
    }
}
