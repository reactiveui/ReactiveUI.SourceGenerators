// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a <see cref="byte"/> to a nullable <see cref="byte"/>; the conversion always succeeds.</summary>
[DebuggerDisplay("ByteToNullableByteTypeConverter: byte -> byte? (affinity {Affinity})")]
public sealed class ByteToNullableByteTypeConverter : IBindingTypeConverter<byte, byte?>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(byte);

    /// <inheritdoc/>
    public Type ToType => typeof(byte?);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(byte from, object? conversionHint, out byte? result)
    {
        result = from;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is byte value)
        {
            result = (byte?)value;
            return true;
        }

        result = null;
        return false;
    }
}
