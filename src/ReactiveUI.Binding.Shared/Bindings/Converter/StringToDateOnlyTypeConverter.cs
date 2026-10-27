// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts a <see cref="string"/> to a <see cref="DateOnly"/> with <see cref="DateOnly.TryParse(string?, out DateOnly)"/>
/// under the current culture; a null or unparseable string fails.
/// </summary>
public sealed class StringToDateOnlyTypeConverter : BindingTypeConverter<string, DateOnly>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out DateOnly result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        return DateOnly.TryParse(from, out result);
    }
}
#endif
