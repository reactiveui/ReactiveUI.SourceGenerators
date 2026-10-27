// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Passes a <see cref="string"/> through unchanged; a null or non-string value fails the conversion.</summary>
[DebuggerDisplay("StringConverter: string -> string identity (affinity {Affinity})")]
public sealed class StringConverter : IBindingTypeConverter
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public Type FromType => typeof(string);

    /// <inheritdoc/>
    public Type ToType => typeof(string);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        switch (from)
        {
            case null:
                {
                    result = null;
                    return false;
                }

            case string s:
                {
                    result = s;
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
