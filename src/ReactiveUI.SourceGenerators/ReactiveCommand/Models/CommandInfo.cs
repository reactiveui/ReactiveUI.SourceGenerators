// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.Input.Models;

internal record CommandInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    string MethodName,
    string MethodReturnType,
    string? ArgumentType,
    bool IsTask,
    bool IsReturnTypeVoid,
    bool IsObservable,
    string? CanExecuteObservableName,
    CanExecuteTypeInfo? CanExecuteTypeInfo,
    EquatableArray<AttributeInfo> ForwardedPropertyAttributes)
{
    private const string UnitTypeName = "global::System.Reactive.Unit";

    public string GetOutputTypeText() => IsReturnTypeVoid
            ? UnitTypeName
            : MethodReturnType;

    public string GetInputTypeText() => string.IsNullOrWhiteSpace(ArgumentType)
            ? UnitTypeName
            : ArgumentType!;
}
