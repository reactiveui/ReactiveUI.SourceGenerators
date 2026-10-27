// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to nullable floats (single-precision).</summary>
public class StringToNullableSingleTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Single value parsed from a positive numeric string.</summary>
    private const float ParsedSingle = 123.456F;

    /// <summary>Tolerance used when comparing parsed single-precision values.</summary>
    private const float SingleTolerance = 0.001F;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToNullableSingleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsTrue()
    {
        var converter = new StringToNullableSingleTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToNullableSingleTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert StringToSingleNullable Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToSingleNullable_Succeeds()
    {
        var converter = new StringToNullableSingleTypeConverter();

        var result = converter.TryConvert(ParsedSingle.ToString(System.Globalization.CultureInfo.CurrentCulture), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNotNull();
        await Assert.That(output!.Value).IsEqualTo(ParsedSingle).Within(SingleTolerance);
    }
}
