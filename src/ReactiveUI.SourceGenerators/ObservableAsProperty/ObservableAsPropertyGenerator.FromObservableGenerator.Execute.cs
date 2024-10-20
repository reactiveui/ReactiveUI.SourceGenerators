// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.ObservableAsProperty.Models;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// Observable As Property From Observable Generator.
/// </summary>
/// <seealso cref="Microsoft.CodeAnalysis.IIncrementalGenerator" />
public sealed partial class ObservableAsPropertyGenerator
{
    internal static partial class Execute
    {
        internal static ImmutableArray<MemberDeclarationSyntax> GetPropertySyntax(ObservableMethodInfo propertyInfo)
        {
            var getterFieldIdentifierName = GetGeneratedFieldName(propertyInfo);

            // Get the property type syntax
            TypeSyntax propertyType = IdentifierName(propertyInfo.GetObservableTypeText());

            ArrowExpressionClauseSyntax getterArrowExpression;
            if (propertyType.ToFullString().EndsWith("?"))
            {
                getterArrowExpression = ArrowExpressionClause(ParseExpression($"{getterFieldIdentifierName} = ({getterFieldIdentifierName}Helper == null ? {getterFieldIdentifierName} : {getterFieldIdentifierName}Helper.Value)"));
            }
            else
            {
                getterArrowExpression = ArrowExpressionClause(ParseExpression($"{getterFieldIdentifierName} = {getterFieldIdentifierName}Helper?.Value ?? {getterFieldIdentifierName}"));
            }

            // Prepare the forwarded attributes, if any
            var forwardedAttributes =
                propertyInfo.ForwardedPropertyAttributes
                .Select(static a => AttributeList(SingletonSeparatedList(a.GetSyntax())))
                .ToImmutableArray();

            return ImmutableArray.Create<MemberDeclarationSyntax>(
                FieldDeclaration(VariableDeclaration(propertyType))
                        .AddDeclarationVariables(VariableDeclarator(getterFieldIdentifierName))
                        .AddAttributeLists(
                            AttributeList(SingletonSeparatedList(
                                Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                                .AddArgumentListArguments(
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                            .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{propertyInfo.PropertyName}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())))
                            .AddModifiers(
                                Token(SyntaxKind.PrivateKeyword)),
                FieldDeclaration(VariableDeclaration(ParseTypeName($"ReactiveUI.ObservableAsPropertyHelper<{propertyType}>?")))
                    .AddDeclarationVariables(VariableDeclarator(getterFieldIdentifierName + "Helper"))
                    .AddAttributeLists(
                        AttributeList(SingletonSeparatedList(
                            Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                            .AddArgumentListArguments(
                                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                        .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{getterFieldIdentifierName + "Helper"}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())))
                        .AddModifiers(
                            Token(SyntaxKind.PrivateKeyword)),
                PropertyDeclaration(propertyType, Identifier(propertyInfo.PropertyName))
                    .AddAttributeLists(
                        AttributeList(SingletonSeparatedList(
                            Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                            .AddArgumentListArguments(
                                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                        .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{getterFieldIdentifierName}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())),
                        AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverage)))))
                    .AddAttributeLists([.. forwardedAttributes])
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(getterArrowExpression)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))));
        }

        internal static MethodDeclarationSyntax GetPropertyInitiliser(ObservableMethodInfo[] propertyInfos)
        {
            using var propertyInitilisers = ImmutableArrayBuilder<StatementSyntax>.Rent();

            foreach (var propertyInfo in propertyInfos)
            {
                var fieldIdentifierName = GetGeneratedFieldName(propertyInfo);
                if (propertyInfo.IsProperty)
                {
                    propertyInitilisers.Add(ParseStatement($"{fieldIdentifierName}Helper = {propertyInfo.MethodName}!.ToProperty(this, nameof({propertyInfo.PropertyName}));"));
                }
                else
                {
                    propertyInitilisers.Add(ParseStatement($"{fieldIdentifierName}Helper = {propertyInfo.MethodName}()!.ToProperty(this, nameof({propertyInfo.PropertyName}));"));
                }
            }

            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("InitializeOAPH"))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ObservableAsPropertyGenerator).Assembly.GetName().Version.ToString())))))),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverage)))))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                .WithBody(Block(propertyInitilisers.ToImmutable()));
        }

        internal static bool IsObservableReturnType(ITypeSymbol? typeSymbol)
        {
            var nameFormat = SymbolDisplayFormat.FullyQualifiedFormat;
            do
            {
                var typeName = typeSymbol?.ToDisplayString(nameFormat);
                if (typeName?.Contains("global::System.IObservable") == true)
                {
                    return true;
                }

                typeSymbol = typeSymbol?.BaseType;
            }
            while (typeSymbol != null);

            return false;
        }

        /// <summary>
        /// Gathers all forwarded attributes for the generated field and property.
        /// </summary>
        /// <param name="methodSymbol">The input <see cref="IMethodSymbol" /> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel" /> instance for the current run.</param>
        /// <param name="methodDeclaration">The method declaration.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <param name="propertyAttributes">The resulting property attributes to forward.</param>
        internal static void GatherForwardedAttributesFromMethod(
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> propertyAttributes)
        {
            using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            static void GatherForwardedAttributesFromMethod(
                IMethodSymbol methodSymbol,
                SemanticModel semanticModel,
                MethodDeclarationSyntax methodDeclaration,
                CancellationToken token,
                ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfo)
            {
                // Get the single syntax reference for the input method symbol (there should be only one)
                if (methodSymbol.DeclaringSyntaxReferences is not [SyntaxReference syntaxReference])
                {
                    return;
                }

                // Gather explicit forwarded attributes info
                foreach (var attributeList in methodDeclaration.AttributeLists)
                {
                    if (attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.PropertyKeyword))
                    {
                        continue;
                    }

                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                        {
                            continue;
                        }

                        var attributeArguments = attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

                        // Try to extract the forwarded attribute
                        if (!AttributeInfo.TryCreate(attributeTypeSymbol, semanticModel, attributeArguments, token, out var attributeInfo))
                        {
                            continue;
                        }

                        // Add the new attribute info to the right builder
                        if (attributeList.Target?.Identifier is SyntaxToken(SyntaxKind.PropertyKeyword))
                        {
                            propertyAttributesInfo.Add(attributeInfo);
                        }
                    }
                }
            }

            // If the method is a partial definition, also gather attributes from the implementation part
            if (methodSymbol is { IsPartialDefinition: true } or { PartialDefinitionPart: not null })
            {
                var partialDefinition = methodSymbol.PartialDefinitionPart ?? methodSymbol;
                var partialImplementation = methodSymbol.PartialImplementationPart ?? methodSymbol;

                // We always give priority to the partial definition, to ensure a predictable and testable ordering
                GatherForwardedAttributesFromMethod(partialDefinition, semanticModel, methodDeclaration, token, propertyAttributesInfo);
                GatherForwardedAttributesFromMethod(partialImplementation, semanticModel, methodDeclaration, token, propertyAttributesInfo);
            }
            else
            {
                // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
                GatherForwardedAttributesFromMethod(methodSymbol, semanticModel, methodDeclaration, token, propertyAttributesInfo);
            }

            propertyAttributes = propertyAttributesInfo.ToImmutable();
        }

        internal static void GatherForwardedAttributesFromProperty(
            IPropertySymbol methodSymbol,
            SemanticModel semanticModel,
            PropertyDeclarationSyntax methodDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> propertyAttributes)
        {
            using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            static void GatherForwardedAttributesFromProperty(
                IPropertySymbol methodSymbol,
                SemanticModel semanticModel,
                PropertyDeclarationSyntax methodDeclaration,
                CancellationToken token,
                ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfo)
            {
                // Get the single syntax reference for the input method symbol (there should be only one)
                if (methodSymbol.DeclaringSyntaxReferences is not [SyntaxReference syntaxReference])
                {
                    return;
                }

                // Gather explicit forwarded attributes info
                foreach (var attributeList in methodDeclaration.AttributeLists)
                {
                    if (attributeList.Target?.Identifier is not SyntaxToken(SyntaxKind.PropertyKeyword))
                    {
                        continue;
                    }

                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out var attributeTypeSymbol))
                        {
                            continue;
                        }

                        var attributeArguments = attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>();

                        // Try to extract the forwarded attribute
                        if (!AttributeInfo.TryCreate(attributeTypeSymbol, semanticModel, attributeArguments, token, out var attributeInfo))
                        {
                            continue;
                        }

                        // Add the new attribute info to the right builder
                        if (attributeList.Target?.Identifier is SyntaxToken(SyntaxKind.PropertyKeyword))
                        {
                            propertyAttributesInfo.Add(attributeInfo);
                        }
                    }
                }
            }

            // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
            GatherForwardedAttributesFromProperty(methodSymbol, semanticModel, methodDeclaration, token, propertyAttributesInfo);

            propertyAttributes = propertyAttributesInfo.ToImmutable();
        }

        internal static string GetGeneratedFieldName(ObservableMethodInfo propertyInfo)
        {
            var commandName = propertyInfo.PropertyName;

            return $"_{char.ToLower(commandName[0], CultureInfo.InvariantCulture)}{commandName.Substring(1)}";
        }
    }
}
