// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting integers to strings.</summary>
public class IntegerToStringTypeConverterTests
{
    /// <summary>The conversion hint passed to the converter, which selects the output format.</summary>
    private const int ConversionHint = 8;

    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new IntegerToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert IntToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_IntToString_Succeeds()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 123_456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456");
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = int.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MaxValue.ToString());
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = int.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MinValue.ToString());
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = -123_456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("-123456");
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 42;

        var result = converter.TryConvert(value, ConversionHint, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00000042");
    }

    /// <summary>Verifies TryConvert WithStringFormatHint CustomFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_CustomFormat()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 42;

        var result = converter.TryConvert(value, "000", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("042");
    }

    /// <summary>Verifies TryConvert WithStringFormatHint HexFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_HexFormat()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 255;

        var result = converter.TryConvert(value, "X", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("FF");
    }

    /// <summary>Verifies TryConvert WithStringFormatHint HexFormatLowercase.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_HexFormatLowercase()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 255;

        var result = converter.TryConvert(value, "x8", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("000000ff");
    }

    /// <summary>Verifies TryConvert WithStringFormatHint NumberFormat.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_NumberFormat()
    {
        var converter = new IntegerToStringTypeConverter();
        const int value = 1_234_567;

        var result = converter.TryConvert(value, "N0", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("N0"));
    }
}
