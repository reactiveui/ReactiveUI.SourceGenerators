// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Captures the metadata needed to generate an observable-backed property.</summary>
/// <param name="TargetInfo">The target type that owns the generated property.</param>
/// <param name="MethodName">The source method or property name.</param>
/// <param name="MethodReturnType">The source member's return type.</param>
/// <param name="ArgumentType">The source method's argument type, when present.</param>
/// <param name="PropertyName">The generated property name.</param>
/// <param name="ObservableType">The observable's fully qualified type name.</param>
/// <param name="IsNullableType">Whether the observable element type permits null.</param>
/// <param name="IsProperty">Whether the source member is a property.</param>
/// <param name="ForwardedPropertyAttributes">Attributes copied to the generated property.</param>
/// <param name="IsReadOnly">The generated property's read-only modifier.</param>
/// <param name="AccessModifier">The generated property's accessibility.</param>
/// <param name="InitialValue">The generated property's initial value, when present.</param>
internal sealed record ObservableMethodInfo(
    TargetInfo TargetInfo,
    string MethodName,
    string MethodReturnType,
    string? ArgumentType,
    string PropertyName,
    string ObservableType,
    bool IsNullableType,
    bool IsProperty,
    EquatableArray<string> ForwardedPropertyAttributes,
    string IsReadOnly,
    string AccessModifier,
    string? InitialValue)
{
    /// <summary>Gets whether this model originated from a partial property.</summary>
    internal bool IsFromPartialProperty => ObservableType.IndexOf("##FromPartialProperty##", StringComparison.Ordinal) >= 0;

    /// <summary>Gets the observable type with its partial-property marker removed.</summary>
    internal string PartialPropertyType => ObservableType.Replace("##FromPartialProperty##", string.Empty);

    /// <summary>Gets the field name generated for this property.</summary>
    /// <returns>The generated backing field name.</returns>
    internal string GetGeneratedFieldName()
    {
        var propertyName = PropertyName;

        return $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
    }
}
