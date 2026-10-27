// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable long to long.</summary>
public class NullableLongToLongTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Sample long value used for conversion round-trips.</summary>
    private const long SampleLong = 1_234_567_890_123_456L;

    /// <summary>Smaller long value used for conversion checks.</summary>
    private const long SmallLong = 42L;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableLongToLongTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert WithValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithValue_Succeeds()
    {
        var converter = new NullableLongToLongTypeConverter();
        var result = converter.TryConvert(SampleLong, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(SampleLong);
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableLongToLongTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies FromType ReturnsLongNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsLongNullable()
    {
        var converter = new NullableLongToLongTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(long?));
    }

    /// <summary>Verifies ToType ReturnsLong.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsLong()
    {
        var converter = new NullableLongToLongTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(long));
    }

    /// <summary>Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new NullableLongToLongTypeConverter();
        var success = converter.TryConvertTyped(SmallLong, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(SmallLong);
    }

    /// <summary>Verifies TryConvertTyped WithNullValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new NullableLongToLongTypeConverter();

        var success = converter.TryConvertTyped(null, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies TryConvertTyped WithInvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithInvalidType_ReturnsFalse()
    {
        var converter = new NullableLongToLongTypeConverter();
        const string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
