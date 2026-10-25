// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the WPF platform module. Compiled twice: once against ReactiveUI.Binding.Wpf and once, under
/// REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Wpf, so both leaves are exercised by the same
/// assertions.
/// </summary>
public class WpfBindingModuleTests
{
    /// <summary>Verifies that Configure registers the dependency-property observation factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersDependencyPropertyObservation()
    {
        var resolver = new ModernDependencyResolver();

        new WpfBindingModule().Configure(resolver);

        var factories = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(factories.Exists(static f => f is DependencyObjectObservableForProperty)).IsTrue();
    }

    /// <summary>Verifies that Configure registers the invoker that writes on a view's own thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersTheViewThreadInvoker()
    {
        var resolver = new ModernDependencyResolver();

        new WpfBindingModule().Configure(resolver);

        var invokers = resolver.GetServices<IViewThreadInvoker>().ToList();

        await Assert.That(invokers.Exists(static i => i is DispatcherViewThreadInvoker)).IsTrue();
    }

    /// <summary>Verifies that Configure rejects a null resolver rather than failing later.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new WpfBindingModule();

        await Assert.That(() => module.Configure(null!)).ThrowsExactly<ArgumentNullException>();
    }
}
