// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.CodeFixers.Extensions;

/// <summary>Extension methods for the <see cref="ISymbol"/> type.</summary>
internal static class ISymbolExtensions
{
    /// <summary>Extension methods for <see cref="ISymbol"/> instances.</summary>
    /// <param name="symbol">The symbol to extend.</param>
    extension(ISymbol symbol)
    {
        /// <summary>Checks whether or not a symbol has an attribute with the specified fully qualified metadata name.</summary>
        /// <param name="name">The attribute name to look for.</param>
        /// <returns>Whether the symbol has an attribute with the specified name.</returns>
        internal bool HasAttributeWithFullyQualifiedMetadataName(string name)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.HasFullyQualifiedMetadataName(name) == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
