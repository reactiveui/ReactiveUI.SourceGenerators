// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to decimals.</summary>
public class StringToDecimalTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Decimal value parsed from a positive numeric string.</summary>
    private const decimal ParsedDecimal = 123.456M;

    /// <summary>Decimal value parsed from a negative numeric string.</summary>
    private const decimal NegativeDecimal = -123.456M;

    /// <summary>Decimal value parsed in the typed conversion test.</summary>
    private const decimal TypedDecimal = 456.789M;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToDecimalTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToDecimalTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToDecimalTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert StringToDecimal Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToDecimal_Succeeds()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvert(ParsedDecimal.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ParsedDecimal);
    }

    /// <summary>Verifies TryConvert NullString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullString_ReturnsFalse()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsEqualTo(0M);
    }

    /// <summary>Verifies TryConvert ZeroValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroValue_Succeeds()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvert("0", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(0M);
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvert(NegativeDecimal.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(NegativeDecimal);
    }

    /// <summary>Verifies TryConvertTyped ValidString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidString_Succeeds()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvertTyped(TypedDecimal.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(TypedDecimal);
    }

    /// <summary>Verifies TryConvertTyped InvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_InvalidType_ReturnsFalse()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvertTyped(ParsedDecimal, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies TryConvertTyped NullInput ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReturnsFalse()
    {
        var converter = new StringToDecimalTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies FromType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringToDecimalTypeConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies ToType ReturnsDecimalType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDecimalType()
    {
        var converter = new StringToDecimalTypeConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(decimal));
    }
}
