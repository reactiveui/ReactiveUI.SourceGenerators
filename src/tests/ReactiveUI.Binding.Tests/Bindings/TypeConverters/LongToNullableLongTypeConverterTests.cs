// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting long to nullable long.</summary>
public class LongToNullableLongTypeConverterTests
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
        var converter = new LongToNullableLongTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert AlwaysSucceeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_AlwaysSucceeds()
    {
        var converter = new LongToNullableLongTypeConverter();
        const long value = 1_234_567_890_123_456L;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo((long?)SampleLong);
    }

    /// <summary>Verifies FromType ReturnsLong.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsLong()
    {
        var converter = new LongToNullableLongTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(long));
    }

    /// <summary>Verifies ToType ReturnsLongNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsLongNullable()
    {
        var converter = new LongToNullableLongTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(long?));
    }

    /// <summary>Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new LongToNullableLongTypeConverter();
        const long value = 42L;

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo((long?)SmallLong);
    }

    /// <summary>Verifies TryConvertTyped WithNullValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new LongToNullableLongTypeConverter();

        var success = converter.TryConvertTyped(null, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies TryConvertTyped WithInvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithInvalidType_ReturnsFalse()
    {
        var converter = new LongToNullableLongTypeConverter();
        const string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
