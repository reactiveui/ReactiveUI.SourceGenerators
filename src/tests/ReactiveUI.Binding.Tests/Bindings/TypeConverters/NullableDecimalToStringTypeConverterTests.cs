// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="NullableDecimalToStringTypeConverter"/> type converter.</summary>
public class NullableDecimalToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Decimal value converted with a width hint that pads it to two decimal places.</summary>
    private const decimal FormattedDecimal = 42.5M;

    /// <summary>Negative decimal value used to check the sign survives conversion.</summary>
    private const decimal NegativeDecimal = -123.456M;

    /// <summary>Sample decimal value used for conversion round-trips.</summary>
    private const decimal SampleDecimal = 123.456M;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DecimalNullableToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DecimalNullableToString_Succeeds()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var result = converter.TryConvert(SampleDecimal, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(SampleDecimal.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        decimal? value = decimal.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(decimal.MaxValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        decimal? value = decimal.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(decimal.MinValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var result = converter.TryConvert(NegativeDecimal, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(NegativeDecimal.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert NullValue ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var result = converter.TryConvert(FormattedDecimal, ExpectedAffinity, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(FormattedDecimal.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert Zero Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Zero_Succeeds()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var result = converter.TryConvert(0M, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("0");
    }

    /// <summary>Verifies TryConvert with a string format hint formats correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        decimal? value = 42.5M;

        var result = converter.TryConvert(value, "F4", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.Value.ToString("F4"));
    }
}
