// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>A custom fallback converter that handles object-to-string conversion using ToString().</summary>
public sealed class CustomFallbackConverter : IBindingFallbackConverter
{
    /// <summary>The affinity score for fallback conversions.</summary>
    private const int FallbackAffinity = 1;

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type fromType, Type toType) =>
        toType == typeof(string) ? FallbackAffinity : 0;

    /// <inheritdoc/>
    public bool TryConvert(Type fromType, object from, Type toType, object? conversionHint, out object? result)
    {
        if (toType != typeof(string))
        {
            result = null;
            return false;
        }

        result = from.ToString();
        return result is not null;
    }
}
