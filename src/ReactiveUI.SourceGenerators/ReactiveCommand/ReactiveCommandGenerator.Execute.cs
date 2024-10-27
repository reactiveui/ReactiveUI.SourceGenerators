// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Input.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators;

/// <summary>
/// ReactiveCommandGenerator.
/// </summary>
/// <seealso cref="IIncrementalGenerator" />
public partial class ReactiveCommandGenerator
{
    private const string ReactiveUI = "ReactiveUI";
    private const string ReactiveCommand = "ReactiveCommand";
    private const string RxCmd = ReactiveUI + "." + ReactiveCommand;
    private const string Create = ".Create";
    private const string CreateO = ".CreateFromObservable";
    private const string CreateT = ".CreateFromTask";
    private const string ObsoleteReason = "Commands are initialized automatically. Method will be removed in future version.";

    /// <summary>
    /// A container for all the logic for <see cref="ReactiveCommandGenerator"/>.
    /// </summary>
    internal static class Execute
    {
        internal static MethodDeclarationSyntax GetCommandInitiliser() => MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("InitializeCommands"))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ReactiveGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ReactiveGenerator).Assembly.GetName().Version.ToString())))))),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.ExcludeFromCodeCoverage)))),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName(AttributeDefinitions.Obsolete))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(ObsoleteReason)))))))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                .WithBody(Block());

        internal static MemberDeclarationSyntax[] GetCommandProperty(CommandInfo commandExtensionInfo)
        {
            var outputType = commandExtensionInfo.GetOutputTypeText();
            var inputType = commandExtensionInfo.GetInputTypeText();
            var commandName = GetGeneratedCommandName(commandExtensionInfo.MethodName, commandExtensionInfo.IsTask);
            var fieldName = GetGeneratedFieldName(commandName);

            ExpressionSyntax initializer;
            if (commandExtensionInfo.ArgumentType == null)
            {
                initializer = GenerateBasicCommand(commandExtensionInfo, fieldName);
            }
            else if (commandExtensionInfo.ArgumentType != null && commandExtensionInfo.IsReturnTypeVoid)
            {
                initializer = GenerateInCommand(commandExtensionInfo, fieldName, inputType);
            }
            else if (commandExtensionInfo.ArgumentType != null && !commandExtensionInfo.IsReturnTypeVoid)
            {
                initializer = GenerateInOutCommand(commandExtensionInfo, fieldName, outputType, inputType);
            }
            else
            {
                return [];
            }

            // Prepare any forwarded property attributes
            var forwardedPropertyAttributes =
            commandExtensionInfo.ForwardedPropertyAttributes
            .Select(static a => AttributeList(SingletonSeparatedList(a.GetSyntax())))
            .ToImmutableArray();

            var qualifiedName = QualifiedName(
                IdentifierName(ReactiveUI),
                GenericName(
                    Identifier(ReactiveCommand))
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SeparatedList<TypeSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                    IdentifierName(inputType),
                                    Token(SyntaxKind.CommaToken),
                                    IdentifierName(outputType)
                            }))));

            var fieldDeclaration = FieldDeclaration(
                VariableDeclaration(NullableType(qualifiedName)))
                .AddDeclarationVariables(VariableDeclarator(fieldName))
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName(AttributeDefinitions.GeneratedCode))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ReactiveCommandGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ReactiveCommandGenerator).Assembly.GetName().Version.ToString())))))))
                        .AddModifiers(
                        Token(SyntaxKind.PrivateKeyword))
                        .NormalizeWhitespace();

            var commandDeclaration = PropertyDeclaration(
                qualifiedName,
                Identifier(commandName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(ArrowExpressionClause(initializer)))
                .AddAttributeLists([.. forwardedPropertyAttributes])
                .NormalizeWhitespace();
            return [fieldDeclaration, commandDeclaration];

            static ExpressionSyntax GenerateBasicCommand(CommandInfo commandExtensionInfo, string fieldName)
            {
                var commandType = commandExtensionInfo.IsObservable ? CreateO : commandExtensionInfo.IsTask ? CreateT : Create;
                if (string.IsNullOrEmpty(commandExtensionInfo.CanExecuteObservableName))
                {
                    return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}({commandExtensionInfo.MethodName});");
                }

                return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}({commandExtensionInfo.MethodName}, {commandExtensionInfo.CanExecuteObservableName}{(commandExtensionInfo.CanExecuteTypeInfo == CanExecuteTypeInfo.MethodObservable ? "()" : string.Empty)});");
            }

            static ExpressionSyntax GenerateInOutCommand(CommandInfo commandExtensionInfo, string fieldName, string outputType, string inputType)
            {
                var commandType = commandExtensionInfo.IsObservable ? CreateO : commandExtensionInfo.IsTask ? CreateT : Create;
                if (string.IsNullOrEmpty(commandExtensionInfo.CanExecuteObservableName))
                {
                    return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}<{inputType}, {outputType}>({commandExtensionInfo.MethodName});");
                }

                return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}<{inputType}, {outputType}>({commandExtensionInfo.MethodName}, {commandExtensionInfo.CanExecuteObservableName}{(commandExtensionInfo.CanExecuteTypeInfo == CanExecuteTypeInfo.MethodObservable ? "()" : string.Empty)});");
            }

            static ExpressionSyntax GenerateInCommand(CommandInfo commandExtensionInfo, string fieldName, string inputType)
            {
                var commandType = commandExtensionInfo.IsTask ? CreateT : Create;
                if (string.IsNullOrEmpty(commandExtensionInfo.CanExecuteObservableName))
                {
                    return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}<{inputType}>({commandExtensionInfo.MethodName});");
                }

                return ParseExpression($"{fieldName} ??= {RxCmd}{commandType}<{inputType}>({commandExtensionInfo.MethodName}, {commandExtensionInfo.CanExecuteObservableName}{(commandExtensionInfo.CanExecuteTypeInfo == CanExecuteTypeInfo.MethodObservable ? "()" : string.Empty)});");
            }
        }

        internal static bool IsTaskReturnType(ITypeSymbol? typeSymbol)
        {
            var nameFormat = SymbolDisplayFormat.FullyQualifiedFormat;
            do
            {
                var typeName = typeSymbol?.ToDisplayString(nameFormat);
                if (typeName == "global::System.Threading.Tasks.Task")
                {
                    return true;
                }

                typeSymbol = typeSymbol?.BaseType;
            }
            while (typeSymbol != null);

            return false;
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

        internal static bool IsObservableBoolType(ITypeSymbol? typeSymbol)
        {
            var nameFormat = SymbolDisplayFormat.FullyQualifiedFormat;
            do
            {
                var typeName = typeSymbol?.ToDisplayString(nameFormat);
                if (typeName?.Contains("global::System.IObservable<bool>") == true)
                {
                    return true;
                }

                typeSymbol = typeSymbol?.BaseType;
            }
            while (typeSymbol != null);

            return false;
        }

        internal static ITypeSymbol GetTaskReturnType(Compilation compilation, ITypeSymbol typeSymbol) => typeSymbol switch
        {
            INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol => namedTypeSymbol.TypeArguments[0],
            _ => compilation.GetSpecialType(SpecialType.System_Void)
        };

        /// <summary>
        /// Tries to get the expression type for the "CanExecute" property, if available.
        /// </summary>
        /// <param name="methodSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
        /// <param name="attributeData">The <see cref="AttributeData"/> instance for <paramref name="methodSymbol"/>.</param>
        /// <param name="canExecuteMemberName">The resulting can execute member name, if available.</param>
        /// <param name="canExecuteTypeInfo">The resulting expression type, if available.</param>
        internal static void TryGetCanExecuteExpressionType(
            IMethodSymbol methodSymbol,
            AttributeData attributeData,
            out string? canExecuteMemberName,
            out CanExecuteTypeInfo? canExecuteTypeInfo)
        {
            // Get the can execute member, if any
            if (!attributeData.TryGetNamedArgument("CanExecute", out string? memberName))
            {
                canExecuteMemberName = null;
                canExecuteTypeInfo = null;

                return;
            }

            if (memberName is null)
            {
                goto Failure;
            }

            var canExecuteSymbols = methodSymbol.ContainingType!.GetAllMembers(memberName).ToImmutableArray();

            if (canExecuteSymbols.IsEmpty)
            {
                // Special case for when the target member is a generated property from [ObservableProperty]
                if (TryGetCanExecuteMemberFromGeneratedProperty(memberName, methodSymbol.ContainingType, out canExecuteTypeInfo))
                {
                    canExecuteMemberName = memberName;

                    return;
                }
            }
            else if (canExecuteSymbols.Length > 1)
            {
                goto Failure;
            }
            else if (TryGetCanExecuteExpressionFromSymbol(canExecuteSymbols[0], out canExecuteTypeInfo))
            {
                canExecuteMemberName = memberName;

                return;
            }

        Failure:
            canExecuteMemberName = null;
            canExecuteTypeInfo = null;

            return;
        }

        /// <summary>
        /// Gets the expression type for the can execute logic, if possible.
        /// </summary>
        /// <param name="canExecuteSymbol">The can execute member symbol (either a method or a property).</param>
        /// <param name="canExecuteTypeInfo">The resulting can execute expression type, if available.</param>
        /// <returns>Whether or not <paramref name="canExecuteTypeInfo"/> was set and the input symbol was valid.</returns>
        internal static bool TryGetCanExecuteExpressionFromSymbol(
            ISymbol canExecuteSymbol,
            [NotNullWhen(true)] out CanExecuteTypeInfo? canExecuteTypeInfo)
        {
            if (canExecuteSymbol is IMethodSymbol canExecuteMethodSymbol)
            {
                // The return type must always be a bool
                if (!IsObservableBoolType(canExecuteMethodSymbol.ReturnType))
                {
                    goto Failure;
                }

                // If the method has parameters, it has to have a single one matching the command type
                if (canExecuteMethodSymbol.Parameters.Length == 1)
                {
                    goto Failure;
                }

                // Parameterless methods are always valid
                if (canExecuteMethodSymbol.Parameters.IsEmpty)
                {
                    canExecuteTypeInfo = CanExecuteTypeInfo.MethodObservable;

                    return true;
                }
            }
            else if (canExecuteSymbol is IPropertySymbol { GetMethod: not null } canExecutePropertySymbol)
            {
                // The property type must always be a bool
                if (!IsObservableBoolType(canExecutePropertySymbol.Type))
                {
                    goto Failure;
                }

                canExecuteTypeInfo = CanExecuteTypeInfo.PropertyObservable;

                return true;
            }
            else if (canExecuteSymbol is IFieldSymbol canExecuteFieldSymbol)
            {
                // The property type must always be a bool
                if (!IsObservableBoolType(canExecuteFieldSymbol.Type))
                {
                    goto Failure;
                }

                canExecuteTypeInfo = CanExecuteTypeInfo.FieldObservable;

                return true;
            }

        Failure:
            canExecuteTypeInfo = null;

            return false;
        }

        /// <summary>
        /// Gets the expression type for the can execute logic, if possible.
        /// </summary>
        /// <param name="memberName">The member name passed to <c>[ReactiveCommand(CanExecute = ...)]</c>.</param>
        /// <param name="containingType">The containing type for the method annotated with <c>[ReactiveCommand]</c>.</param>
        /// <param name="canExecuteTypeInfo">The resulting can execute expression type, if available.</param>
        /// <returns>Whether or not <paramref name="canExecuteTypeInfo"/> was set and the input symbol was valid.</returns>
        internal static bool TryGetCanExecuteMemberFromGeneratedProperty(
            string memberName,
            INamedTypeSymbol containingType,
            [NotNullWhen(true)] out CanExecuteTypeInfo? canExecuteTypeInfo)
        {
            foreach (var memberSymbol in containingType.GetAllMembers())
            {
                // Only look for instance fields of Observable bool type
                if (!IsObservableBoolType(memberSymbol.ContainingType) || memberSymbol is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                var attributes = memberSymbol.GetAttributes();

                // Only filter fields with the [Reactive] attribute
                if (memberSymbol is IFieldSymbol &&
                    !attributes.Any(static a => a.AttributeClass?.HasFullyQualifiedMetadataName(
                        "ReactiveUI.SourceGenerators.ReactiveAttribute") == true))
                {
                    continue;
                }

                // Get the target property name either directly or matching the generated one
                var propertyName = fieldSymbol.GetGeneratedPropertyName();

                // If the generated property name matches, get the right expression type
                if (memberName == propertyName)
                {
                    canExecuteTypeInfo = CanExecuteTypeInfo.PropertyObservable;

                    return true;
                }
            }

            canExecuteTypeInfo = null;

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
        internal static void GatherForwardedAttributes(
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> propertyAttributes)
        {
            using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            static void GatherForwardedAttributes(
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
                GatherForwardedAttributes(partialDefinition, semanticModel, methodDeclaration, token, propertyAttributesInfo);
                GatherForwardedAttributes(partialImplementation, semanticModel, methodDeclaration, token, propertyAttributesInfo);
            }
            else
            {
                // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
                GatherForwardedAttributes(methodSymbol, semanticModel, methodDeclaration, token, propertyAttributesInfo);
            }

            propertyAttributes = propertyAttributesInfo.ToImmutable();
        }

        internal static string GetGeneratedCommandName(string methodName, bool isAsync)
        {
            var commandName = methodName;

            if (commandName.StartsWith("m_"))
            {
                commandName = commandName.Substring(2);
            }
            else if (commandName.StartsWith("_"))
            {
                commandName = commandName.TrimStart('_');
            }

            if (commandName.EndsWith("Async") && isAsync)
            {
                commandName = commandName.Substring(0, commandName.Length - "Async".Length);
            }

            return $"{char.ToUpper(commandName[0], CultureInfo.InvariantCulture)}{commandName.Substring(1)}Command";
        }

        internal static string GetGeneratedFieldName(string generatedCommandName) =>
            $"_{char.ToLower(generatedCommandName[0], CultureInfo.InvariantCulture)}{generatedCommandName.Substring(1)}";
    }
}
