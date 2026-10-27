// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>Matches native type names without allocating a formatted symbol display.</summary>
internal static class NativeTypeIdentity
{
    /// <summary>Compares namespace and containing-type segments ordinally.</summary>
    /// <param name="type">The candidate type.</param>
    /// <param name="qualifiedName">The native type name using dots for nested types.</param>
    /// <returns>True when the complete type identity matches.</returns>
    internal static bool Matches(ITypeSymbol? type, string qualifiedName)
    {
        ISymbol? current = type;
        var end = qualifiedName.Length;
        while (current is not null && end > 0)
        {
            var start = qualifiedName.LastIndexOf('.', end - 1) + 1;
            var name = current.MetadataName;
            if (name.Length != end - start || string.CompareOrdinal(name, 0, qualifiedName, start, name.Length) != 0)
            {
                return false;
            }

            current = current.ContainingSymbol;
            end = start - 1;
        }

        return end < 0 && current is INamespaceSymbol { IsGlobalNamespace: true };
    }
}
