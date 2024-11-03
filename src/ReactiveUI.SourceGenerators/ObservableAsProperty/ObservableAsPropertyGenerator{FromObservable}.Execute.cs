// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.ObservableAsProperty.Models;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// Observable As Property From Observable Generator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
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
                        AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverageString)))))
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
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverageString)))))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                .WithBody(Block(propertyInitilisers.ToImmutable()));
        }

        internal static string GetGeneratedFieldName(ObservableMethodInfo propertyInfo)
        {
            var commandName = propertyInfo.PropertyName;

            return $"_{char.ToLower(commandName[0], CultureInfo.InvariantCulture)}{commandName.Substring(1)}";
        }
    }
}
