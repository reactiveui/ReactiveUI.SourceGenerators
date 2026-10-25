// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable double to double.</summary>
public class NullableDoubleToDoubleTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Sample double value used for precise conversion checks.</summary>
    private const double SampleDouble = 123.456789;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert WithValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithValue_Succeeds()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        var result = converter.TryConvert(SampleDouble, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(SampleDouble);
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies FromType ReturnsDoubleNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsDoubleNullable()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(double?));
    }

    /// <summary>Verifies ToType ReturnsDouble.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDouble()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(double));
    }
}
