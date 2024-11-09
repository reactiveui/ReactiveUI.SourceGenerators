// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.ObservableAsProperty.Models
{
    internal record ObservableMethodInfo(
    string FileHintName,
    string TargetName,
    string TargetNamespace,
    string TargetNamespaceWithNamespace,
    string TargetVisibility,
    string TargetType,
    string MethodName,
    string MethodReturnType,
    string? ArgumentType,
    string PropertyName,
    string ObservableType,
    bool IsNullableType,
    bool IsProperty,
    EquatableArray<string> ForwardedPropertyAttributes)
    {
        public string GetGeneratedFieldName()
        {
            var propertyName = PropertyName;

            return $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
        }
    }
}
