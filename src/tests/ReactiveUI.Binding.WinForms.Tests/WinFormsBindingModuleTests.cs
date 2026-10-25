// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>
/// Tests for the WinForms platform module. Compiled twice: once against ReactiveUI.Binding.WinForms and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.WinForms, so both leaves are exercised by the same
/// assertions.
/// </summary>
public class WinFormsBindingModuleTests
{
    /// <summary>Verifies that Configure registers the event-based observation factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersEventBasedObservation()
    {
        var resolver = new ModernDependencyResolver();

        new WinFormsBindingModule().Configure(resolver);

        var factories = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(factories.Exists(static f => f is WinFormsCreatesObservableForProperty)).IsTrue();
    }

    /// <summary>Verifies that Configure registers the invoker that writes on a control's own thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersTheViewThreadInvoker()
    {
        var resolver = new ModernDependencyResolver();

        new WinFormsBindingModule().Configure(resolver);

        var invokers = resolver.GetServices<IViewThreadInvoker>().ToList();

        await Assert.That(invokers.Exists(static i => i is ControlViewThreadInvoker)).IsTrue();
    }

    /// <summary>Verifies that Configure rejects a null resolver rather than failing later.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new WinFormsBindingModule();

        await Assert.That(() => module.Configure(null!)).ThrowsExactly<ArgumentNullException>();
    }
}
