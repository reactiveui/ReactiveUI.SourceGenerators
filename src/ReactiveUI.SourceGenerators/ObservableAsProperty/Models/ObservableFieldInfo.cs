// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Reactive.Models;

/// <summary>
/// A model with gathered info on a given field.
/// </summary>
internal sealed record ObservableFieldInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    string TypeNameWithNullabilityAnnotations,
    string FieldName,
    string PropertyName,
    string? Initializer,
    bool IsReferenceTypeOrUnconstrainedTypeParameter,
    bool IncludeMemberNotNullOnSetAccessor,
    EquatableArray<string> ForwardedAttributes,
    string AccessModifier,
    string Inheritance);
