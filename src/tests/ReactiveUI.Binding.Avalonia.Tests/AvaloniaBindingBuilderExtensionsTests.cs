// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Tests for the Avalonia builder extensions, compiled once against each runtime flavour.</summary>
/// <remarks>
/// Each call casts to one interface on purpose: the concrete builder implements both, and WithAvalonia is offered
/// on each, so a concrete-typed receiver would not pick one.
/// </remarks>
public class AvaloniaBindingBuilderExtensionsTests
{
    /// <summary>WithAvalonia on the binding builder returns the same builder.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithAvalonia_OnBindingBuilder_ReturnsTheSameBuilder()
    {
        var builder = new ReactiveUIBindingBuilder(new ModernDependencyResolver(), null);

        var result = ((IReactiveUIBindingBuilder)builder).WithAvalonia();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>WithAvalonia on the app builder forwards to the binding builder overload.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithAvalonia_OnAppBuilder_ReturnsTheSameBuilder()
    {
        var builder = new ReactiveUIBindingBuilder(new ModernDependencyResolver(), null);

        var result = ((IAppBuilder)builder).WithAvalonia();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>WithAvalonia rejects a null builder.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithAvalonia_NullBuilder_ThrowsArgumentNullException() =>
        await Assert.That(static () => ((IReactiveUIBindingBuilder)null!).WithAvalonia())
            .ThrowsExactly<ArgumentNullException>();
}
