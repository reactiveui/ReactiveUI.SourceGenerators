// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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

/// <summary>Extension methods for the <see cref="AttributeData"/> type.</summary>
internal static class AttributeDataExtensions
{
    /// <summary>Provides extension members for attribute data.</summary>
    /// <param name="attributeData">The attribute data to extend.</param>
    extension(AttributeData attributeData)
    {
        /// <summary>Tries to get a given named argument value from this instance, if present.</summary>
        /// <typeparam name="T">The type of argument to check.</typeparam>
        /// <param name="name">The name of the argument to check.</param>
        /// <param name="value">The resulting argument value, if present.</param>
        /// <returns>Whether this instance contains an argument named <paramref name="name"/> with a valid value.</returns>
        internal bool TryGetNamedArgument<T>(string name, out T? value)
        {
            if (attributeData is null)
            {
                value = default;
                return false;
            }

            // NamedArguments returns a default ImmutableArray when attribute data is incomplete/malformed.
            // Guard with IsDefaultOrEmpty rather than catching NullReferenceException to avoid masking real bugs.
            if (!attributeData.NamedArguments.IsDefaultOrEmpty)
            {
                foreach (var properties in attributeData.NamedArguments)
                {
                    if (properties.Key == name)
                    {
                        return TryConvertNamedArgument(properties.Value, out value);
                    }
                }
            }

            value = default;
            return false;
        }

        /// <summary>Gets a named argument from this instance.</summary>
        /// <typeparam name="T">The type of argument to get.</typeparam>
        /// <param name="name">The name of the argument.</param>
        /// <returns>The named argument value.</returns>
        internal T? GetNamedArgument<T>(string name)
        {
            if (attributeData is null)
            {
                return default;
            }

            // NamedArguments returns a default ImmutableArray when attribute data is incomplete/malformed.
            // Guard with IsDefaultOrEmpty rather than catching NullReferenceException to avoid masking real bugs.
            if (attributeData.NamedArguments.IsDefaultOrEmpty)
            {
                return default;
            }

            foreach (var properties in attributeData.NamedArguments)
            {
                if (properties.Key == name)
                {
                    return TryConvertNamedArgument(properties.Value, out T? value) ? value : default;
                }
            }

            return default;
        }

        /// <summary>Enumerates all items in a flattened sequence of constructor arguments from this instance.</summary>
        /// <typeparam name="T">The type of constructor arguments to retrieve.</typeparam>
        /// <returns>A sequence of all constructor arguments of the specified type.</returns>
        internal IEnumerable<T?> GetConstructorArguments<T>()
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

                    if (constant.Kind == TypedConstantKind.Primitive
                        && constant.Value is T value)
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

        /// <summary>Gathers forwarded attributes from a class for this instance.</summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="classDeclaration">The class declaration.</param>
        /// <param name="token">The token.</param>
        /// <param name="classAttributesInfo">The class attributes information.</param>
        internal void GatherForwardedAttributesFromClass(
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

        /// <summary>Gets the generic type from this instance.</summary>
        /// <returns>The generic type name, if present.</returns>
        internal string? GetGenericType()
        {
            var attributeClassName = attributeData.AttributeClass?.ToDisplayString();
            if (string.IsNullOrWhiteSpace(attributeClassName))
            {
                return null;
            }

            var start = attributeClassName!.IndexOf('<');
            var end = attributeClassName.LastIndexOf('>');

            return start >= 0 && end > start
                ? attributeClassName.Substring(start + 1, end - start - 1)
                : null;
        }
    }

    /// <summary>Tries to convert a typed constant to the requested value type.</summary>
    /// <typeparam name="T">The requested value type.</typeparam>
    /// <param name="typedConstant">The typed constant to convert.</param>
    /// <param name="value">The converted value, if conversion succeeds.</param>
    /// <returns>Whether conversion succeeds.</returns>
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

    /// <summary>Gets the raw value represented by a typed constant.</summary>
    /// <param name="typedConstant">The typed constant to inspect.</param>
    /// <returns>The raw value, if available.</returns>
    private static object? TryGetRawValue(in TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Error)
        {
            return null;
        }

        return typedConstant.Type?.TypeKind == TypeKind.Enum
            ? GetEnumRawValue(typedConstant)
            : typedConstant.Value;
    }

    /// <summary>Gets the raw value represented by an enum typed constant.</summary>
    /// <param name="typedConstant">The enum typed constant to inspect.</param>
    /// <returns>The raw enum value, if available.</returns>
    private static object? GetEnumRawValue(in TypedConstant typedConstant)
    {
        if (typedConstant.Value is IFieldSymbol fieldSymbol)
        {
            return fieldSymbol.ConstantValue;
        }

        return typedConstant.Value
            ?? (typedConstant.Type is INamedTypeSymbol enumType
                ? GetEnumMemberValue(enumType, typedConstant.ToCSharpString())
                : null);
    }

    /// <summary>Gets the constant value of an enum member rendered in C# source.</summary>
    /// <param name="enumType">The enum type containing the member.</param>
    /// <param name="csharpValue">The rendered enum value.</param>
    /// <returns>The enum member constant value, if it can be resolved.</returns>
    private static object? GetEnumMemberValue(INamedTypeSymbol enumType, string csharpValue)
    {
        if (string.IsNullOrWhiteSpace(csharpValue))
        {
            return null;
        }

        var separatorIndex = csharpValue.LastIndexOf('.');
        var enumMemberName = csharpValue[(separatorIndex + 1)..];
        foreach (var member in enumType.GetMembers(enumMemberName))
        {
            if (member is IFieldSymbol enumMember)
            {
                return enumMember.ConstantValue;
            }
        }

        return null;
    }
}
