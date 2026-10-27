// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for converting strings to Uri.</summary>
public class StringToUriTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToUriTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert AbsoluteUri Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_AbsoluteUri_Succeeds()
    {
        var converter = new StringToUriTypeConverter();
        var expected = new Uri("https://reactiveui.net/docs");

        var result = converter.TryConvert("https://reactiveui.net/docs", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(expected);
    }

    /// <summary>Verifies TryConvert RelativeUri Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_RelativeUri_Succeeds()
    {
        var converter = new StringToUriTypeConverter();
        var expected = new Uri("/path/to/resource", UriKind.Relative);

        var result = converter.TryConvert("/path/to/resource", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(expected);
    }

    /// <summary>Verifies TryConvert Null ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new StringToUriTypeConverter();
        var result = converter.TryConvert(null, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvert EmptyString CreatesRelativeUri.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_CreatesRelativeUri()
    {
        var converter = new StringToUriTypeConverter();

        var result = converter.TryConvert(string.Empty, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNotNull();
    }
}
