// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.BindableDerivedList.Models;

/// <summary>
/// A model with gathered info on a given field.
/// </summary>
internal sealed record BindableDerivedListInfo(
    TargetInfo TargetInfo,
    string TypeNameWithNullabilityAnnotations,
    string FieldName,
    string PropertyName,
    bool IsReferenceTypeOrUnconstrainedTypeParameter,
    bool IncludeMemberNotNullOnSetAccessor,
    EquatableArray<string> ForwardedAttributes);
