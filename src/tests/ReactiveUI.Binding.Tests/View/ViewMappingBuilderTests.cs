// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests for the <see cref="ViewMappingBuilder"/> fluent API.</summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class ViewMappingBuilderTests
{
    /// <summary>Verifies that Map registers a mapping that the locator can resolve.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Map_RegistersMapping()
    {
        var locator = new DefaultViewLocator();
        var builder = new ViewMappingBuilder(locator);

        _ = builder.Map<TestViewModel, TestView>();

        var result = locator.ResolveView(new TestViewModel());
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<TestView>();
    }

    /// <summary>Verifies that Map returns the builder for chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Map_ReturnsSelfForChaining()
    {
        var locator = new DefaultViewLocator();
        var builder = new ViewMappingBuilder(locator);

        var returned = builder.Map<TestViewModel, TestView>();

        await Assert.That(returned).IsEqualTo(builder);
    }

    /// <summary>Verifies that multiple mappings can be chained.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Map_MultipleChained()
    {
        var locator = new DefaultViewLocator();
        var builder = new ViewMappingBuilder(locator);

        _ = builder
            .Map<TestViewModel, TestView>()
            .Map<OtherViewModel, OtherView>();

        var result1 = locator.ResolveView(new TestViewModel());
        var result2 = locator.ResolveView(new OtherViewModel());

        await Assert.That(result1).IsNotNull();
        await Assert.That(result2).IsNotNull();
    }

    /// <summary>Verifies that Map with factory delegates to the locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapWithFactory_RegistersFactory()
    {
        var locator = new DefaultViewLocator();
        var builder = new ViewMappingBuilder(locator);
        var factoryCalled = false;

        _ = builder.Map<TestViewModel>(() =>
        {
            factoryCalled = true;
            return new TestView();
        });

        _ = locator.ResolveView(new TestViewModel());

        await Assert.That(factoryCalled).IsTrue();
    }

    /// <summary>Simple view model for testing.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "The mapping tests key off the type identity alone.")]
    private sealed class TestViewModel;

    /// <summary>Another view model for testing multiple mappings.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "The mapping tests key off the type identity alone.")]
    private sealed class OtherViewModel;

    /// <summary>Simple view for testing.</summary>
    private sealed class TestView : IViewFor<TestViewModel>
    {
        /// <inheritdoc/>
        public TestViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as TestViewModel;
        }
    }

    /// <summary>Another view for testing multiple mappings.</summary>
    private sealed class OtherView : IViewFor<OtherViewModel>
    {
        /// <inheritdoc/>
        public OtherViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as OtherViewModel;
        }
    }
}
