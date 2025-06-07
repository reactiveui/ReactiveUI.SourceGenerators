// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="ITypeSymbol"/> type.
/// </summary>
internal static class ITypeSymbolExtensions
{
    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> is or inherits from <paramref name="name"/>.</returns>
    public static bool HasOrInheritsFromFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> inherits from <paramref name="name"/>.</returns>
    public static bool InheritsFromFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        var baseType = typeSymbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> implements a specified interface.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the interface to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> implements <paramref name="name"/>.</returns>
    public static bool ImplementsFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        foreach (var implementedInterface in typeSymbol.AllInterfaces)
        {
            if (implementedInterface.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> is or inherits from <paramref name="name"/>.</returns>
    public static bool HasOrInheritsFromFullyQualifiedMetadataNameStartingWith(this ITypeSymbol typeSymbol, string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.ContainsFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> inherits from <paramref name="name"/>.</returns>
    public static bool InheritsFromFullyQualifiedMetadataNameStartingWith(this ITypeSymbol typeSymbol, string name)
    {
        var baseType = typeSymbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.ContainsFullyQualifiedMetadataName(name))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits a specified attribute.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The name of the attribute to look for.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> has an attribute with the specified type name.</returns>
    public static bool HasOrInheritsAttributeWithFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.HasAttributeWithFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given type symbol has a specified fully qualified metadata name.
    /// </summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name to check.</param>
    /// <returns>Whether <paramref name="symbol"/> has a full name equals to <paramref name="name"/>.</returns>
    public static bool HasFullyQualifiedMetadataName(this ITypeSymbol symbol, string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(builder);

        return builder.WrittenSpan.StartsWith(name.AsSpan());
    }

    public static bool ContainsFullyQualifiedMetadataName(this ITypeSymbol symbol, string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(builder);

        return builder.WrittenSpan.ToString().Contains(name);
    }

    /// <summary>
    /// Gets the fully qualified metadata name for a given <see cref="ITypeSymbol"/> instance.
    /// </summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance.</param>
    /// <returns>The fully qualified metadata name for <paramref name="symbol"/>.</returns>
    public static string GetFullyQualifiedMetadataName(this ITypeSymbol symbol)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(builder);

        return builder.ToString();
    }

    public static bool IsTaskReturnType(this ITypeSymbol? typeSymbol)
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

    public static bool IsObservableReturnType(this ITypeSymbol? typeSymbol)
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

    public static bool IsISchedulerType(this ITypeSymbol? typeSymbol)
    {
        var nameFormat = SymbolDisplayFormat.FullyQualifiedFormat;
        do
        {
            var typeName = typeSymbol?.ToDisplayString(nameFormat);
            if (typeName == "global::System.Reactive.Concurrency.IScheduler")
            {
                return true;
            }

            typeSymbol = typeSymbol?.BaseType;
        }
        while (typeSymbol != null);
        return false;
    }

    public static bool IsObservableBoolType(this ITypeSymbol? typeSymbol)
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

    /// <summary>
    /// Determines whether [is nullable type].
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <returns>
    ///   <c>true</c> if [is nullable type] [the specified type symbol]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullableType(this ITypeSymbol? typeSymbol) => typeSymbol?.NullableAnnotation == NullableAnnotation.Annotated;

    public static ITypeSymbol GetTaskReturnType(this ITypeSymbol typeSymbol, Compilation compilation) => typeSymbol switch
    {
        INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol => namedTypeSymbol.TypeArguments[0],
        _ => compilation.GetSpecialType(SpecialType.System_Void)
    };

    /// <summary>
    /// Appends the fully qualified metadata name for a given symbol to a target builder.
    /// </summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance.</param>
    /// <param name="builder">The target <see cref="ImmutableArrayBuilder{T}"/> instance.</param>
    private static void AppendFullyQualifiedMetadataName(this ITypeSymbol symbol, ImmutableArrayBuilder<char> builder)
    {
        static void BuildFrom(ISymbol? symbol, ImmutableArrayBuilder<char> builder)
        {
            switch (symbol)
            {
                // Namespaces that are nested also append a leading '.'
                case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    BuildFrom(symbol.ContainingNamespace, builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Other namespaces (ie. the one right before global) skip the leading '.'
                case INamespaceSymbol { IsGlobalNamespace: false }:
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Types with no namespace just have their metadata name directly written
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Types with a containing non-global namespace also append a leading '.'
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                    BuildFrom(namespaceSymbol, builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Nested types append a leading '+'
                case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                    BuildFrom(typeSymbol, builder);
                    builder.Add('+');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;
                default:
                    break;
            }
        }

        BuildFrom(symbol, builder);
    }
}
