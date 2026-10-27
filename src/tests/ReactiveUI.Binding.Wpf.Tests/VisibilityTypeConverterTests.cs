// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the WPF visibility converters. Compiled twice: once against ReactiveUI.Binding.Wpf and
/// once, under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Wpf, so both leaves are exercised
/// by the same assertions.
/// </summary>
public class VisibilityTypeConverterTests
{
    /// <summary>Verifies that true maps to Visible and false to Collapsed by default.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BooleanToVisibility_WithoutHint_UsesCollapsedForFalse()
    {
        var converter = new BooleanToVisibilityTypeConverter();

        await Assert.That(converter.TryConvert(true, null, out var visible)).IsTrue();
        await Assert.That(visible).IsEqualTo(Visibility.Visible);

        await Assert.That(converter.TryConvert(false, null, out var hidden)).IsTrue();
        await Assert.That(hidden).IsEqualTo(Visibility.Collapsed);
    }

    /// <summary>Verifies that the Inverse hint flips the mapping.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BooleanToVisibility_WithInverseHint_FlipsTheMapping()
    {
        var converter = new BooleanToVisibilityTypeConverter();

        await Assert.That(converter.TryConvert(true, BooleanToVisibilityHints.Inverse, out var collapsed)).IsTrue();
        await Assert.That(collapsed).IsEqualTo(Visibility.Collapsed);

        await Assert.That(converter.TryConvert(false, BooleanToVisibilityHints.Inverse, out var visible)).IsTrue();
        await Assert.That(visible).IsEqualTo(Visibility.Visible);
    }

    /// <summary>Verifies that the UseHidden hint selects Hidden rather than Collapsed.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BooleanToVisibility_WithUseHiddenHint_UsesHidden()
    {
        var converter = new BooleanToVisibilityTypeConverter();

        await Assert.That(converter.TryConvert(false, BooleanToVisibilityHints.UseHidden, out var result)).IsTrue();
        await Assert.That(result).IsEqualTo(Visibility.Hidden);
    }

    /// <summary>Verifies that Inverse and UseHidden combine.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BooleanToVisibility_WithCombinedHints_AppliesBoth()
    {
        var converter = new BooleanToVisibilityTypeConverter();
        const BooleanToVisibilityHints Hints = BooleanToVisibilityHints.Inverse | BooleanToVisibilityHints.UseHidden;

        await Assert.That(converter.TryConvert(true, Hints, out var result)).IsTrue();
        await Assert.That(result).IsEqualTo(Visibility.Hidden);
    }

    /// <summary>Verifies that a hint of an unrelated type is ignored rather than throwing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task BooleanToVisibility_WithUnrelatedHint_FallsBackToDefault()
    {
        var converter = new BooleanToVisibilityTypeConverter();

        await Assert.That(converter.TryConvert(false, "not a hint", out var result)).IsTrue();
        await Assert.That(result).IsEqualTo(Visibility.Collapsed);
    }

    /// <summary>Verifies that only Visible maps to true.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task VisibilityToBoolean_MapsOnlyVisibleToTrue()
    {
        var converter = new VisibilityToBooleanTypeConverter();

        await Assert.That(converter.TryConvert(Visibility.Visible, null, out var visible)).IsTrue();
        await Assert.That(visible).IsTrue();

        await Assert.That(converter.TryConvert(Visibility.Collapsed, null, out var collapsed)).IsTrue();
        await Assert.That(collapsed).IsFalse();

        await Assert.That(converter.TryConvert(Visibility.Hidden, null, out var hidden)).IsTrue();
        await Assert.That(hidden).IsFalse();
    }

    /// <summary>Verifies that the Inverse hint flips the result.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task VisibilityToBoolean_WithInverseHint_FlipsTheResult()
    {
        var converter = new VisibilityToBooleanTypeConverter();

        await Assert.That(converter.TryConvert(Visibility.Visible, BooleanToVisibilityHints.Inverse, out var result)).IsTrue();
        await Assert.That(result).IsFalse();
    }

    /// <summary>Verifies that both converters advertise the internal converter affinity.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Converters_AdvertiseTheInternalConverterAffinity()
    {
        await Assert.That(new BooleanToVisibilityTypeConverter().GetAffinityForObjects())
            .IsEqualTo(BindingAffinity.DefaultInternalTypeConverter);
        await Assert.That(new VisibilityToBooleanTypeConverter().GetAffinityForObjects())
            .IsEqualTo(BindingAffinity.DefaultInternalTypeConverter);
    }
}
