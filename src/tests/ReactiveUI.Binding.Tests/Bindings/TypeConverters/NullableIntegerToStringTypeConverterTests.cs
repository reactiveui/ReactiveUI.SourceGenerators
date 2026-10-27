// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="NullableIntegerToStringTypeConverter"/> type converter.</summary>
public class NullableIntegerToStringTypeConverterTests
{
    /// <summary>The conversion hint passed to the converter, which selects the output format.</summary>
    private const int ConversionHint = 8;

    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Integer value converted with the hexadecimal format hint.</summary>
    private const int HexInteger = 255;

    /// <summary>Integer value narrow enough for the width hint to show its zero padding.</summary>
    private const int PaddedInteger = 42;

    /// <summary>Sample integer value used for conversion round-trips.</summary>
    private const int SampleInteger = 123_456;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert IntNullableToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_IntNullableToString_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var result = converter.TryConvert(SampleInteger, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456");
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = int.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MaxValue.ToString());
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = int.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MinValue.ToString());
    }

    /// <summary>Verifies TryConvert NullValue ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var result = converter.TryConvert(PaddedInteger, ConversionHint, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00000042");
    }

    /// <summary>Verifies TryConvert with a string format hint formats correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var result = converter.TryConvert(HexInteger, "X", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("FF");
    }
}
