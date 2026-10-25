// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>Tests for the <see cref="ReactiveUIBindingModule"/> class.</summary>
public class ReactiveUIBindingModuleTests
{
    /// <summary>The expected number of observable-for-property services registered by the module.</summary>
    private const int ExpectedObservableServices = 2;

    /// <summary>Verifies that Configure registers INPC and POCO observable for property implementations.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersINPCAndPOCO()
    {
        var resolver = new ModernDependencyResolver();
        var module = new ReactiveUIBindingModule();

        module.Configure(resolver);

        var services = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(services.Count).IsEqualTo(ExpectedObservableServices);

        var hasINPC = services.Exists(static s => s is INPCObservableForProperty);
        var hasPOCO = services.Exists(static s => s is POCOObservableForProperty);

        await Assert.That(hasINPC).IsTrue();
        await Assert.That(hasPOCO).IsTrue();
    }

    /// <summary>
    /// Verifies that Configure does not register any command binders
    /// (known patterns are handled at compile time by the source generator).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NoCommandBinders()
    {
        var resolver = new ModernDependencyResolver();
        var module = new ReactiveUIBindingModule();

        module.Configure(resolver);

        var binders = resolver.GetServices<ICreatesCommandBinding>().ToList();

        await Assert.That(binders.Count).IsEqualTo(0);
    }

    /// <summary>Verifies that Configure throws for null resolver.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new ReactiveUIBindingModule();

        await Assert.That(() => module.Configure(null!))
            .ThrowsExactly<ArgumentNullException>();
    }
}
