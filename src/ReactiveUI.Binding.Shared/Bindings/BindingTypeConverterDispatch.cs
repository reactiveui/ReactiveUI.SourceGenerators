// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Dispatches conversions using a type-only fast-path, avoiding reflection.</summary>
public static class BindingTypeConverterDispatch
{
    /// <summary>Attempts conversion via the converter's type-only metadata and its object shim.</summary>
    /// <param name="converter">The converter.</param>
    /// <param name="from">The source value.</param>
    /// <param name="toType">The target type requested by the caller.</param>
    /// <param name="conversionHint">Implementation-defined hint.</param>
    /// <param name="result">The converted result.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    public static bool TryConvert(
        IBindingTypeConverter converter,
        object? from,
        Type toType,
        object? conversionHint,
        out object? result)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ArgumentExceptionHelper.ThrowIfNull(toType);

        if (converter.ToType != toType)
        {
            result = null;
            return false;
        }

        if (from is null)
        {
            var fromType = converter.FromType;
            if (fromType.IsValueType && Nullable.GetUnderlyingType(fromType) is null)
            {
                result = null;
                return false;
            }

            return converter.TryConvertTyped(null, conversionHint, out result);
        }

        var runtimeType = from.GetType();
        var converterFromType = converter.FromType;

        // Exact pair match keeps dispatch predictable and avoids assignability ambiguity,
        // but allow nullable<T> converters to accept boxed T values.
        if (converterFromType != runtimeType
            && Nullable.GetUnderlyingType(converterFromType) != runtimeType)
        {
            result = null;
            return false;
        }

        return converter.TryConvertTyped(from, conversionHint, out result);
    }

    /// <summary>Attempts conversion using a fallback converter.</summary>
    /// <param name="converter">The fallback converter.</param>
    /// <param name="fromType">The source runtime type.</param>
    /// <param name="from">The source value (guaranteed non-null by caller).</param>
    /// <param name="toType">The target type.</param>
    /// <param name="conversionHint">Implementation-defined hint.</param>
    /// <param name="result">The converted result.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryConvertFallback(
        IBindingFallbackConverter converter,
        Type fromType,
        object from,
        Type toType,
        object? conversionHint,
        out object? result)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ArgumentExceptionHelper.ThrowIfNull(from);
        ArgumentExceptionHelper.ThrowIfNull(toType);

        // A converter that reports success without producing a value has broken its contract.
        // Treat that as a failed conversion rather than pushing the null on into a binding.
        if (converter.TryConvert(fromType, from, toType, conversionHint, out result) && result is not null)
        {
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>Unified dispatch method that handles both typed and fallback converters.</summary>
    /// <param name="converter">The converter (either <see cref="IBindingTypeConverter"/> or <see cref="IBindingFallbackConverter"/>).</param>
    /// <param name="fromType">The source runtime type.</param>
    /// <param name="from">The source value.</param>
    /// <param name="toType">The target type.</param>
    /// <param name="conversionHint">Implementation-defined hint.</param>
    /// <param name="result">The converted result.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method automatically dispatches to the appropriate converter type:
    /// <list type="bullet">
    /// <item><description><see cref="IBindingTypeConverter"/> - uses exact pair matching</description></item>
    /// <item><description><see cref="IBindingFallbackConverter"/> - requires non-null input</description></item>
    /// </list>
    /// </remarks>
    public static bool TryConvertAny(
        object? converter,
        Type fromType,
        object? from,
        Type toType,
        object? conversionHint,
        out object? result)
    {
        ArgumentExceptionHelper.ThrowIfNull(toType);

        switch (converter)
        {
            case null:
                {
                    result = null;
                    return false;
                }

            // Dispatch to typed converter
            case IBindingTypeConverter typedConverter:
                return TryConvert(typedConverter, from, toType, conversionHint, out result);

            // Dispatch to fallback converter (requires non-null input)
            case IBindingFallbackConverter fallbackConverter:
                {
                    if (from is not null)
                    {
                        return TryConvertFallback(
                            fallbackConverter,
                            fromType,
                            from,
                            toType,
                            conversionHint,
                            out result);
                    }

                    result = null;
                    return false;
                }

            default:
                {
                    // Unknown converter type
                    result = null;
                    return false;
                }
        }
    }
}
