// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes a <c>ReactiveProperty&lt;T&gt;</c> property and its DataAnnotations validation attributes.</summary>
/// <param name="PropertyName">The property name on the declaring class.</param>
/// <param name="DisplayName">The display name from <c>[Display(Name = ...)]</c>, or the property name.</param>
/// <param name="ValidationAttributes">The validation attributes applied to the property.</param>
internal sealed record ValidatedPropertyInfo(
    string PropertyName,
    string DisplayName,
    EquatableArray<AttributeInfo> ValidationAttributes);
