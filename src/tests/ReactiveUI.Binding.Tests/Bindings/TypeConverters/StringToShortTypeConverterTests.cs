// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to short integers.</summary>
public class StringToShortTypeConverterTests
{
    /// <summary>A sample value used by these tests.</summary>
    private const int SampleValue = 1_234;

    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Short value parsed from a positive numeric string.</summary>
    private const short ParsedShort = 12_345;

    /// <summary>Short value parsed from a negative numeric string.</summary>
    private const short NegativeShort = -12_345;

    /// <summary>Short value parsed in the typed conversion test.</summary>
    private const short TypedShort = 1_000;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToShortTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert OutOfRangeValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_OutOfRangeValue_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();
        var result = converter.TryConvert("99999", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert StringToShort Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToShort_Succeeds()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvert("12345", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(ParsedShort);
    }

    /// <summary>Verifies TryConvert NullString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullString_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsEqualTo((short)0);
    }

    /// <summary>Verifies TryConvert ZeroValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroValue_Succeeds()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvert("0", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo((short)0);
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvert("-12345", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(NegativeShort);
    }

    /// <summary>Verifies TryConvertTyped ValidString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidString_Succeeds()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvertTyped("1000", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(TypedShort);
    }

    /// <summary>Verifies TryConvertTyped InvalidType ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_InvalidType_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvertTyped(SampleValue, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies TryConvertTyped NullInput ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReturnsFalse()
    {
        var converter = new StringToShortTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies FromType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringToShortTypeConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies ToType ReturnsShortType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsShortType()
    {
        var converter = new StringToShortTypeConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(short));
    }
}
