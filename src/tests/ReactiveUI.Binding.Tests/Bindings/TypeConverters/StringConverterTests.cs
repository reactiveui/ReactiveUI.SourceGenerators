// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="StringConverter"/> type converter.</summary>
public class StringConverterTests
{
    /// <summary>The expected affinity reported by the string converter.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>Verifies FromType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies ToType ReturnsStringType.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsStringType()
    {
        var converter = new StringConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies TryConvertTyped EmptyString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_EmptyString_Succeeds()
    {
        var converter = new StringConverter();
        var value = string.Empty;

        var result = converter.TryConvertTyped(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies TryConvertTyped IgnoresConversionHint.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_IgnoresConversionHint()
    {
        var converter = new StringConverter();
        const string value = "test";

        var result = converter.TryConvertTyped(value, "some hint", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("test");
    }

    /// <summary>Verifies TryConvertTyped NonStringValue ReturnsFalse.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NonStringValue_ReturnsFalse()
    {
        var converter = new StringConverter();
        const int value = 123;
        var result = converter.TryConvertTyped(value, null, out _);

        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies TryConvertTyped NullValue ReturnsFalseAndNull.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullValue_ReturnsFalseAndNull()
    {
        var converter = new StringConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>Verifies TryConvertTyped StringToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_StringToString_Succeeds()
    {
        var converter = new StringConverter();
        const string value = "test";

        var result = converter.TryConvertTyped(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("test");
    }
}
