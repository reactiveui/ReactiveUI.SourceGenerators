// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a nullable <see cref="float"/> to a <see cref="float"/>; a null value fails the conversion.</summary>
[DebuggerDisplay("NullableSingleToSingleTypeConverter: float? -> float (affinity {Affinity})")]
public sealed class NullableSingleToSingleTypeConverter : IBindingTypeConverter<float?, float>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(float?);

    /// <inheritdoc/>
    public Type ToType => typeof(float);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(float? from, object? conversionHint, [NotNullWhen(true)] out float result)
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

            case float value:
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
