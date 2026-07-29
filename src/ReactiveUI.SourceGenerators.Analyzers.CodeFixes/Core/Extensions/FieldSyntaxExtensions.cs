// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.CodeFixers.Extensions;

/// <summary>Extension methods for field and property symbols.</summary>
internal static class FieldSyntaxExtensions
{
    /// <summary>Extension methods for <see cref="IFieldSymbol"/> instances.</summary>
    /// <param name="fieldSymbol">The field symbol to extend.</param>
    extension(IFieldSymbol fieldSymbol)
    {
        /// <summary>Validates the containing type for a given field being annotated.</summary>
        /// <returns>Whether or not the containing type is valid.</returns>
        internal bool IsTargetTypeValid()
        {
            var isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
            var isIObservableObject = fieldSymbol.ContainingType.ImplementsFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
            var hasObservableObjectAttribute = fieldSymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

            return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
        }
    }

    /// <summary>Extension methods for <see cref="IPropertySymbol"/> instances.</summary>
    /// <param name="propertySymbol">The property symbol to extend.</param>
    extension(IPropertySymbol propertySymbol)
    {
        /// <summary>Validates the containing type for a given property being annotated.</summary>
        /// <returns>Whether or not the containing type is valid.</returns>
        internal bool IsTargetTypeValid()
        {
            var isObservableObject = propertySymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
            var isIObservableObject = propertySymbol.ContainingType.ImplementsFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
            var hasObservableObjectAttribute = propertySymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

            return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
        }
    }
}
