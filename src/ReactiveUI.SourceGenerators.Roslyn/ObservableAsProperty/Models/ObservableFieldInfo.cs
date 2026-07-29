// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>A model with gathered info on a given field.</summary>
/// <param name="TargetInfo">The target type that owns the generated property.</param>
/// <param name="TypeNameWithNullabilityAnnotations">The property's fully qualified type name.</param>
/// <param name="FieldName">The source field name.</param>
/// <param name="PropertyName">The generated property name.</param>
/// <param name="Initializer">The source field initializer, when present.</param>
/// <param name="IsReferenceTypeOrUnconstrainedTypeParameter">Whether the field type permits null.</param>
/// <param name="IncludeMemberNotNullOnSetAccessor">Whether a member-not-null annotation is required.</param>
/// <param name="ForwardedAttributes">Attributes copied to the generated property.</param>
/// <param name="IsReadOnly">The generated property's read-only modifier.</param>
/// <param name="AccessModifier">The generated property's accessibility.</param>
/// <param name="Inheritance">The generated property's inheritance modifier.</param>
internal sealed record ObservableFieldInfo(
    TargetInfo TargetInfo,
    string TypeNameWithNullabilityAnnotations,
    string FieldName,
    string PropertyName,
    string? Initializer,
    bool IsReferenceTypeOrUnconstrainedTypeParameter,
    bool IncludeMemberNotNullOnSetAccessor,
    EquatableArray<string> ForwardedAttributes,
    string IsReadOnly,
    string AccessModifier,
    string Inheritance);
