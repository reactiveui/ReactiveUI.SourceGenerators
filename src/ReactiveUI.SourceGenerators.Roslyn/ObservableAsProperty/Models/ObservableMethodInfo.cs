// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;

namespace ReactiveUI.SourceGenerators.ObservableAsProperty.Models
{
    internal record ObservableMethodInfo(
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
        public bool IsFromPartialProperty => ObservableType.Contains("##FromPartialProperty##");

        public string PartialPropertyType => ObservableType.Replace("##FromPartialProperty##", string.Empty);

        public string GetGeneratedFieldName()
        {
            var propertyName = PropertyName;

            return $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
        }
    }
}
