// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>Helper methods for working with symbols.</summary>
internal static class SymbolHelpers
{
    /// <summary>Default display format for symbols, omitting the global namespace and including nullable reference type modifiers.</summary>
    internal static readonly SymbolDisplayFormat DefaultDisplay =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}
