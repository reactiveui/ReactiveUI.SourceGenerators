// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="ITypeSymbol"/> type.</summary>
internal static class ITypeSymbolExtensions
{
    /// <summary>Provides metadata-name and hierarchy operations for a type symbol.</summary>
    /// <param name="typeSymbol">The type symbol receiving the extension operation.</param>
    extension(ITypeSymbol typeSymbol)
    {
    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol is or inherits from <paramref name="name"/>.</returns>
    internal bool HasOrInheritsFromFullyQualifiedMetadataName(string name)
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

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol inherits from <paramref name="name"/>.</returns>
    internal bool InheritsFromFullyQualifiedMetadataName(string name)
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

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> implements a specified interface.</summary>
    /// <param name="name">The full name of the interface to check for inheritance.</param>
    /// <returns>Whether the type symbol implements <paramref name="name"/>.</returns>
    internal bool ImplementsFullyQualifiedMetadataName(string name)
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

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol is or inherits from <paramref name="name"/>.</returns>
    internal bool HasOrInheritsFromFullyQualifiedMetadataNameStartingWith(string name)
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

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> inherits from a specified type.</summary>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether the type symbol inherits from <paramref name="name"/>.</returns>
    internal bool InheritsFromFullyQualifiedMetadataNameStartingWith(string name)
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

    /// <summary>Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits a specified attribute.</summary>
    /// <param name="name">The name of the attribute to look for.</param>
    /// <returns>Whether the type symbol has an attribute with the specified type name.</returns>
    internal bool HasOrInheritsAttributeWithFullyQualifiedMetadataName(string name)
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

    /// <summary>Checks whether or not a given type symbol has a specified fully qualified metadata name.</summary>
    /// <param name="name">The full name to check.</param>
    /// <returns>Whether the type symbol has a full name equal to <paramref name="name"/>.</returns>
    internal bool HasFullyQualifiedMetadataName(string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        AppendFullyQualifiedMetadataName(typeSymbol, builder);

        return builder.WrittenSpan.StartsWith(name.AsSpan());
    }

    /// <summary>Checks whether a type symbol's metadata name contains a given value.</summary>
    /// <param name="name">The value to find in the full metadata name.</param>
    /// <returns>Whether the metadata name contains <paramref name="name"/>.</returns>
    internal bool ContainsFullyQualifiedMetadataName(string name)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        AppendFullyQualifiedMetadataName(typeSymbol, builder);

        return builder.WrittenSpan.ToString().Contains(name);
    }

    /// <summary>Gets the fully qualified metadata name for a given <see cref="ITypeSymbol"/> instance.</summary>
    /// <returns>The fully qualified metadata name for the type symbol.</returns>
    internal string GetFullyQualifiedMetadataName()
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        AppendFullyQualifiedMetadataName(typeSymbol, builder);

        return builder.ToString();
    }

    }

    /// <summary>Provides null-tolerant type-classification operations.</summary>
    /// <param name="typeSymbol">The type symbol receiving the extension operation.</param>
    extension(ITypeSymbol? typeSymbol)
    {
    /// <summary>Determines whether a type symbol represents a task return type.</summary>
    /// <returns>Whether the type symbol or a base type represents a task.</returns>
    internal bool IsTaskReturnType()
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
        while (typeSymbol is not null);

        return false;
    }

    /// <summary>Determines whether a type symbol represents an observable return type.</summary>
    /// <returns>Whether the type symbol or a base type represents an observable.</returns>
    internal bool IsObservableReturnType()
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
        while (typeSymbol is not null);

        return false;
    }

    /// <summary>Determines whether a type symbol represents the configured scheduler API.</summary>
    /// <param name="api">The ReactiveUI API family to inspect.</param>
    /// <returns>Whether the type symbol or a base type represents that scheduler API.</returns>
    internal bool IsSchedulerType(ReactiveUiApi api)
    {
        var expectedTypeName = api == ReactiveUiApi.Primitives
            ? "global::ReactiveUI.Primitives.Concurrency.ISequencer"
            : "global::System.Reactive.Concurrency.IScheduler";
        var nameFormat = SymbolDisplayFormat.FullyQualifiedFormat;
        do
        {
            var typeName = typeSymbol?.ToDisplayString(nameFormat);
            if (typeName == expectedTypeName)
            {
                return true;
            }

            typeSymbol = typeSymbol?.BaseType;
        }
        while (typeSymbol is not null);
        return false;
    }

    /// <summary>Determines whether a type symbol represents an observable Boolean value.</summary>
    /// <returns>Whether the type symbol or a base type represents an observable Boolean.</returns>
    internal bool IsObservableBoolType()
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
        while (typeSymbol is not null);

        return false;
    }

    /// <summary>Determines whether [is nullable type].</summary>
    /// <returns>
    ///   <c>true</c> if [is nullable type] [the specified type symbol]; otherwise, <c>false</c>.
    /// </returns>
    internal bool IsNullableType() => typeSymbol?.NullableAnnotation == NullableAnnotation.Annotated;

    /// <summary>Gets the type produced by a task-like generic return type.</summary>
    /// <param name="compilation">The current compilation.</param>
    /// <returns>The task result type, or <see cref="SpecialType.System_Void"/> when no result exists.</returns>
    internal ITypeSymbol GetTaskReturnType(Compilation compilation) => typeSymbol switch
    {
        INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol => namedTypeSymbol.TypeArguments[0],
        _ => compilation.GetSpecialType(SpecialType.System_Void)
    };

    }

    /// <summary>Appends the fully qualified metadata name for a given symbol to a target builder.</summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance.</param>
    /// <param name="builder">The target <see cref="ImmutableArrayBuilder{T}"/> instance.</param>
    private static void AppendFullyQualifiedMetadataName(ITypeSymbol symbol, ImmutableArrayBuilder<char> builder)
    {
        static void BuildFrom(ISymbol? current, ImmutableArrayBuilder<char> target)
        {
            if (current is INamespaceSymbol namespaceSymbol)
            {
                if (!namespaceSymbol.IsGlobalNamespace)
                {
                    if (!namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        BuildFrom(namespaceSymbol.ContainingNamespace, target);
                        target.Add('.');
                    }

                    target.AddRange(namespaceSymbol.MetadataName.AsSpan());
                }

                return;
            }

            if (current is not ITypeSymbol currentType)
            {
                return;
            }

            if (currentType.ContainingSymbol is ITypeSymbol containingType)
            {
                BuildFrom(containingType, target);
                target.Add('+');
            }
            else if (currentType.ContainingSymbol is INamespaceSymbol containingNamespace && !containingNamespace.IsGlobalNamespace)
            {
                BuildFrom(containingNamespace, target);
                target.Add('.');
            }

            target.AddRange(currentType.MetadataName.AsSpan());
        }

        BuildFrom(symbol, builder);
    }
}
