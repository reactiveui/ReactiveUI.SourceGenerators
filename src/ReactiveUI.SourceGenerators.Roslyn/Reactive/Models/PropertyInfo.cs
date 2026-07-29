// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Contains the metadata needed to generate a reactive property.</summary>
/// <param name="TargetInfo">The enclosing type metadata.</param>
/// <param name="TypeNameWithNullabilityAnnotations">The property's fully-qualified type name.</param>
/// <param name="FieldName">The backing field name.</param>
/// <param name="PropertyName">The generated property name.</param>
/// <param name="IsReferenceTypeOrUnconstrainedTypeParameter">Whether the property type is nullable-reference compatible.</param>
/// <param name="IncludeMemberNotNullOnSetAccessor">Whether the setter should have a <c>MemberNotNull</c> attribute.</param>
/// <param name="ForwardedAttributes">The attributes copied to the generated property.</param>
/// <param name="SetAccessModifier">The generated setter's access modifier.</param>
/// <param name="Inheritance">The generated property's inheritance modifier.</param>
/// <param name="UseRequired">The generated property's required modifier.</param>
/// <param name="IsProperty">Whether the source member is a partial property.</param>
/// <param name="PropertyAccessModifier">The generated property's access modifier.</param>
/// <param name="AlsoNotify">The additional property names to notify after assignment.</param>
/// <param name="XmlComment">The XML documentation to copy to the generated property.</param>
internal sealed record PropertyInfo(
    TargetInfo TargetInfo,
    string TypeNameWithNullabilityAnnotations,
    string FieldName,
    string PropertyName,
    bool IsReferenceTypeOrUnconstrainedTypeParameter,
    bool IncludeMemberNotNullOnSetAccessor,
    EquatableArray<string> ForwardedAttributes,
    string SetAccessModifier,
    string Inheritance,
    string UseRequired,
    bool IsProperty,
    string PropertyAccessModifier,
    EquatableArray<string> AlsoNotify,
    string? XmlComment);
