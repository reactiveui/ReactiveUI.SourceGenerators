// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to doubles.</summary>
public class StringToDoubleTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Double value parsed from scientific notation.</summary>
    private const double ScientificDouble = 1.23E+10;

    /// <summary>Double value parsed from a positive numeric string.</summary>
    private const double ParsedDouble = 123.456;

    /// <summary>Double value parsed from a negative numeric string.</summary>
    private const double NegativeDouble = -123.456;

    /// <summary>Double value parsed in the typed conversion test.</summary>
    private const double TypedDouble = 456.789;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToDoubleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToDoubleTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToDoubleTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert ScientificNotation Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ScientificNotation_Succeeds()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvert(ScientificDouble.ToString("E2", System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ScientificDouble);
    }

    /// <summary>Verifies TryConvert StringToDouble Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToDouble_Succeeds()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvert(ParsedDouble.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ParsedDouble);
    }

    /// <summary>Verifies TryConvert NullString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullString_ReturnsFalse()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsEqualTo(0.0);
    }

    /// <summary>Verifies TryConvert ZeroValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroValue_Succeeds()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvert("0", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(0.0);
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvert(NegativeDouble.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(NegativeDouble);
    }

    /// <summary>Verifies TryConvertTyped ValidString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidString_Succeeds()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvertTyped(TypedDouble.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(TypedDouble);
    }

    /// <summary>Verifies TryConvertTyped InvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_InvalidType_ReturnsFalse()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvertTyped(ParsedDouble, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies TryConvertTyped NullInput ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReturnsFalse()
    {
        var converter = new StringToDoubleTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies FromType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringToDoubleTypeConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies ToType ReturnsDoubleType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDoubleType()
    {
        var converter = new StringToDoubleTypeConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(double));
    }
}
