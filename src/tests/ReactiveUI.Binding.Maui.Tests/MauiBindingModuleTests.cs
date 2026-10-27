// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Maui.Tests;

/// <summary>
/// Tests for the MAUI platform module. Compiled twice: once against ReactiveUI.Binding.Maui and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Maui, so both leaves are exercised by the
/// same assertions.
/// </summary>
public class MauiBindingModuleTests
{
    /// <summary>The number of converters the module registers on a target without a WinUI head.</summary>
    private const int ExpectedConverters = 2;

    /// <summary>Verifies that Configure registers both directions of the visibility conversion.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersBothVisibilityConverters()
    {
        var resolver = new ModernDependencyResolver();
        var module = new MauiBindingModule();

        module.Configure(resolver);

        var converters = resolver.GetServices<IBindingTypeConverter>().ToList();

        await Assert.That(converters.Count).IsEqualTo(ExpectedConverters);
        await Assert.That(converters.Exists(static c => c is BooleanToVisibilityTypeConverter)).IsTrue();
        await Assert.That(converters.Exists(static c => c is VisibilityToBooleanTypeConverter)).IsTrue();
    }

    /// <summary>Verifies that Configure registers the invoker that writes on a view's own thread.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_RegistersTheViewThreadInvoker()
    {
        var resolver = new ModernDependencyResolver();

        new MauiBindingModule().Configure(resolver);

        var invokers = resolver.GetServices<IViewThreadInvoker>().ToList();

        await Assert.That(invokers.Exists(static i => i is DispatcherViewThreadInvoker)).IsTrue();
    }

    /// <summary>Verifies that Configure rejects a null resolver rather than failing later.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new MauiBindingModule();

        await Assert.That(() => module.Configure(null!)).ThrowsExactly<ArgumentNullException>();
    }
}
