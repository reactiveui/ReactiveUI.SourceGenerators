// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>
/// Shows an amount of money as text with a dollar sign. The conversion hint is the number format; without one the
/// converter uses two decimal places.
/// </summary>
[System.Diagnostics.DebuggerDisplay("CurrencyTextConverter: Currency text converter")]
public sealed class CurrencyTextConverter : BindingTypeConverter<decimal, string>
{
    /// <summary>The affinity that outranks the converters the library registers.</summary>
    private const int OverridingAffinity = 100;

    /// <summary>The number format used when the binding passes no hint.</summary>
    private const string DefaultFormat = "N2";

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => OverridingAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(decimal from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        string format = conversionHint as string ?? DefaultFormat;
        result = $"${from.ToString(format, CultureInfo.InvariantCulture)}";
        return true;
    }
}
