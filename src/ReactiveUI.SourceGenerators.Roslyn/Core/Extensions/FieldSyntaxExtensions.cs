// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Extensions;

internal static class FieldSyntaxExtensions
{
    /// <summary>
    /// Get the generated property name for an input field.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>The generated property name for <paramref name="fieldSymbol"/>.</returns>
    internal static string GetGeneratedPropertyName(this IFieldSymbol fieldSymbol)
    {
        var propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("m_"))
        {
            propertyName = propertyName.Substring(2);
        }
        else if (propertyName.StartsWith("_"))
        {
            propertyName = propertyName.TrimStart('_');
        }

        return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
    }

    internal static string GetGeneratedFieldName(this IPropertySymbol propertySymbol)
    {
        var propertyName = propertySymbol.Name;

        return $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
    }

    /// <summary>
    /// Gets the nullability info on the generated property.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
    /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
    /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
    internal static void GetNullabilityInfo(
        this IFieldSymbol fieldSymbol,
        SemanticModel semanticModel,
        out bool isReferenceTypeOrUnconstraindTypeParameter,
        out bool includeMemberNotNullOnSetAccessor)
    {
        // We're using IsValueType here and not IsReferenceType to also cover unconstrained type parameter cases.
        // This will cover both reference types as well T when the constraints are not struct or unmanaged.
        // If this is true, it means the field storage can potentially be in a null state (even if not annotated).
        isReferenceTypeOrUnconstraindTypeParameter = !fieldSymbol.Type.IsValueType;

        // This is used to avoid nullability warnings when setting the property from a constructor, in case the field
        // was marked as not nullable. Nullability annotations are assumed to always be enabled to make the logic simpler.
        // Consider this example:
        //
        // partial class MyViewModel : ReactiveObject
        // {
        //    public MyViewModel()
        //    {
        //        Name = "Bob";
        //    }
        //
        //    [Reactive]
        //    private string _name;
        // }
        //
        // The [MemberNotNull] attribute is needed on the setter for the generated Name property so that when Name
        // is set, the compiler can determine that the name backing field is also being set (to a non null value).
        // Of course, this can only be the case if the field type is also of a type that could be in a null state.
        includeMemberNotNullOnSetAccessor =
            isReferenceTypeOrUnconstraindTypeParameter &&
            fieldSymbol.Type.NullableAnnotation != NullableAnnotation.Annotated &&
            semanticModel.Compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute");
    }

    internal static void GetNullabilityInfo(
        this IPropertySymbol propertySymbol,
        SemanticModel semanticModel,
        out bool isReferenceTypeOrUnconstraindTypeParameter,
        out bool includeMemberNotNullOnSetAccessor)
    {
        // We're using IsValueType here and not IsReferenceType to also cover unconstrained type parameter cases.
        // This will cover both reference types as well T when the constraints are not struct or unmanaged.
        // If this is true, it means the field storage can potentially be in a null state (even if not annotated).
        isReferenceTypeOrUnconstraindTypeParameter = !propertySymbol.Type.IsValueType;

        // This is used to avoid nullability warnings when setting the property from a constructor, in case the field
        // was marked as not nullable. Nullability annotations are assumed to always be enabled to make the logic simpler.
        // Consider this example:
        //
        // partial class MyViewModel : ReactiveObject
        // {
        //    public MyViewModel()
        //    {
        //        Name = "Bob";
        //    }
        //
        //    [Reactive]
        //    private string _name;
        // }
        //
        // The [MemberNotNull] attribute is needed on the setter for the generated Name property so that when Name
        // is set, the compiler can determine that the name backing field is also being set (to a non null value).
        // Of course, this can only be the case if the field type is also of a type that could be in a null state.
        includeMemberNotNullOnSetAccessor =
            isReferenceTypeOrUnconstraindTypeParameter &&
            propertySymbol.Type.NullableAnnotation != NullableAnnotation.Annotated &&
            semanticModel.Compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute");
    }

    /// <summary>
    /// Validates the containing type for a given field being annotated.
    /// </summary>
    /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
    /// <returns>Whether or not the containing type for <paramref name="fieldSymbol"/> is valid.</returns>
    internal static bool IsTargetTypeValid(this IFieldSymbol fieldSymbol)
    {
        var isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.ReactiveObject");
        var isIObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
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
        var isIObservableObject = propertySymbol.ContainingType.InheritsFromFullyQualifiedMetadataName("ReactiveUI.IReactiveObject");
        var hasObservableObjectAttribute = propertySymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName("ReactiveUI.SourceGenerators.ReactiveObjectAttribute");

        return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
    }
}
