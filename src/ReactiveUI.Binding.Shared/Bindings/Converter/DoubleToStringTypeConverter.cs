// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts a <see cref="double"/> to a <see cref="string"/> using the current culture. An <see cref="int"/> hint gives the
/// number of decimal places (the <c>F</c> format) and a <see cref="string"/> hint gives the format string; a malformed
/// format throws <see cref="FormatException"/>.
/// </summary>
public sealed class DoubleToStringTypeConverter : BindingTypeConverter<double, string>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(double from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        switch (conversionHint)
        {
            case int decimalPlaces:
                {
                    result = from.ToString($"F{decimalPlaces}");
                    return true;
                }

            case string format:
                {
                    result = from.ToString(format);
                    return true;
                }

            default:
                {
                    result = from.ToString(System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
        }
    }
}
