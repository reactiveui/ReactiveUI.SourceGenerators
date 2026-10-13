// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Tests for the <see cref="SingleToStringTypeConverter"/> type converter.</summary>
public class SingleToStringTypeConverterTests
{
    /// <summary>Expected affinity returned for matched converter type pairs.</summary>
    private const int ExpectedAffinity = 2;

    /// <summary>A value whose decimal separator differs between the invariant culture and fr-FR.</summary>
    private const float FractionalValue = 7.25F;

    /// <summary>Verifies GetAffinityForObjects Returns2.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new SingleToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(ExpectedAffinity);
    }

    /// <summary>Verifies TryConvert MaxValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = float.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(float.MaxValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert MinValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = float.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(float.MinValue.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert NegativeValue Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = -123.456F;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert SingleToString Succeeds.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_SingleToString_Succeeds()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = 123.456F;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert uses the current culture when its decimal separator is a comma.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [TUnit.Core.Executors.Culture("fr-FR")]
    public async Task TryConvert_CommaDecimalCulture_UsesCurrentCulture()
    {
        var converter = new SingleToStringTypeConverter();
        var result = converter.TryConvert(FractionalValue, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("7,25");
    }

    /// <summary>Verifies TryConvert WithConversionHint FormatsCorrectly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = 42.5F;

        var result = converter.TryConvert(value, ExpectedAffinity, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
    }

    /// <summary>Verifies TryConvert with a string format hint formats correctly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new SingleToStringTypeConverter();
        const float value = 42.5F;

        var result = converter.TryConvert(value, "E2", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString("E2"));
    }
}
