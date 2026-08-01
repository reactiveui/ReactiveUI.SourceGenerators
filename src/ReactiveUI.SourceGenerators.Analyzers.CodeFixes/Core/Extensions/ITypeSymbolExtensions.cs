// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.CodeFixers.Helpers;

namespace ReactiveUI.SourceGenerators.CodeFixers.Extensions;

/// <summary>Extension methods for the <see cref="ITypeSymbol"/> type.</summary>
internal static class ITypeSymbolExtensions
{
    /// <summary>Extension methods for <see cref="ITypeSymbol"/> instances.</summary>
    /// <param name="typeSymbol">The type symbol to extend.</param>
    extension(ITypeSymbol typeSymbol)
    {
        /// <summary>Checks whether a type symbol inherits from a specified type.</summary>
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

        /// <summary>Checks whether a type symbol implements a specified interface.</summary>
        /// <param name="name">The full name of the interface to check for implementation.</param>
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

        /// <summary>Checks whether a type symbol has or inherits a specified attribute.</summary>
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

        /// <summary>Checks whether a type symbol has a specified fully qualified metadata name.</summary>
        /// <param name="name">The full name to check.</param>
        /// <returns>Whether the type symbol has a full name equal to <paramref name="name"/>.</returns>
        internal bool HasFullyQualifiedMetadataName(string name)
        {
            using var builder = ImmutableArrayBuilder<char>.Rent();

            AppendFullyQualifiedMetadataName(typeSymbol, builder);

            return builder.WrittenSpan.StartsWith(name.AsSpan());
        }
    }

    /// <summary>Appends a symbol's fully qualified metadata name to a target builder.</summary>
    /// <param name="symbol">The symbol whose metadata name will be appended.</param>
    /// <param name="builder">The target builder.</param>
    private static void AppendFullyQualifiedMetadataName(ITypeSymbol symbol, ImmutableArrayBuilder<char> builder)
    {
        static void BuildFrom(ISymbol? current, ImmutableArrayBuilder<char> target)
        {
            switch (current)
            {
                case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    {
                        BuildFrom(current.ContainingNamespace, target);
                        target.Add('.');
                        target.AddRange(current.MetadataName.AsSpan());
                        break;
                    }

                case INamespaceSymbol { IsGlobalNamespace: false }:
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol } when namespaceSymbol.IsGlobalNamespace:
                    {
                        target.AddRange(current.MetadataName.AsSpan());
                        break;
                    }

                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                    {
                        BuildFrom(namespaceSymbol, target);
                        target.Add('.');
                        target.AddRange(current.MetadataName.AsSpan());
                        break;
                    }

                case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                    {
                        BuildFrom(typeSymbol, target);
                        target.Add('+');
                        target.AddRange(current.MetadataName.AsSpan());
                        break;
                    }
            }
        }

        BuildFrom(symbol, builder);
    }
}
