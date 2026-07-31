// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for the <see cref="ISymbol"/> type.</summary>
internal static class ISymbolExtensions
{
    /// <summary>Provides extension members for symbols.</summary>
    /// <param name="symbol">The symbol to extend.</param>
    extension(ISymbol symbol)
    {
        /// <summary>Gets the fully qualified name for this symbol.</summary>
        /// <returns>The fully qualified name for this symbol.</returns>
        internal string GetFullyQualifiedName() =>
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        /// <summary>Gets the fully qualified name for this symbol, including nullability annotations.</summary>
        /// <returns>The fully qualified name for this symbol.</returns>
        internal string GetFullyQualifiedNameWithNullabilityAnnotations() =>
            symbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));

        /// <summary>Checks whether this symbol has an attribute with the specified fully qualified metadata name.</summary>
        /// <param name="name">The attribute name to look for.</param>
        /// <returns>Whether this symbol has an attribute with the specified name.</returns>
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

        /// <summary>Checks whether this symbol has an attribute with the specified type.</summary>
        /// <param name="typeSymbol">The attribute type to look for.</param>
        /// <returns>Whether this symbol has an attribute with the specified type.</returns>
        internal bool HasAttributeWithType(ITypeSymbol? typeSymbol) =>
            typeSymbol is not null && symbol.TryGetAttributeWithType(typeSymbol, out _);

        /// <summary>Tries to get an attribute with the specified type.</summary>
        /// <param name="typeSymbol">The attribute type to look for.</param>
        /// <param name="attributeData">The resulting attribute, if it was found.</param>
        /// <returns>Whether this symbol has an attribute with the specified type.</returns>
        internal bool TryGetAttributeWithType(
            ITypeSymbol typeSymbol,
            [NotNullWhen(true)] out AttributeData? attributeData)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, typeSymbol))
                {
                    attributeData = attribute;
                    return true;
                }
            }

            attributeData = null;
            return false;
        }

        /// <summary>Tries to get an attribute with the specified fully qualified metadata name.</summary>
        /// <param name="name">The attribute name to look for.</param>
        /// <param name="attributeData">The resulting attribute, if it was found.</param>
        /// <returns>Whether this symbol has an attribute with the specified name.</returns>
        internal bool TryGetAttributeWithFullyQualifiedMetadataName(
            string name,
            [NotNullWhen(true)] out AttributeData? attributeData)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.HasFullyQualifiedMetadataName(name) == true)
                {
                    attributeData = attribute;
                    return true;
                }
            }

            attributeData = null;
            return false;
        }

        /// <summary>Calculates the effective accessibility for this symbol.</summary>
        /// <returns>The effective accessibility for this symbol.</returns>
        internal Accessibility GetEffectiveAccessibility()
        {
            if (symbol.Kind is SymbolKind.Alias or SymbolKind.TypeParameter)
            {
                return Accessibility.Private;
            }

            if (symbol.Kind == SymbolKind.Parameter)
            {
                return symbol.ContainingSymbol.GetEffectiveAccessibility();
            }

            var visibility = Accessibility.Public;
            for (var current = symbol; current is not null && current.Kind != SymbolKind.Namespace; current = current.ContainingSymbol)
            {
                var declaredAccessibility = current.DeclaredAccessibility;
                if (declaredAccessibility is Accessibility.NotApplicable or Accessibility.Private)
                {
                    return Accessibility.Private;
                }

                if (declaredAccessibility is Accessibility.Internal or Accessibility.ProtectedAndInternal)
                {
                    visibility = Accessibility.Internal;
                }
            }

            return visibility;
        }

        /// <summary>Checks whether this symbol can be accessed from a specified assembly.</summary>
        /// <param name="assembly">The assembly to check access for.</param>
        /// <returns>Whether <paramref name="assembly"/> can access this symbol.</returns>
        internal bool CanBeAccessedFrom(IAssemblySymbol assembly)
        {
            var accessibility = symbol.GetEffectiveAccessibility();
            return accessibility == Accessibility.Public
                || (accessibility == Accessibility.Internal && symbol.ContainingAssembly.GivesAccessTo(assembly));
        }

        /// <summary>Gets the string representation of the accessibility level of this symbol.</summary>
        /// <returns>The accessibility string for this symbol.</returns>
        internal string GetAccessibilityString() => symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => throw new InvalidOperationException("unknown accessibility")
        };
    }
}
