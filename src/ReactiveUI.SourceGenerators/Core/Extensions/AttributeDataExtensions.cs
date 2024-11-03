// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="AttributeData"/> type.
/// </summary>
internal static class AttributeDataExtensions
{
    /// <summary>
    /// Checks whether a given <see cref="AttributeData"/> instance contains a specified named argument.
    /// </summary>
    /// <typeparam name="T">The type of argument to check.</typeparam>
    /// <param name="attributeData">The target <see cref="AttributeData"/> instance to check.</param>
    /// <param name="name">The name of the argument to check.</param>
    /// <param name="value">The expected value for the target named argument.</param>
    /// <returns>Whether or not <paramref name="attributeData"/> contains an argument named <paramref name="name"/> with the expected value.</returns>
    public static bool HasNamedArgument<T>(this AttributeData attributeData, string name, T? value)
    {
        foreach (var properties in attributeData.NamedArguments)
        {
            if (properties.Key == name)
            {
                return
                    properties.Value.Value is T argumentValue &&
                    EqualityComparer<T?>.Default.Equals(argumentValue, value);
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to get the location of the input <see cref="AttributeData"/> instance.
    /// </summary>
    /// <param name="attributeData">The input <see cref="AttributeData"/> instance to get the location for.</param>
    /// <returns>The resulting location for <paramref name="attributeData"/>, if a syntax reference is available.</returns>
    public static Location? GetLocation(this AttributeData attributeData)
    {
        if (attributeData.ApplicationSyntaxReference is { } syntaxReference)
        {
            return syntaxReference.SyntaxTree.GetLocation(syntaxReference.Span);
        }

        return null;
    }

    /// <summary>
    /// Gets a given named argument value from an <see cref="AttributeData"/> instance, or a fallback value.
    /// </summary>
    /// <typeparam name="T">The type of argument to check.</typeparam>
    /// <param name="attributeData">The target <see cref="AttributeData"/> instance to check.</param>
    /// <param name="name">The name of the argument to check.</param>
    /// <param name="fallback">The fallback value to use if the named argument is not present.</param>
    /// <returns>The argument named <paramref name="name"/>, or a fallback value.</returns>
    public static T? GetNamedArgument<T>(this AttributeData attributeData, string name, T? fallback = default)
    {
        if (attributeData.TryGetNamedArgument(name, out T? value))
        {
            return value;
        }

        return fallback;
    }

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
    /// Gets the attribute syntax as a string for generating code.
    /// </summary>
    /// <param name="attribute">The attribute data from the original code.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <returns>A class array containing the syntax and other relevant metadata.</returns>
    public static PropertyAttributeData? GetAttributeSyntax(this AttributeData attribute, CancellationToken token)
    {
        // Retrieve the syntax from the attribute reference.
        if (attribute.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax)
        {
            // If the syntax is not available, return an empty string.
            return null;
        }

        // Normalize the syntax for correct formatting and return it as a string.
        return new(attribute.AttributeClass?.ContainingNamespace?.ToDisplayString(SymbolHelpers.DefaultDisplay), syntax.NormalizeWhitespace().ToFullString());
    }

    /// <summary>
    /// Generates a string containing the applicable attributes for a given target (e.g., field or property).
    /// </summary>
    /// <param name="attributes">The collection of attribute data to process.</param>
    /// <param name="allowedTarget">The attribute target (e.g., property, field).</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A class array containing the syntax and other relevant metadata.</returns>
    public static PropertyAttributeData[] GenerateAttributes(
        this IEnumerable<AttributeData> attributes,
        AttributeTargets allowedTarget,
        CancellationToken token)
    {
        // Filter and convert each attribute to its syntax form, ensuring it can target the given element type.
        var applicableAttributes = attributes
            .Where(attr => attr.AttributeClass.AttributeCanTarget(allowedTarget))
            .Select(attr => attr.GetAttributeSyntax(token))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToImmutableArray();

        return [.. applicableAttributes];
    }

    /// <summary>
    /// Generates the formatted attributes for fields and properties.
    /// </summary>
    /// <param name="attr">The attribute to format.</param>
    /// <returns>A formatted string of attributes.</returns>
    public static string FormatAttributes(this PropertyAttributeData attr)
    {
        // If the attribute namespace is null, omit the dot (.) separator.
        var namespacePrefix = string.IsNullOrEmpty(attr.AttributeNamespace)
            ? string.Empty
            : $"{attr.AttributeNamespace}.";

        return $"[{namespacePrefix}{attr.AttributeSyntax}]";
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
