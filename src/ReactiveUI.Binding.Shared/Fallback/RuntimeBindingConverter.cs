// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Runtime conversion entry point used by generated <c>BindTo</c> bindings to coerce a source value
/// to the target property type when the two differ, or when an explicit converter or conversion hint
/// is supplied.
/// </summary>
/// <remarks>
/// An explicit converter override wins; otherwise <see cref="BindingConverters.Current"/> resolves the best
/// converter for the declared source and target types. With no converter registered for the pair, a value that is
/// already an instance of the target type passes through unchanged. When no converter can produce a value, the
/// conversion fails and the generated binding skips the assignment for that emission.
/// </remarks>
public static class RuntimeBindingConverter
{
    /// <summary>Attempts to convert <paramref name="value"/> from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/> for a generated <c>BindTo</c> assignment.</summary>
    /// <typeparam name="TFrom">The declared source value type.</typeparam>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="value">The value produced by the source observable.</param>
    /// <param name="conversionHint">An optional conversion hint forwarded to the converter.</param>
    /// <param name="converterOverride">An optional explicit converter that takes precedence over the registry.</param>
    /// <param name="result">The converted value when conversion succeeds.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    /// <remarks>The converter is resolved from <typeparamref name="TFrom"/> and <typeparamref name="TTo"/>, not from the runtime type of <paramref name="value"/>.</remarks>
    public static bool TryConvert<TFrom, TTo>(
        TFrom value,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        out TTo result)
    {
        var toType = typeof(TTo);
        var fromType = typeof(TFrom);
        var resolved = converterOverride ?? BindingConverters.Current.ResolveConverter(fromType, toType);
        if (resolved is IBindingTypeConverter<TFrom, TTo> typedConverter)
        {
            return typedConverter.TryConvert(value, conversionHint, out result!);
        }

        object? boxed = value;
        object? converted;

        if (converterOverride is not null)
        {
            if (BindingTypeConverterDispatch.TryConvert(converterOverride, boxed, toType, conversionHint, out converted)
                && converted is TTo typedOverride)
            {
                result = typedOverride;
                return true;
            }

            result = default!;
            return false;
        }

        if (resolved is null && TryPassThrough(boxed, out result))
        {
            return true;
        }

        if (BindingTypeConverterDispatch.TryConvertAny(resolved, fromType, boxed, toType, conversionHint, out converted)
            && converted is TTo typed)
        {
            result = typed;
            return true;
        }

        result = default!;
        return false;
    }

    /// <summary>Passes a value through unconverted when nothing is registered for the pair.</summary>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="value">The boxed source value.</param>
    /// <param name="result">The value as the target type when it passes through.</param>
    /// <returns>
    /// <see langword="true"/> when the value already is the target type at runtime (an object-typed
    /// <c>SelectedItem</c> holding a string, or an int bound to an int), or is a null the target can hold.
    /// </returns>
    private static bool TryPassThrough<TTo>(object? value, out TTo result)
    {
        if (value is TTo same)
        {
            result = same;
            return true;
        }

        result = default!;
        return value is null && default(TTo) is null;
    }
}
