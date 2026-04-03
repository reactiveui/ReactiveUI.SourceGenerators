// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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
        if (attributeData is null)
        {
            value = default;
            return false;
        }

        try
        {
            foreach (var properties in attributeData.NamedArguments)
            {
                if (properties.Key == name)
                {
                    return TryConvertNamedArgument(properties.Value, out value);
                }
            }
        }
        catch (NullReferenceException)
        {
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Gets the named argument.
    /// </summary>
    /// <typeparam name="T">The type of argument to get.</typeparam>
    /// <param name="attributeData">The attribute data.</param>
    /// <param name="name">The name.</param>
    /// <returns>The named argument value.</returns>
    public static T? GetNamedArgument<T>(this AttributeData attributeData, string name)
    {
        if (attributeData is null)
        {
            return default;
        }

        try
        {
            foreach (var properties in attributeData.NamedArguments)
            {
                if (properties.Key == name)
                {
                    return TryConvertNamedArgument(properties.Value, out T? value) ? value : default;
                }
            }
        }
        catch (NullReferenceException)
        {
        }

        return default;
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
    /// Gets the type of the generic.
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <returns>A String.</returns>
    public static string? GetGenericType(this AttributeData attributeData)
    {
        var success = attributeData?.AttributeClass?.ToDisplayString();
        if (string.IsNullOrWhiteSpace(success))
        {
            return null;
        }

        var attributeClassName = success ?? string.Empty;
        var start = attributeClassName.IndexOf('<');
        var end = attributeClassName.LastIndexOf('>');

        return start >= 0 && end > start
            ? attributeClassName.Substring(start + 1, end - start - 1)
            : null;
    }

    private static bool TryConvertNamedArgument<T>(in TypedConstant typedConstant, out T? value)
    {
        var rawValue = TryGetRawValue(typedConstant);

        if (rawValue is null)
        {
            value = default;
            return false;
        }

        if (rawValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType.IsEnum)
        {
            try
            {
                if (rawValue is string enumName)
                {
                    value = (T)Enum.Parse(targetType, enumName, ignoreCase: false);
                    return true;
                }

                value = (T)Enum.ToObject(targetType, rawValue);
                return true;
            }
            catch (ArgumentException)
            {
                value = default;
                return false;
            }
        }

        try
        {
            value = (T)Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch (InvalidCastException)
        {
            value = default;
            return false;
        }
        catch (FormatException)
        {
            value = default;
            return false;
        }
        catch (OverflowException)
        {
            value = default;
            return false;
        }
    }

    private static object? TryGetRawValue(in TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Error)
        {
            return null;
        }

        if (typedConstant.Type?.TypeKind == TypeKind.Enum)
        {
            if (typedConstant.Value is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.ConstantValue;
            }

            if (typedConstant.Value is not null)
            {
                return typedConstant.Value;
            }

            if (typedConstant.Type is INamedTypeSymbol enumType)
            {
                var csharpValue = typedConstant.ToCSharpString();

                if (!string.IsNullOrWhiteSpace(csharpValue))
                {
                    var enumMemberName = csharpValue.Split('.').LastOrDefault();
                    return enumType.GetMembers(enumMemberName ?? string.Empty).OfType<IFieldSymbol>().FirstOrDefault()?.ConstantValue;
                }
            }

            return null;
        }

        return typedConstant.Value;
    }
}
