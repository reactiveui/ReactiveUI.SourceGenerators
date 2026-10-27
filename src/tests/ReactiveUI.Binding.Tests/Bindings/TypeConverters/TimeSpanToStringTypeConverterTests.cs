// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting TimeSpan to strings.</summary>
public class TimeSpanToStringTypeConverterTests
{
    /// <summary>A negative duration, in minutes, used to check sign handling.</summary>
    private const int NegativeMinutes = -30;

    /// <summary>The number of hours in the sample duration under test.</summary>
    private const double SampleHours = 2.5;

    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert TimeSpan Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var value = TimeSpan.FromHours(SampleHours);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }

    /// <summary>Verifies TryConvert ZeroTimeSpan Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroTimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var result = converter.TryConvert(TimeSpan.Zero, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00:00:00");
    }

    /// <summary>Verifies TryConvert NegativeTimeSpan Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeTimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var value = TimeSpan.FromMinutes(NegativeMinutes);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }
}
