// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Base class for a converter between one type pair; it supplies <see cref="FromType"/>, <see cref="ToType"/> and
/// an object-based <see cref="TryConvertTyped(object?, object?, out object?)"/> over the typed <c>TryConvert</c>.
/// </summary>
/// <typeparam name="TFrom">The source type to convert from.</typeparam>
/// <typeparam name="TTo">The target type to convert to.</typeparam>
[DebuggerDisplay("BindingTypeConverter: {FromType.Name,nq} -> {ToType.Name,nq} converter")]
public abstract class BindingTypeConverter<TFrom, TTo> : IBindingTypeConverter<TFrom, TTo>
{
    /// <inheritdoc/>
    public Type FromType => typeof(TFrom);

    /// <inheritdoc/>
    public Type ToType => typeof(TTo);

    /// <summary>Returns this converter's priority among the converters registered for the same type pair.</summary>
    /// <returns>
    /// A positive value when the converter applies; zero or less excludes it. The highest value wins and the
    /// earliest registered converter wins a tie. The built-in converters return 2, and
    /// <see cref="EqualityTypeConverter"/> returns 1, so a larger value outranks them.
    /// </returns>
    public abstract int GetAffinityForObjects();

    /// <inheritdoc/>
    public abstract bool TryConvert(TFrom? from, object? conversionHint, [MaybeNullWhen(true)] out TTo? result);

    /// <summary>Converts a boxed value by casting it to <typeparamref name="TFrom"/> and calling <c>TryConvert</c>.</summary>
    /// <param name="from">The source value. A null is passed on as <c>default</c> when <typeparamref name="TFrom"/> can hold null, and fails when it cannot.</param>
    /// <param name="conversionHint">Implementation-defined hint, passed to <c>TryConvert</c> unchanged.</param>
    /// <param name="result">The converted value, or null when the conversion fails or produces a null.</param>
    /// <returns><see langword="false"/> when <paramref name="from"/> is not a <typeparamref name="TFrom"/> or <c>TryConvert</c> fails; otherwise <see langword="true"/>.</returns>
    public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
    {
        // Allow null inputs for converters whose source type can represent null, and
        // permit null outputs when the target type is nullable/reference.
        TTo? typedResult;
        if (from is null)
        {
            if (default(TFrom) is not null)
            {
                result = null;
                return false;
            }

            if (!TryConvert(default, conversionHint, out typedResult))
            {
                result = null;
                return false;
            }

            result = typedResult;
            return true;
        }

        if (from is not TFrom castFrom)
        {
            result = null;
            return false;
        }

        if (!TryConvert(castFrom, conversionHint, out typedResult))
        {
            result = null;
            return false;
        }

        result = typedResult;
        return true;
    }
}
