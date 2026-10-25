// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>Tests for the <see cref="DefaultViewLocator"/> class.</summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class DefaultViewLocatorTests
{
    /// <summary>Verifies that ResolveView returns null when the view model is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_NullViewModel_ReturnsNull()
    {
        var locator = new DefaultViewLocator();

        var result = locator.ResolveView((object?)null);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that the generic ResolveView returns null when the view model is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewGeneric_NullViewModel_ReturnsNull()
    {
        var locator = new DefaultViewLocator();

        var result = locator.ResolveView<TestViewModel>(null!, null);

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that Map registers a view and ResolveView resolves it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Map_ThenResolve_ReturnsView()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<TestView>();
    }

    /// <summary>Verifies that Map with a factory creates views using the factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapWithFactory_ThenResolve_UsesFactory()
    {
        var locator = new DefaultViewLocator();
        var factoryCalled = false;
        locator.Map<TestViewModel>(() =>
        {
            factoryCalled = true;
            return new TestView();
        });

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(factoryCalled).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that the generic ResolveView resolves from explicit mappings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewGeneric_WithMapping_ReturnsView()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        var vm = new TestViewModel();
        var result = locator.ResolveView(vm);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that Unmap removes a mapping and subsequent resolves return null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Unmap_RemovesMapping()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        var removed = locator.Unmap<TestViewModel>();
        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(removed).IsTrue();
        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that Unmap returns false when no mapping exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Unmap_NoMapping_ReturnsFalse()
    {
        var locator = new DefaultViewLocator();

        var removed = locator.Unmap<TestViewModel>();

        await Assert.That(removed).IsFalse();
    }

    /// <summary>Verifies that contracts provide independent namespaces for mappings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Map_WithContract_ResolvesByContract()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>("desktop");

        var withContract = locator.ResolveView(new TestViewModel(), "desktop");
        var withoutContract = locator.ResolveView(new TestViewModel());

        await Assert.That(withContract).IsNotNull();
        await Assert.That(withoutContract).IsNull();
    }

    /// <summary>Verifies that ResolveView sets the ViewModel property on the resolved view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_SetsViewModelOnView()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        var vm = new TestViewModel { Name = "test" };
        var view = locator.ResolveView(vm);

        await Assert.That(view).IsNotNull();
        await Assert.That(view!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that the generated dispatch function is called when set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetGeneratedViewDispatch_IsUsedByResolve()
    {
        var locator = new DefaultViewLocator();
        var dispatchCalled = false;

        DefaultViewLocator.SetGeneratedViewDispatch((_, _) =>
        {
            dispatchCalled = true;
            return new TestView();
        });

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(dispatchCalled).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that generated dispatch takes priority over explicit mappings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GeneratedDispatch_TakesPriorityOverMappings()
    {
        var locator = new DefaultViewLocator();
        var generatedView = new TestView();
        locator.Map<TestViewModel>(static () => new TestView());

        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => generatedView);

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(result).IsEqualTo(generatedView);
    }

    /// <summary>Verifies that when generated dispatch returns null, mappings are used as fallback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GeneratedDispatch_ReturnsNull_FallsBackToMappings()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        DefaultViewLocator.SetGeneratedViewDispatch(static (_, _) => null);

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<TestView>();
    }

    /// <summary>Verifies that the generic ResolveView uses the generated dispatch.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewGeneric_UsesGeneratedDispatch()
    {
        var locator = new DefaultViewLocator();
        var generatedView = new TestView();

        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => generatedView);

        var result = locator.ResolveView(new TestViewModel());

        await Assert.That(result).IsEqualTo(generatedView);
    }

    /// <summary>Verifies that null contract is normalized to empty string.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_NullContract_NormalizedToEmpty()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();

        var withNull = locator.ResolveView(new TestViewModel());
        var withEmpty = locator.ResolveView(new TestViewModel(), string.Empty);

        await Assert.That(withNull).IsNotNull();
        await Assert.That(withEmpty).IsNotNull();
    }

    /// <summary>Verifies that SetGeneratedViewDispatch throws on null argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SetGeneratedViewDispatch_NullThrows()
    {
        var action = static () => DefaultViewLocator.SetGeneratedViewDispatch(null!);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>Verifies that Map with factory throws on null factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapWithFactory_NullFactory_Throws()
    {
        var locator = new DefaultViewLocator();
        var action = () => locator.Map<TestViewModel>(null!);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>Verifies that generic ResolveView falls back to service locator when no dispatch or mapping exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewGeneric_ServiceLocatorFallback()
    {
        var locator = new DefaultViewLocator();
        var vm = new TestViewModel();

        // Register an IViewFor<TestViewModel> in the service locator
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView());

        var result = locator.ResolveView(vm);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(result!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that generic ResolveView falls back to service locator with a non-empty contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewGeneric_ServiceLocatorWithContract()
    {
        var locator = new DefaultViewLocator();
        var vm = new TestViewModel();

        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView(), "custom");

        var result = locator.ResolveView(vm, "custom");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that non-generic ResolveView uses generated dispatch and sets ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewNonGeneric_GeneratedDispatch_SetsViewModel()
    {
        var locator = new DefaultViewLocator();
        var vm = new TestViewModel { Name = "dispatched" };
        var dispatchView = new TestView();

        DefaultViewLocator.SetGeneratedViewDispatch((_, _) => dispatchView);

        var result = locator.ResolveView((object)vm);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that non-generic ResolveView falls back to mappings when dispatch returns null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewNonGeneric_MappingFallback_SetsViewModel()
    {
        var locator = new DefaultViewLocator();
        var vm = new TestViewModel { Name = "mapped" };
        locator.Map<TestViewModel, TestView>();

        DefaultViewLocator.SetGeneratedViewDispatch(static (_, _) => null);

        var result = locator.ResolveView((object)vm);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.ViewModel).IsEqualTo(vm);
    }

    /// <summary>Verifies that non-generic ResolveView returns null when no resolution succeeds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewNonGeneric_NoMatch_ReturnsNull()
    {
        var locator = new DefaultViewLocator();

        var result = locator.ResolveView((object)new TestViewModel());

        await Assert.That(result).IsNull();
    }

    /// <summary>Verifies that CreateMappingBuilder returns a builder that registers mappings on this locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateMappingBuilder_RegistersOnLocator()
    {
        var locator = new DefaultViewLocator();
        var builder = locator.CreateMappingBuilder();

        _ = builder.Map<TestViewModel, TestView>();

        var vm = new TestViewModel();
        var result = locator.ResolveView(vm);

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<TestView>();
    }

    /// <summary>Simple view model for testing.</summary>
    private sealed class TestViewModel
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

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
}
