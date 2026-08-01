// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Extensions;

/// <summary>Extension methods for ReactiveUI field, property, and method symbols.</summary>
internal static class FieldSyntaxExtensions
{
    /// <summary>The number of characters in a member-style backing-field prefix.</summary>
    private const int FieldPrefixLength = 2;

    /// <summary>The metadata name of ReactiveUI's observable object base type.</summary>
    private const string ReactiveObjectTypeName = "ReactiveUI.ReactiveObject";

    /// <summary>The metadata name of ReactiveUI's observable object interface.</summary>
    private const string ReactiveObjectInterfaceTypeName = "ReactiveUI.IReactiveObject";

    /// <summary>Provides operations for annotated field symbols.</summary>
    /// <param name="fieldSymbol">The field symbol receiving the extension operation.</param>
    extension(IFieldSymbol fieldSymbol)
    {
        /// <summary>Gets the generated property name for an input field.</summary>
        /// <returns>The generated property name.</returns>
        internal string GetGeneratedPropertyName()
        {
            var propertyName = fieldSymbol.Name;

            if (propertyName.StartsWith("m_", System.StringComparison.Ordinal))
            {
                propertyName = propertyName[FieldPrefixLength..];
            }
            else if (propertyName.StartsWith("_", System.StringComparison.Ordinal))
            {
                propertyName = propertyName.TrimStart('_');
            }

            return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
        }

        /// <summary>Gets nullability information for a generated property.</summary>
        /// <param name="semanticModel">The semantic model for the current run.</param>
        /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
        /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
        internal void GetNullabilityInfo(
            SemanticModel semanticModel,
            out bool isReferenceTypeOrUnconstraindTypeParameter,
            out bool includeMemberNotNullOnSetAccessor) =>
            GetNullabilityInfo(fieldSymbol.Type, semanticModel, out isReferenceTypeOrUnconstraindTypeParameter, out includeMemberNotNullOnSetAccessor);

        /// <summary>Validates the containing type for a given field being annotated.</summary>
        /// <returns>Whether the containing type is valid.</returns>
        internal bool IsTargetTypeValid() => IsTargetTypeValid(fieldSymbol.ContainingType);
    }

    /// <summary>Provides operations for annotated method symbols.</summary>
    /// <param name="methodSymbol">The method symbol receiving the extension operation.</param>
    extension(IMethodSymbol methodSymbol)
    {
        /// <summary>Validates the containing type for a given method being annotated.</summary>
        /// <returns>Whether the containing type is valid.</returns>
        internal bool IsTargetTypeValid() => IsTargetTypeValid(methodSymbol.ContainingType);
    }

    /// <summary>Provides operations for annotated property symbols.</summary>
    /// <param name="propertySymbol">The property symbol receiving the extension operation.</param>
    extension(IPropertySymbol propertySymbol)
    {
        /// <summary>Gets the generated backing-field name for an input property.</summary>
        /// <returns>The generated backing-field name.</returns>
        internal string GetGeneratedFieldName()
        {
            var propertyName = propertySymbol.Name;

            return $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
        }

        /// <summary>Gets nullability information for a generated property.</summary>
        /// <param name="semanticModel">The semantic model for the current run.</param>
        /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
        /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
        internal void GetNullabilityInfo(
            SemanticModel semanticModel,
            out bool isReferenceTypeOrUnconstraindTypeParameter,
            out bool includeMemberNotNullOnSetAccessor) =>
            GetNullabilityInfo(propertySymbol.Type, semanticModel, out isReferenceTypeOrUnconstraindTypeParameter, out includeMemberNotNullOnSetAccessor);

        /// <summary>Validates the containing type for a given property being annotated.</summary>
        /// <returns>Whether the containing type is valid.</returns>
        internal bool IsTargetTypeValid() => IsTargetTypeValid(propertySymbol.ContainingType);
    }

    /// <summary>Determines whether a containing type supports ReactiveUI-generated members.</summary>
    /// <param name="containingType">The type that owns the annotated member.</param>
    /// <returns>Whether the containing type is a supported ReactiveUI observable type.</returns>
    private static bool IsTargetTypeValid(INamedTypeSymbol containingType)
    {
        var isObservableObject = containingType.InheritsFromFullyQualifiedMetadataName(ReactiveObjectTypeName);
        var isIObservableObject = containingType.ImplementsFullyQualifiedMetadataName(ReactiveObjectInterfaceTypeName);
        var hasObservableObjectAttribute = containingType.HasOrInheritsAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveObjectAttributeType);

        return isIObservableObject || isObservableObject || hasObservableObjectAttribute;
    }

    /// <summary>Gets nullability information for a property generated from a type.</summary>
    /// <param name="typeSymbol">The member type to evaluate.</param>
    /// <param name="semanticModel">The semantic model for the current run.</param>
    /// <param name="isReferenceTypeOrUnconstraindTypeParameter">Whether the property type supports nullability.</param>
    /// <param name="includeMemberNotNullOnSetAccessor">Whether MemberNotNullAttribute should be used on the setter.</param>
    private static void GetNullabilityInfo(
        ITypeSymbol typeSymbol,
        SemanticModel semanticModel,
        out bool isReferenceTypeOrUnconstraindTypeParameter,
        out bool includeMemberNotNullOnSetAccessor)
    {
        isReferenceTypeOrUnconstraindTypeParameter = !typeSymbol.IsValueType;
        includeMemberNotNullOnSetAccessor =
            isReferenceTypeOrUnconstraindTypeParameter
            && typeSymbol.NullableAnnotation != NullableAnnotation.Annotated
            && semanticModel.Compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute");
    }
}
