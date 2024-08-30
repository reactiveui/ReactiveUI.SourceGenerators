// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.Helpers;

namespace ReactiveUI.SourceGenerators.ObservableAsProperty.Models
{
    internal record ObservableMethodInfo(
    string MethodName,
    ITypeSymbol MethodReturnType,
    ITypeSymbol? ArgumentType,
    string PropertyName,
    bool IsProperty,
    EquatableArray<AttributeInfo> ForwardedPropertyAttributes)
    {
        public string GetObservableTypeText() => MethodReturnType is not INamedTypeSymbol typeSymbol
                ? string.Empty
                : typeSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
