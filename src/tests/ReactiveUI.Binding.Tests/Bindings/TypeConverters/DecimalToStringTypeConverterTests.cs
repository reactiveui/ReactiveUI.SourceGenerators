// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="DecimalToStringTypeConverter"/> type converter.</summary>
public class DecimalToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new DecimalToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DecimalToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DecimalToString_Succeeds()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 123.456M;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = decimal.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(decimal.MaxValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = decimal.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(decimal.MinValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = -123.456M;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 42.5M;

        var result = converter.TryConvert(value, ExpectedAffinity, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint CurrencyFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_CurrencyFormat()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 1234.56M;

        var result = converter.TryConvert(value, "C", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("C"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint ExponentialFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_ExponentialFormat()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 1234.5678M;

        var result = converter.TryConvert(value, "E2", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("E2"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 1234.5678M;

        var result = converter.TryConvert(value, "N2", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("N2"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint PercentFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_PercentFormat()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 0.1234M;

        var result = converter.TryConvert(value, "P2", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("P2"));
    }

    /// <summary>Verifies TryConvert Zero Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Zero_Succeeds()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal value = 0M;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("0");
    }
}
