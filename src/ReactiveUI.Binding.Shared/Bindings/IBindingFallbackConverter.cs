// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts runtime type pairs that no typed converter covers. The converter service asks the fallback
/// converters only when no typed converter with a positive affinity is registered for the pair.
/// </summary>
/// <remarks>
/// Affinity only ranks fallback converters against each other, and the earliest registered converter wins a tie.
/// The library registers no fallback converters of its own.
/// </remarks>
public interface IBindingFallbackConverter : IEnableLogger
{
    /// <summary>Calculates affinity for the specified runtime type pair.</summary>
    /// <param name="fromType">The runtime source type.</param>
    /// <param name="toType">The target type.</param>
    /// <returns>
    /// A positive value when the converter handles the pair, where a higher value wins over other fallback converters;
    /// zero or less when it does not.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method MUST be:
    /// <list type="bullet">
    /// <item><description>Pure (no side effects)</description></item>
    /// <item><description>Fast (cache any expensive metadata)</description></item>
    /// <item><description>Safe (no exceptions, no user code execution)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is invoked during converter selection and may be called frequently.
    /// Results should be cached internally where appropriate.
    /// </para>
    /// </remarks>
    int GetAffinityForObjects(
        Type fromType,
        Type toType);

    /// <summary>Attempts to convert the value to the target type.</summary>
    /// <param name="fromType">The runtime source type (guaranteed non-null).</param>
    /// <param name="from">The value to convert (guaranteed non-null).</param>
    /// <param name="toType">The target type (guaranteed non-null).</param>
    /// <param name="conversionHint">Implementation-defined conversion hint (e.g., format string, culture).</param>
    /// <param name="result">
    /// The converted value. Implementations must produce a non-null value when returning
    /// <see langword="true"/>.
    /// </param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// Returning <see langword="true"/> obliges an implementation to produce a non-null
    /// <paramref name="result"/>, but that obligation is not expressed as a nullability annotation:
    /// implementations come from outside the library and nothing enforces it. The dispatch layer
    /// therefore checks, and reports a null result as a failed conversion rather than letting it
    /// reach a binding.
    /// </para>
    /// <para>
    /// Null input handling is performed by the dispatch layer. This method will never receive
    /// null as the <paramref name="from"/> parameter.
    /// </para>
    /// </remarks>
    bool TryConvert(
        Type fromType,
        object from,
        Type toType,
        object? conversionHint,
        out object? result);
}
