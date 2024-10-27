﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>
/// Helper methods for working with symbols.
/// </summary>
internal static class SymbolHelpers
{
    /// <summary>
    /// Default display format for symbols, omitting the global namespace and including nullable reference type modifiers.
    /// </summary>
    public static readonly SymbolDisplayFormat DefaultDisplay =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}
