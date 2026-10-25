// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Tests for the Avalonia platform module, compiled once against each runtime flavour.</summary>
public class AvaloniaBindingModuleTests
{
    /// <summary>Configure registers the property observer.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersThePropertyObserver()
    {
        var resolver = new ModernDependencyResolver();

        new AvaloniaBindingModule().Configure(resolver);

        await Assert.That(resolver.GetServices<ICreatesObservableForProperty>().Any(static p => p is AvaloniaObjectObservableForProperty)).IsTrue();
    }

    /// <summary>Configure registers the command binder.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersTheCommandBinder()
    {
        var resolver = new ModernDependencyResolver();

        new AvaloniaBindingModule().Configure(resolver);

        await Assert.That(resolver.GetServices<ICreatesCommandBinding>().Any(static b => b is AvaloniaCreatesCommandBinding)).IsTrue();
    }

    /// <summary>Configure registers the shared view thread invoker.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersTheSharedViewThreadInvoker()
    {
        var resolver = new ModernDependencyResolver();

        new AvaloniaBindingModule().Configure(resolver);

        await Assert.That(resolver.GetServices<IViewThreadInvoker>().Any(static i => ReferenceEquals(i, AvaloniaViewThreadInvoker.Instance))).IsTrue();
    }

    /// <summary>Configure rejects a null resolver.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException() =>
        await Assert.That(static () => new AvaloniaBindingModule().Configure(null!)).ThrowsExactly<ArgumentNullException>();
}
