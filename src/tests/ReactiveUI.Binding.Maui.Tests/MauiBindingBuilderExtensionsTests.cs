// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Maui.Tests;

/// <summary>
/// Tests for the MAUI builder extensions. Compiled twice: once against ReactiveUI.Binding.Maui and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Maui, so both leaves are exercised by the
/// same assertions.
/// </summary>
/// <remarks>
/// Each call goes through an explicit interface cast on purpose: the concrete builder implements both
/// interfaces and WithMaui is offered on each, so a concrete-typed receiver would not pick one.
/// </remarks>
public class MauiBindingBuilderExtensionsTests
{
    /// <summary>Verifies that WithMaui on the binding builder returns the same builder for chaining.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithMaui_OnBindingBuilder_ReturnsTheSameBuilder()
    {
        var builder = new ReactiveUIBindingBuilder(new ModernDependencyResolver(), null);

        var result = ((IReactiveUIBindingBuilder)builder).WithMaui();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that WithMaui on the app builder forwards to the binding builder overload.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithMaui_OnAppBuilder_ReturnsTheSameBuilder()
    {
        var builder = new ReactiveUIBindingBuilder(new ModernDependencyResolver(), null);

        var result = ((IAppBuilder)builder).WithMaui();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that WithMaui rejects a null builder rather than failing later.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithMaui_NullBuilder_ThrowsArgumentNullException() =>
        await Assert.That(static () => ((IReactiveUIBindingBuilder)null!).WithMaui())
            .ThrowsExactly<ArgumentNullException>();
}
