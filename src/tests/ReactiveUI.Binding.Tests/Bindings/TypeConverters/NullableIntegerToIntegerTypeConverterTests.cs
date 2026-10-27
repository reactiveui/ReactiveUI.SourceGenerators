// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable int to int.</summary>
public class NullableIntegerToIntegerTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Sample integer value used for conversion round-trips.</summary>
    private const int SampleInteger = 123_456;

    /// <summary>Smaller integer value used for conversion checks.</summary>
    private const int SmallInteger = 42;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert WithValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithValue_Succeeds()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        var result = converter.TryConvert(SampleInteger, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(SampleInteger);
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies FromType ReturnsIntNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsIntNullable()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(int?));
    }

    /// <summary>Verifies ToType ReturnsInt.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsInt()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(int));
    }

    /// <summary>Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        var success = converter.TryConvertTyped(SmallInteger, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(SmallInteger);
    }

    /// <summary>Verifies TryConvertTyped WithNullValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();

        var success = converter.TryConvertTyped(null, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies TryConvertTyped WithInvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithInvalidType_ReturnsFalse()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        const string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
