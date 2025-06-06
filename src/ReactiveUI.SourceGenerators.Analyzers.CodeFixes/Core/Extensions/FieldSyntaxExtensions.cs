﻿// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

internal static class FieldSyntaxExtensions
{
    /// <summary>
    /// Validates the containing type for a given field being annotated.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>Whether or not the containing type for <paramref name="fieldSymbol"/> is valid.</returns>
    internal static bool IsTargetTypeValid(this IFieldSymbol fieldSymbol)
    {
        var isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
        var isIObservableObject = fieldSymbol.ContainingType.ImplementsFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
        var hasObservableObjectAttribute = fieldSymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

        return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
    }

    /// <summary>
    /// Validates the containing type for a given field being annotated.
    /// </summary>
    /// <param name="propertySymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>Whether or not the containing type for <paramref name="propertySymbol"/> is valid.</returns>
    internal static bool IsTargetTypeValid(this IPropertySymbol propertySymbol)
    {
        var isObservableObject = propertySymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
        var isIObservableObject = propertySymbol.ContainingType.ImplementsFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
        var hasObservableObjectAttribute = propertySymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

        return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
    }
}
