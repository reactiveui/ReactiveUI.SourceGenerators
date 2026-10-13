// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to floats (single-precision).</summary>
public class StringToSingleTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Single value parsed from a positive numeric string.</summary>
    private const float ParsedSingle = 123.456F;

    /// <summary>Single value parsed from a negative numeric string.</summary>
    private const float NegativeSingle = -123.456F;

    /// <summary>Single value parsed from scientific notation.</summary>
    private const float ScientificSingle = 1.23E+5F;

    /// <summary>Single value parsed in the typed conversion test.</summary>
    private const float TypedSingle = 456.789F;

    /// <summary>Tolerance used when comparing parsed single-precision values.</summary>
    private const float SingleTolerance = 0.001F;

    /// <summary>Tolerance used when comparing scientific-notation single values.</summary>
    private const float ScientificTolerance = 0.1F;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToSingleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert StringToSingle Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToSingle_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(ParsedSingle.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ParsedSingle).Within(SingleTolerance);
    }

    /// <summary>Verifies TryConvert NullString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsEqualTo(0.0F);
    }

    /// <summary>Verifies TryConvert ZeroValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroValue_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("0", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(0.0F);
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(NegativeSingle.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(NegativeSingle).Within(SingleTolerance);
    }

    /// <summary>Verifies TryConvert ScientificNotation Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ScientificNotation_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(ScientificSingle.ToString("E2", System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ScientificSingle).Within(ScientificTolerance);
    }

    /// <summary>Verifies TryConvertTyped ValidString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidString_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped(TypedSingle.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsTypeOf<float>();
        await Assert.That((float)output!).IsEqualTo(TypedSingle).Within(SingleTolerance);
    }

    /// <summary>Verifies TryConvertTyped InvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_InvalidType_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped(ParsedSingle, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies TryConvertTyped NullInput ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies FromType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringToSingleTypeConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies ToType ReturnsFloatType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsFloatType()
    {
        var converter = new StringToSingleTypeConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(float));
    }
}
