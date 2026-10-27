// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting Guid to strings.</summary>
public class GuidToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new GuidToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert Guid Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Guid_Succeeds()
    {
        var converter = new GuidToStringTypeConverter();
        var value = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345678-1234-1234-1234-123456789abc");
    }

    /// <summary>Verifies TryConvert EmptyGuid Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyGuid_Succeeds()
    {
        var converter = new GuidToStringTypeConverter();
        var result = converter.TryConvert(Guid.Empty, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00000000-0000-0000-0000-000000000000");
    }
}
