// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Contains the information needed to generate an <c>IViewFor</c> implementation.</summary>
/// <param name="TargetInfo">The annotated target type.</param>
/// <param name="ViewModelTypeName">The fully qualified view model type name.</param>
/// <param name="BaseType">The supported UI framework base type.</param>
internal sealed record IViewForInfo(
    TargetInfo TargetInfo,
    string ViewModelTypeName,
    IViewForBaseType BaseType);
