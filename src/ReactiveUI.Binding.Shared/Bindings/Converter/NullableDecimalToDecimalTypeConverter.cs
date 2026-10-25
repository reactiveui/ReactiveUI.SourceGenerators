// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a nullable <see cref="decimal"/> to a <see cref="decimal"/>; a null value fails the conversion.</summary>
[DebuggerDisplay("NullableDecimalToDecimalTypeConverter: decimal? -> decimal (affinity {Affinity})")]
public sealed class NullableDecimalToDecimalTypeConverter : IBindingTypeConverter<decimal?, decimal>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(decimal?);

    /// <inheritdoc/>
    public Type ToType => typeof(decimal);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(decimal? from, object? conversionHint, [NotNullWhen(true)] out decimal result)
    {
        if (from is null)
        {
            result = default;
            return false;
        }

        result = from.Value;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        switch (from)
        {
            case null:
                {
                    result = null;
                    return TryConvert(null, conversionHint, out _);
                }

            case decimal value:
                {
                    result = value;
                    return true;
                }

            default:
                {
                    result = null;
                    return false;
                }
        }
    }
}
