// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts any value to a <see cref="bool"/> that says whether it equals the conversion hint, using <see cref="object.Equals(object, object)"/>.</summary>
/// <remarks>
/// The conversion always succeeds, and two nulls are equal. The converter is registered for the
/// (<see cref="object"/>, <see cref="bool"/>) pair, so a registry lookup finds it only for that pair.
/// </remarks>
[DebuggerDisplay("EqualityTypeConverter: {FromType.Name,nq} -> {ToType.Name,nq} by equality with the conversion hint")]
public sealed class EqualityTypeConverter : IBindingTypeConverter
{
    /// <inheritdoc/>
    public Type FromType => typeof(object);

    /// <inheritdoc/>
    public Type ToType => typeof(bool);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => 1;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        // Always return a bool result
        result = Equals(from, conversionHint);
        return true;
    }
}
