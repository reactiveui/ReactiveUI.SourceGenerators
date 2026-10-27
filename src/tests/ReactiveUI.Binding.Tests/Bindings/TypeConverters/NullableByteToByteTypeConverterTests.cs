// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable byte to byte.</summary>
public class NullableByteToByteTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Sample byte value used for conversion round-trips.</summary>
    private const byte SampleByte = 42;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableByteToByteTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert WithValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithValue_Succeeds()
    {
        var converter = new NullableByteToByteTypeConverter();
        var result = converter.TryConvert(SampleByte, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(SampleByte);
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableByteToByteTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies FromType ReturnsByteNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsByteNullable()
    {
        var converter = new NullableByteToByteTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(byte?));
    }

    /// <summary>Verifies ToType ReturnsByte.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsByte()
    {
        var converter = new NullableByteToByteTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(byte));
    }

    /// <summary>Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new NullableByteToByteTypeConverter();
        var success = converter.TryConvertTyped(SampleByte, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(SampleByte);
    }

    /// <summary>Verifies TryConvertTyped WithNullValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new NullableByteToByteTypeConverter();

        var success = converter.TryConvertTyped(null, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies TryConvertTyped WithInvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithInvalidType_ReturnsFalse()
    {
        var converter = new NullableByteToByteTypeConverter();
        const string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
