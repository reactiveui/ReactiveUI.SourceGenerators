// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>The conversion stage of a generated binding whose conversion the generator wrote at compile time.</summary>
/// <remarks>
/// A converter the call site names always converts. Otherwise a registered converter converts only when its
/// affinity is higher than the generated conversion's, and the generated conversion converts when none is.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GeneratedConversion
{
    /// <summary>Converts each value of a binding's source for its target.</summary>
    /// <typeparam name="TFrom">The declared source value type.</typeparam>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="source">The values to convert.</param>
    /// <param name="converterOverride">The converter the call site named, or <see langword="null"/> for none.</param>
    /// <param name="conversionHint">The hint handed to whichever conversion runs.</param>
    /// <param name="generatedAffinity">The affinity of the generated conversion.</param>
    /// <param name="isIdentity">Whether the generated conversion hands every value on unchanged.</param>
    /// <param name="convert">The generated conversion, which reports whether it produced a value.</param>
    /// <param name="assignmentFallback">
    /// Assigns a value a registered converter declined, or <see langword="null"/> when the types are not assignable.
    /// </param>
    /// <returns>The converted values, without those no conversion produced a value for.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="convert"/> is <see langword="null"/>.</exception>
    public static IObservable<TTo> Convert<TFrom, TTo>(
        IObservable<TFrom> source,
        IBindingTypeConverter? converterOverride,
        object? conversionHint,
        int generatedAffinity,
        bool isIdentity,
        Func<TFrom, object?, (bool Success, TTo Value)> convert,
        Func<TFrom, TTo>? assignmentFallback)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(convert);

        var converter = converterOverride ?? FindRegistered(typeof(TFrom), typeof(TTo), generatedAffinity);
        if (converter is not null)
        {
            // A converter the call site named is the only conversion it asked for, so a value it declines is dropped.
            var fallback = converterOverride is null ? assignmentFallback : null;
            return source.Choose(new ConverterStage<TFrom, TTo>(converter, conversionHint, fallback).Convert);
        }

        return isIdentity
            ? (IObservable<TTo>)source
            : source.Choose(new GeneratedStage<TFrom, TTo>(convert, conversionHint).Convert);
    }

    /// <summary>Finds a registered converter that outranks the generated conversion.</summary>
    /// <param name="fromType">The declared source value type.</param>
    /// <param name="toType">The target property type.</param>
    /// <param name="generatedAffinity">The affinity of the generated conversion.</param>
    /// <returns>The converter, or <see langword="null"/> when the generated conversion ranks at least as high.</returns>
    private static IBindingTypeConverter? FindRegistered(Type fromType, Type toType, int generatedAffinity)
    {
        var registered = BindingConverters.Current.TypedConverters.TryGetConverter(fromType, toType);
        return registered is not null && registered.GetAffinityForObjects() > generatedAffinity ? registered : null;
    }

    /// <summary>Runs the generated conversion with the binding's hint.</summary>
    /// <typeparam name="TFrom">The declared source value type.</typeparam>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="convert">The generated conversion.</param>
    /// <param name="conversionHint">The hint handed to the conversion.</param>
    private sealed class GeneratedStage<TFrom, TTo>(Func<TFrom, object?, (bool Success, TTo Value)> convert, object? conversionHint)
    {
        /// <summary>Converts one value.</summary>
        /// <param name="value">The source value.</param>
        /// <returns>Whether a value was produced, and the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool Success, TTo Value) Convert(TFrom value) => convert(value, conversionHint);
    }

    /// <summary>Runs a named or registered converter with the binding's hint.</summary>
    /// <typeparam name="TFrom">The declared source value type.</typeparam>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="converter">The converter that won.</param>
    /// <param name="conversionHint">The hint handed to the converter.</param>
    /// <param name="assignmentFallback">Assigns a value the converter declined, or <see langword="null"/> to drop it.</param>
    private sealed class ConverterStage<TFrom, TTo>(IBindingTypeConverter converter, object? conversionHint, Func<TFrom, TTo>? assignmentFallback)
    {
        /// <summary>The converter as its typed interface, which converts without boxing, when it implements it.</summary>
        private readonly IBindingTypeConverter<TFrom, TTo>? _typed = converter as IBindingTypeConverter<TFrom, TTo>;

        /// <summary>Converts one value.</summary>
        /// <param name="value">The source value.</param>
        /// <returns>Whether a value was produced, and the value.</returns>
        public (bool Success, TTo Value) Convert(TFrom value)
        {
            if (_typed is not null)
            {
                if (_typed.TryConvert(value, conversionHint, out var converted))
                {
                    return (true, converted!);
                }
            }
            else if (converter.TryConvertTyped(value, conversionHint, out var boxed))
            {
                return (true, (TTo)boxed!);
            }

            return assignmentFallback is null ? (false, default(TTo)!) : (true, assignmentFallback(value));
        }
    }
}
