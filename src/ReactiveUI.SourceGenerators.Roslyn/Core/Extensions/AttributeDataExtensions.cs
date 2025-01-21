// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="AttributeData"/> type.
/// </summary>
internal static class AttributeDataExtensions
{
    /// <summary>
    /// Tries to get a given named argument value from an <see cref="AttributeData"/> instance, if present.
    /// </summary>
    /// <typeparam name="T">The type of argument to check.</typeparam>
    /// <param name="attributeData">The target <see cref="AttributeData"/> instance to check.</param>
    /// <param name="name">The name of the argument to check.</param>
    /// <param name="value">The resulting argument value, if present.</param>
    /// <returns>Whether or not <paramref name="attributeData"/> contains an argument named <paramref name="name"/> with a valid value.</returns>
    public static bool TryGetNamedArgument<T>(this AttributeData attributeData, string name, out T? value)
    {
        foreach (var properties in attributeData.NamedArguments)
        {
            if (properties.Key == name)
            {
                value = (T?)properties.Value.Value;

                return true;
            }
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Enumerates all items in a flattened sequence of constructor arguments for a given <see cref="AttributeData"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of constructor arguments to retrieve.</typeparam>
    /// <param name="attributeData">The target <see cref="AttributeData"/> instance to get the arguments from.</param>
    /// <returns>A sequence of all constructor arguments of the specified type from <paramref name="attributeData"/>.</returns>
    public static IEnumerable<T?> GetConstructorArguments<T>(this AttributeData attributeData)
        where T : class
    {
        static IEnumerable<T?> Enumerate(IEnumerable<TypedConstant> constants)
        {
            foreach (var constant in constants)
            {
                if (constant.IsNull)
                {
                    yield return null;
                }

                if (constant.Kind == TypedConstantKind.Primitive &&
                    constant.Value is T value)
                {
                    yield return value;
                }
                else if (constant.Kind == TypedConstantKind.Array)
                {
                    foreach (var item in Enumerate(constant.Values))
                    {
                        yield return item;
                    }
                }
            }
        }

        return Enumerate(attributeData.ConstructorArguments);
    }

    /// <summary>
    /// Gathers the forwarded attributes from class.
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="token">The token.</param>
    /// <param name="classAttributesInfo">The class attributes information.</param>
    public static void GatherForwardedAttributesFromClass(
            this AttributeData attributeData,
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> classAttributesInfo)
    {
        using var classAttributesInfoBuilder = ImmutableArrayBuilder<AttributeInfo>.Rent();

        static void GatherForwardedAttributes(
            AttributeData attributeData,
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken token,
            ImmutableArrayBuilder<AttributeInfo> classAttributesInfo)
        {
            // Gather explicit forwarded attributes info
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
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

                    var ignoreAttribute = attributeData.AttributeClass?.GetFullyQualifiedMetadataName();
                    if (attributeInfo.TypeName.Contains(ignoreAttribute))
                    {
                        continue;
                    }

                    // Add the new attribute info to the right builder
                    classAttributesInfo.Add(attributeInfo);
                }
            }
        }

        // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
        GatherForwardedAttributes(attributeData, semanticModel, classDeclaration, token, classAttributesInfoBuilder);

        classAttributesInfo = classAttributesInfoBuilder.ToImmutable();
    }

    /// <summary>
    /// Gathers all forwarded attributes for the generated field and property.
    /// </summary>
    /// <param name="methodSymbol">The input <see cref="IMethodSymbol" /> instance to process.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel" /> instance for the current run.</param>
    /// <param name="methodDeclaration">The method declaration.</param>
    /// <param name="token">The cancellation token for the current operation.</param>
    /// <param name="propertyAttributeInfos">The resulting property attributes to forward.</param>
    public static void GatherForwardedAttributesFromMethod(
        this IMethodSymbol methodSymbol,
        SemanticModel semanticModel,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken token,
        out ImmutableArray<AttributeInfo> propertyAttributeInfos)
    {
        using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

        static void GatherForwardedAttributesFromMethod(
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodDeclaration,
            CancellationToken token,
            ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfos)
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
                        propertyAttributesInfos.Add(attributeInfo);
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

        propertyAttributeInfos = propertyAttributesInfo.ToImmutable();
    }

    public static void GatherForwardedAttributesFromProperty(
            this IPropertySymbol propertySymbol,
            SemanticModel semanticModel,
            PropertyDeclarationSyntax propertyDeclaration,
            CancellationToken token,
            out ImmutableArray<AttributeInfo> propertyAttributesInfos)
    {
        using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

        static void GatherForwardedAttributesFromProperty(
            IPropertySymbol propertySymbol,
            SemanticModel semanticModel,
            PropertyDeclarationSyntax propertyDeclaration,
            CancellationToken token,
            ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfos)
        {
            // Get the single syntax reference for the input method symbol (there should be only one)
            if (propertySymbol.DeclaringSyntaxReferences is not [SyntaxReference syntaxReference])
            {
                return;
            }

            // Gather explicit forwarded attributes info
            foreach (var attributeList in propertyDeclaration.AttributeLists)
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
                        propertyAttributesInfos.Add(attributeInfo);
                    }
                }
            }
        }

        // If the method is not a partial definition/implementation, just gather attributes from the method with no modifications
        GatherForwardedAttributesFromProperty(propertySymbol, semanticModel, propertyDeclaration, token, propertyAttributesInfo);

        propertyAttributesInfos = propertyAttributesInfo.ToImmutable();
    }

    /// <summary>
    /// Gets the type of the generic.
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <returns>A String.</returns>
    public static string? GetGenericType(this AttributeData attributeData)
    {
        var success = attributeData?.AttributeClass?.ToDisplayString();
        var start = success?.IndexOf('<') + 1 ?? 0;
        return success?.Substring(start, success.Length - start - 1);
    }
}
