// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ReactiveUI.SourceGenerators.CodeFixers.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators.CodeFixers;

/// <summary>
/// PropertyToFieldCodeFixProvider.
/// </summary>
/// <seealso cref="CodeFixProvider" />
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyToReactiveFieldCodeFixProvider))]
[Shared]
public class PropertyToReactiveFieldCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets a list of diagnostic IDs that this provider can provide fixes for.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(PropertyToReactiveFieldRule.Id);

    /// <summary>
    /// Gets an optional <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
    /// Return null if the provider doesn't support fix all/multiple occurrences.
    /// Otherwise, you can return any of the well known fix all providers from <see cref="T:Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders" /> or implement your own fix all provider.
    /// </summary>
    /// <returns>FixAllProvider.</returns>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
    /// </summary>
    /// <param name="context">A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
    /// The context must only contain diagnostics with a <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the property declaration syntax node
        var propertyDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

        var fieldName = propertyDeclaration?.Identifier.Text;
        fieldName = "_" + fieldName?.Substring(0, 1).ToLower() + fieldName?.Substring(1);

        var attributeSyntaxes =
            propertyDeclaration!.AttributeLists
            .Select(static a => AttributeList(a.Attributes)).ToList();
        attributeSyntaxes.Add(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("ReactiveUI.SourceGenerators.Reactive")))));

        SyntaxList<AttributeListSyntax> al = new(attributeSyntaxes);

        // Create a new field declaration syntax node
        var fieldDeclaration = FieldDeclaration(
            VariableDeclaration(propertyDeclaration!.Type)
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(fieldName).WithInitializer(propertyDeclaration.Initializer))))
            .WithAttributeLists(al)
            .WithLeadingTrivia(propertyDeclaration.GetLeadingTrivia())
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

        // Replace the property with the field
        var newRoot = root?.ReplaceNode(propertyDeclaration, fieldDeclaration);

        // Apply the code fix
        context.RegisterCodeFix(
            CodeAction.Create(
                "Convert to Reactive field",
                c => Task.FromResult(context.Document.WithSyntaxRoot(newRoot!)),
                "Convert to Reactive field"),
            diagnostic);
    }
}
