// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>The fallback converter an older to-do app registered with Splat. It shows any value as its text.</summary>
[DebuggerDisplay("LegacyObjectToStringFallbackConverter: object -> string fallback")]
public sealed class LegacyObjectToStringFallbackConverter : IBindingFallbackConverter
{
    /// <summary>The affinity of a last resort fallback converter.</summary>
    private const int FallbackAffinity = 1;

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type fromType, Type toType) =>
        toType == typeof(string) ? FallbackAffinity : 0;

    /// <inheritdoc/>
    public bool TryConvert(Type fromType, object from, Type toType, object? conversionHint, out object? result)
    {
        result = from.ToString();
        return result is not null;
    }
}
