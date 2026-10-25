// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a <see cref="long"/> to a nullable <see cref="long"/>; the conversion always succeeds.</summary>
[DebuggerDisplay("LongToNullableLongTypeConverter: long -> long? (affinity {Affinity})")]
public sealed class LongToNullableLongTypeConverter : IBindingTypeConverter<long, long?>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(long);

    /// <inheritdoc/>
    public Type ToType => typeof(long?);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(long from, object? conversionHint, out long? result)
    {
        result = from;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is long value)
        {
            result = (long?)value;
            return true;
        }

        result = null;
        return false;
    }
}
