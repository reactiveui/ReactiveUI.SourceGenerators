// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a nullable <see cref="double"/> to a <see cref="double"/>; a null value fails the conversion.</summary>
[DebuggerDisplay("NullableDoubleToDoubleTypeConverter: double? -> double (affinity {Affinity})")]
public sealed class NullableDoubleToDoubleTypeConverter : IBindingTypeConverter<double?, double>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(double?);

    /// <inheritdoc/>
    public Type ToType => typeof(double);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(double? from, object? conversionHint, [NotNullWhen(true)] out double result)
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
            // Handle null by returning false
            case null:
                {
                    result = null;
                    return TryConvert(null, conversionHint, out _);
                }

            // Handle double by converting through strongly-typed method
            case double value:
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
