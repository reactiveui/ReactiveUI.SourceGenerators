// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Conversion;

/// <summary>Adapts Android visibility to typed boolean bindings.</summary>
internal sealed class AndroidConversionPlugin : IConversionPlugin
{
    /// <summary>Android's view state enum, which takes no hint.</summary>
    private static readonly VisibilityPlatform Platform = new("Android.Views.ViewStates", "Gone", false, string.Empty, string.Empty);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ConversionInfo? Select(ITypeSymbol source, ITypeSymbol target, Compilation compilation) =>
        VisibilityConversion.Select(source, target, Platform, compilation);
}
