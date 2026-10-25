// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts an <see cref="int"/> to a nullable <see cref="int"/>; the conversion always succeeds.</summary>
[DebuggerDisplay("IntegerToNullableIntegerTypeConverter: int -> int? (affinity {Affinity})")]
public sealed class IntegerToNullableIntegerTypeConverter : IBindingTypeConverter<int, int?>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(int);

    /// <inheritdoc/>
    public Type ToType => typeof(int?);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvert(int from, object? conversionHint, out int? result)
    {
        result = from;
        return true;
    }

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        if (from is int value)
        {
            result = (int?)value;
            return true;
        }

        result = null;
        return false;
    }
}
