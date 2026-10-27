// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Emits URI formatting and relative-or-absolute URI parsing.</summary>
internal sealed class UriConversionPlugin : IConversionPlugin
{
    /// <inheritdoc/>
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        if (ConversionSymbols.IsSystemType(source, nameof(Uri)) && target.SpecialType == SpecialType.System_String)
        {
            return new("__value.ToString()", "__value != null", BindingAffinity.DefaultInternalTypeConverter);
        }

        return source.SpecialType == SpecialType.System_String && ConversionSymbols.IsSystemType(target, nameof(Uri))
            ? new("__parsed", "global::System.Uri.TryCreate(__value, global::System.UriKind.RelativeOrAbsolute, out __parsed)", BindingAffinity.DefaultInternalTypeConverter)
            {
                Preparation = "global::System.Uri __parsed;",
            }
            : null;
    }
}
