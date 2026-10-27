// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting DateTime to strings.</summary>
public class DateTimeToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new DateTimeToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert DateTime Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DateTime_Succeeds()
    {
        var converter = new DateTimeToStringTypeConverter();
        var value = new DateTime(2_024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new DateTimeToStringTypeConverter();
        var result = converter.TryConvert(DateTime.MinValue, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(DateTime.MinValue.ToString(CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new DateTimeToStringTypeConverter();
        var result = converter.TryConvert(DateTime.MaxValue, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(DateTime.MaxValue.ToString(CultureInfo.CurrentCulture));
    }
}
