// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Converts a banking amount to text with a currency symbol in front.</summary>
[DebuggerDisplay("CurrencyTextConverter: decimal -> currency text")]
public sealed class CurrencyTextConverter : BindingTypeConverter<decimal, string>
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int CurrencyAffinity = 10;

    /// <summary>The currency symbol written in front of the amount.</summary>
    private readonly string _symbol;

    /// <summary>Initializes a new instance of the <see cref="CurrencyTextConverter"/> class.</summary>
    /// <param name="symbol">The currency symbol written in front of the amount.</param>
    public CurrencyTextConverter(string symbol) => _symbol = symbol;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => CurrencyAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(decimal from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        result = _symbol + from.ToString("F2", CultureInfo.InvariantCulture);
        return true;
    }
}
