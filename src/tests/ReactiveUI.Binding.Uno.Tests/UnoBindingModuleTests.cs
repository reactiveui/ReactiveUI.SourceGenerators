// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Uno.Tests;

/// <summary>Tests for the Uno platform module and builder extensions.</summary>
/// <remarks>
/// Each builder call casts to one interface on purpose: the concrete builder implements both, and WithUno is offered
/// on each, so a concrete-typed receiver would not pick one.
/// </remarks>
public class UnoBindingModuleTests
{
    /// <summary>Configure registers the shared view thread invoker.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersTheSharedViewThreadInvoker()
    {
        ModernDependencyResolver resolver = new();

        new UnoBindingModule().Configure(resolver);

        await Assert.That(resolver.GetServices<IViewThreadInvoker>().Any(static i => ReferenceEquals(i, UnoViewThreadInvoker.Instance))).IsTrue();
    }

    /// <summary>Configure rejects a null resolver.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException() =>
        await Assert.That(static () => new UnoBindingModule().Configure(null!)).ThrowsExactly<ArgumentNullException>();

    /// <summary>WithUno on the binding builder returns the same builder.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithUno_OnBindingBuilder_ReturnsTheSameBuilder()
    {
        ReactiveUIBindingBuilder builder = new(new ModernDependencyResolver(), null);

        var result = ((IReactiveUIBindingBuilder)builder).WithUno();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>WithUno on the app builder forwards to the binding builder overload.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithUno_OnAppBuilder_ReturnsTheSameBuilder()
    {
        ReactiveUIBindingBuilder builder = new(new ModernDependencyResolver(), null);

        var result = ((IAppBuilder)builder).WithUno();

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>WithUno rejects a null builder.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task WithUno_NullBuilder_ThrowsArgumentNullException() =>
        await Assert.That(static () => ((IReactiveUIBindingBuilder)null!).WithUno())
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>Anything that is not a dependency object is left to another invoker.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Claims_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(UnoViewThreadInvoker.Instance.Claims(new())).IsFalse();
}
