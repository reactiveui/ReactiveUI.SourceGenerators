// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER
namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting nullable DateOnly to strings.</summary>
public class NullableDateOnlyToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Year component of the sample date used across the conversion round-trips.</summary>
    private const int SampleYear = 2_024;

    /// <summary>Day component of the sample date used across the conversion round-trips.</summary>
    private const int SampleDay = 15;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDateOnlyToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DateOnly Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DateOnly_Succeeds()
    {
        var converter = new NullableDateOnlyToStringTypeConverter();
        DateOnly? value = new DateOnly(SampleYear, 1, SampleDay);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.Value.ToString());
    }

    /// <summary>Verifies TryConvert Null ReturnsNullString.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsNullString()
    {
        var converter = new NullableDateOnlyToStringTypeConverter();
        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }
}
#endif
