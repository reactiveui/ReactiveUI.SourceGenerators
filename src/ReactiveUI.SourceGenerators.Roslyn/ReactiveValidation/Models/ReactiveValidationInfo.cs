// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Describes compile-time DataAnnotations validation wiring for a class marked with <c>[ReactiveValidation]</c>.</summary>
/// <param name="TargetInfo">The target type metadata.</param>
/// <param name="Properties">The validated <c>ReactiveProperty&lt;T&gt;</c> properties on the class.</param>
internal sealed record ReactiveValidationInfo(
    TargetInfo TargetInfo,
    EquatableArray<ValidatedPropertyInfo> Properties);
