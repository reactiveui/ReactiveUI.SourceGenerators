// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Unit tests for standard binding type converters verifying basic conversion correctness.</summary>
public class BindingTypeConvertersUnitTests
{
    /// <summary>Verifies that ByteToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ByteToStringTypeConverter_Converts_Correctly()
    {
        var converter = new ByteToStringTypeConverter();
        const byte sourceValue = 123;

        // Byte to String
        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123");
    }

    /// <summary>Verifies that DecimalToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task DecimalToStringTypeConverter_Converts_Correctly()
    {
        var converter = new DecimalToStringTypeConverter();
        const decimal sourceValue = 123.456M;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies that DoubleToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task DoubleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new DoubleToStringTypeConverter();
        const double sourceValue = 123.456789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies that IntegerToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task IntegerToStringTypeConverter_Converts_Correctly()
    {
        var converter = new IntegerToStringTypeConverter();
        const int sourceValue = 123_456_789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456789");
    }

    /// <summary>Verifies that LongToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task LongToStringTypeConverter_Converts_Correctly()
    {
        var converter = new LongToStringTypeConverter();
        const long sourceValue = 1_234_567_890_123_456_789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("1234567890123456789");
    }

    /// <summary>Verifies that NullableByteToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableByteToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableByteToStringTypeConverter();
        const byte sourceValue = 123;

        // Byte? to String
        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123");
    }

    /// <summary>Verifies that NullableDecimalToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableDecimalToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        decimal? sourceValue = 123.456M;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString());
    }

    /// <summary>Verifies that NullableDoubleToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableDoubleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? sourceValue = 123.456789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString());
    }

    /// <summary>Verifies that NullableIntegerToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableIntegerToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        const int sourceValue = 123_456_789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456789");
    }

    /// <summary>Verifies that NullableLongToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableLongToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableLongToStringTypeConverter();
        const long sourceValue = 1_234_567_890_123_456_789;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("1234567890123456789");
    }

    /// <summary>Verifies that NullableShortToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableShortToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableShortToStringTypeConverter();
        const short sourceValue = 12_345;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345");
    }

    /// <summary>Verifies that NullableSingleToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableSingleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableSingleToStringTypeConverter();
        float? sourceValue = 123.45F;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString());
    }

    /// <summary>Verifies that ShortToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ShortToStringTypeConverter_Converts_Correctly()
    {
        var converter = new ShortToStringTypeConverter();
        const short sourceValue = 12_345;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345");
    }

    /// <summary>Verifies that SingleToStringTypeConverter converts correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SingleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new SingleToStringTypeConverter();
        const float sourceValue = 123.45F;

        var result = converter.TryConvert(sourceValue, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(sourceValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }
}
