// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting DateTimeOffset to strings.</summary>
public class DateTimeOffsetToStringTypeConverterTests
{
    /// <summary>The UTC offset, in hours, of the sample timestamp under test.</summary>
    private const int OffsetHours = -5;

    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new DateTimeOffsetToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DateTimeOffset Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DateTimeOffset_Succeeds()
    {
        var converter = new DateTimeOffsetToStringTypeConverter();
        var value = new DateTimeOffset(2_024, 1, 15, 10, 30, 45, TimeSpan.FromHours(OffsetHours));

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new DateTimeOffsetToStringTypeConverter();
        var result = converter.TryConvert(DateTimeOffset.MinValue, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(DateTimeOffset.MinValue.ToString());
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new DateTimeOffsetToStringTypeConverter();
        var result = converter.TryConvert(DateTimeOffset.MaxValue, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(DateTimeOffset.MaxValue.ToString());
    }
}
