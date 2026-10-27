// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to booleans.</summary>
public class StringToBooleanTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToBooleanTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert TrueString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TrueString_Succeeds()
    {
        var converter = new StringToBooleanTypeConverter();

        var result = converter.TryConvert("True", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsTrue();
    }

    /// <summary>Verifies TryConvert FalseString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_FalseString_Succeeds()
    {
        var converter = new StringToBooleanTypeConverter();

        var result = converter.TryConvert("False", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsFalse();
    }

    /// <summary>Verifies TryConvert TrueLowercase Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TrueLowercase_Succeeds()
    {
        var converter = new StringToBooleanTypeConverter();

        var result = converter.TryConvert("true", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsTrue();
    }

    /// <summary>Verifies TryConvert FalseLowercase Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_FalseLowercase_Succeeds()
    {
        var converter = new StringToBooleanTypeConverter();

        var result = converter.TryConvert("false", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsFalse();
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new StringToBooleanTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert EmptyString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToBooleanTypeConverter();
        var result = converter.TryConvert(string.Empty, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert InvalidString ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToBooleanTypeConverter();
        var result = converter.TryConvert("invalid", null, out _);

        await Assert.That(result).IsFalse();
    }
}
