// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts values from <see cref="FromType"/> to <see cref="ToType"/> for the binding APIs.
/// Register an implementation to teach a binding how to convert between two types.
/// </summary>
public interface IBindingTypeConverter : IEnableLogger
{
    /// <summary>Gets the source type supported by this converter.</summary>
    Type FromType { get; }

    /// <summary>Gets the target type supported by this converter.</summary>
    Type ToType { get; }

    /// <summary>Returns this converter's priority among the converters registered for the same type pair.</summary>
    /// <returns>
    /// A positive value when the converter applies; zero or less excludes it. The registry picks the highest value
    /// and, on a tie, the earliest registered converter.
    /// </returns>
    int GetAffinityForObjects();

    /// <summary>Converts a boxed value.</summary>
    /// <param name="from">The source value; null is accepted only where the converter can convert null.</param>
    /// <param name="conversionHint">Implementation-defined hint, such as a format string.</param>
    /// <param name="result">The converted value; null when the conversion fails or produces a null.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    bool TryConvertTyped(object? from, object? conversionHint, out object? result);
}
