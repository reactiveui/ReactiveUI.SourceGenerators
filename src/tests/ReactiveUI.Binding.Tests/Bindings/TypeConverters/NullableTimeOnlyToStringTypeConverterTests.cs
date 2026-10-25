// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER
namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable TimeOnly to strings.</summary>
public class NullableTimeOnlyToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Hour component of the sample time used across the conversion round-trips.</summary>
    private const int SampleHour = 10;

    /// <summary>Minute component of the sample time used across the conversion round-trips.</summary>
    private const int SampleMinute = 30;

    /// <summary>Second component of the sample time used across the conversion round-trips.</summary>
    private const int SampleSecond = 45;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableTimeOnlyToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert TimeOnly Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TimeOnly_Succeeds()
    {
        var converter = new NullableTimeOnlyToStringTypeConverter();
        TimeOnly? value = new TimeOnly(SampleHour, SampleMinute, SampleSecond);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.Value.ToString());
    }

    /// <summary>Verifies TryConvert Null ReturnsNullString.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsNullString()
    {
        var converter = new NullableTimeOnlyToStringTypeConverter();
        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }
}
#endif
