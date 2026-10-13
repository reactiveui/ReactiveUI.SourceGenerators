// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="DoubleToStringTypeConverter"/> type converter.</summary>
public class DoubleToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new DoubleToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DoubleToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DoubleToString_Succeeds()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 123.456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = double.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(double.MaxValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = double.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(double.MinValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = -123.456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 42.5;

        var result = converter.TryConvert(value, ExpectedAffinity, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint CustomPrecision.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_CustomPrecision()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 0.123456789;

        var result = converter.TryConvert(value, "0.0000", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("0.0000"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint GeneralFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_GeneralFormat()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 123.456;

        var result = converter.TryConvert(value, "G", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("G"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint RoundTripFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_RoundTripFormat()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 123.456789012345;

        var result = converter.TryConvert(value, "R", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("R"));
    }

    /// <summary>Verifies TryConvert WithStringFormatHint ScientificFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_ScientificFormat()
    {
        var converter = new DoubleToStringTypeConverter();
        const double value = 12345.6789;

        var result = converter.TryConvert(value, "E3", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("E3"));
    }
}
