// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting double to nullable double.</summary>
public class DoubleToNullableDoubleTypeConverterTests
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
        var converter = new DoubleToNullableDoubleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert AlwaysSucceeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_AlwaysSucceeds()
    {
        var converter = new DoubleToNullableDoubleTypeConverter();
        const double value = 123.456789;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo((double?)SampleDouble);
    }

    /// <summary>Verifies FromType ReturnsDouble.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsDouble()
    {
        var converter = new DoubleToNullableDoubleTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(double));
    }

    /// <summary>Verifies ToType ReturnsDoubleNullable.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDoubleNullable()
    {
        var converter = new DoubleToNullableDoubleTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(double?));
    }
}
