// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting booleans to strings.</summary>
public class BooleanToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new BooleanToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert True ReturnsTrue.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_True_ReturnsTrue()
    {
        var converter = new BooleanToStringTypeConverter();

        var result = converter.TryConvert(true, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("True");
    }

    /// <summary>Verifies TryConvert False ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_False_ReturnsFalse()
    {
        var converter = new BooleanToStringTypeConverter();

        var result = converter.TryConvert(false, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("False");
    }
}
