// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>
/// Covers resolving a view from a view model type alone, with no view model instance, and mapping a view model
/// type to a view the service locator creates.
/// </summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class DefaultViewLocatorTypedResolveTests
{
    /// <summary>The contract the contract-specific cases register and resolve under.</summary>
    private const string Contract = "compact";

    /// <summary>An explicit mapping resolves first, and the view is left without a view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_FromAMapping_ReturnsTheViewWithoutAViewModel()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView());

        var result = locator.ResolveView<TestViewModel>();

        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(result!.ViewModel).IsNull();
    }

    /// <summary>A mapping registered under a contract resolves only under that contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_WithAContract_UsesTheContractsMapping()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>(Contract);

        var underContract = locator.ResolveView<TestViewModel>(Contract);
        var byDefault = locator.ResolveView<TestViewModel>();

        await Assert.That(underContract).IsTypeOf<TestView>();
        await Assert.That(byDefault).IsNull();
    }

    /// <summary>A mapping whose view is not a view for the type is passed over for the service locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_MappingOfAnotherViewType_FallsBackToTheServiceLocator()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel>(static () => new UnrelatedView());
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView());

        var result = locator.ResolveView<TestViewModel>();

        await Assert.That(result).IsTypeOf<OtherTestView>();
    }

    /// <summary>With no mapping, the service locator supplies the view, under the default contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_WithoutAMapping_UsesTheServiceLocator()
    {
        var locator = new DefaultViewLocator();
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView());

        var result = locator.ResolveView<TestViewModel>(string.Empty);

        await Assert.That(result).IsTypeOf<OtherTestView>();
        await Assert.That(result!.ViewModel).IsNull();
    }

    /// <summary>With no mapping, the service locator supplies the view registered under the contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_WithoutAMapping_UsesTheServiceLocatorsContract()
    {
        var locator = new DefaultViewLocator();
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView(), Contract);

        var result = locator.ResolveView<TestViewModel>(Contract);

        await Assert.That(result).IsTypeOf<OtherTestView>();
    }

    /// <summary>Nothing mapped and nothing registered resolves to null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_NothingMappedOrRegistered_ReturnsNull()
    {
        var locator = new DefaultViewLocator();

        var result = locator.ResolveView<TestViewModel>();

        await Assert.That(result).IsNull();
    }

    /// <summary>Through the interface, the default locator answers with its own mappings.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InterfaceResolveView_DefaultLocator_UsesItsMappings()
    {
        var defaultLocator = new DefaultViewLocator();
        defaultLocator.Map<TestViewModel, TestView>(Contract);
        IViewLocator locator = defaultLocator;

        var byDefault = locator.ResolveView<TestViewModel>();
        var underContract = locator.ResolveView<TestViewModel>(Contract);

        await Assert.That(byDefault).IsNull();
        await Assert.That(underContract).IsTypeOf<TestView>();
    }

    /// <summary>Through the interface, any other locator is answered by the service locator.</summary>
    /// <param name="contract">The contract asked for; empty means the default view.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(Contract)]
    public async Task InterfaceResolveView_OtherLocator_UsesTheServiceLocator(string? contract)
    {
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(
            static () => new OtherTestView(),
            string.IsNullOrEmpty(contract) ? null : contract);
        var result = ResolveThroughInterface(new EmptyLocator(), contract);

        await Assert.That(result).IsTypeOf<OtherTestView>();
    }

    /// <summary>Asking a null locator throws rather than falling through to the service locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InterfaceResolveView_NullLocator_Throws()
    {
        IViewLocator locator = null!;

        await Assert.That(() => locator.ResolveView<TestViewModel>(contract: null)).Throws<ArgumentNullException>();
    }

    /// <summary>A service-locator mapping resolves the view the service locator registers, with the view model set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapFromServiceLocator_ResolvesTheRegisteredView()
    {
        var locator = new DefaultViewLocator();
        _ = locator.CreateMappingBuilder().MapFromServiceLocator<TestViewModel, TestView>();
        AppLocator.CurrentMutable.Register(static () => new TestView());
        var viewModel = new TestViewModel();

        var result = locator.ResolveView(viewModel);

        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(result!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>A service-locator mapping under a contract resolves only under that contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapFromServiceLocator_WithAContract_ResolvesUnderThatContract()
    {
        var locator = new DefaultViewLocator();
        var builder = locator.CreateMappingBuilder();
        AppLocator.CurrentMutable.Register(static () => new TestView());

        var returned = builder.MapFromServiceLocator<TestViewModel, TestView>(Contract);

        await Assert.That(returned).IsSameReferenceAs(builder);
        await Assert.That(locator.ResolveView<TestViewModel>(Contract)).IsTypeOf<TestView>();
        await Assert.That(locator.ResolveView<TestViewModel>()).IsNull();
    }

    /// <summary>A service-locator mapping for a view nobody registered fails loudly when it resolves.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapFromServiceLocator_Unregistered_ThrowsWhenResolved()
    {
        var locator = new DefaultViewLocator();
        _ = locator.CreateMappingBuilder().MapFromServiceLocator<TestViewModel, TestView>();

        await Assert.That(() => locator.ResolveView<TestViewModel>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// A service-locator mapping with a service contract picks the view registered under that contract, when the
    /// default view and the contracted view are both registered as <see cref="IViewFor{T}"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapFromServiceLocator_WithAServiceContract_ResolvesTheContractedRegistration()
    {
        var locator = new DefaultViewLocator();
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView());
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView(), Contract);
        var viewModel = new TestViewModel();

        _ = locator.CreateMappingBuilder()
            .MapFromServiceLocator<TestViewModel, IViewFor<TestViewModel>>()
            .MapFromServiceLocator<TestViewModel, IViewFor<TestViewModel>>(Contract, Contract);

        await Assert.That(locator.ResolveView(viewModel)).IsTypeOf<TestView>();
        await Assert.That(locator.ResolveView(viewModel, Contract)).IsTypeOf<OtherTestView>();
    }

    /// <summary>A service-locator mapping whose service contract nobody registered names that contract when it resolves.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MapFromServiceLocator_UnregisteredServiceContract_ThrowsNamingTheContract()
    {
        var locator = new DefaultViewLocator();
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView());
        _ = locator.CreateMappingBuilder().MapFromServiceLocator<TestViewModel, IViewFor<TestViewModel>>(Contract, Contract);

        await Assert.That(() => locator.ResolveView<TestViewModel>(Contract))
            .Throws<InvalidOperationException>()
            .WithMessageContaining($"'{Contract}'");
    }

    /// <summary>Resolves a view through the <see cref="IViewLocator"/> extension, as a caller holding the interface would.</summary>
    /// <param name="locator">The locator.</param>
    /// <param name="contract">The contract to resolve under.</param>
    /// <returns>The resolved view, or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IViewFor<TestViewModel>? ResolveThroughInterface(IViewLocator locator, string? contract) =>
        locator.ResolveView<TestViewModel>(contract);

    /// <summary>A view model the tests resolve views for.</summary>
    private sealed class TestViewModel
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>A view for <see cref="TestViewModel"/>.</summary>
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

    /// <summary>A second view for <see cref="TestViewModel"/>, so a test can tell which source supplied the view.</summary>
    private sealed class OtherTestView : IViewFor<TestViewModel>
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

    /// <summary>A view that is not a view for <see cref="TestViewModel"/>.</summary>
    private sealed class UnrelatedView : IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }

    /// <summary>A locator other than the default one, which resolves nothing itself.</summary>
    private sealed class EmptyLocator : IViewLocator
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
            where TViewModel : class => null;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IViewFor? ResolveView(object? viewModel, string? contract) => null;

        /// <inheritdoc/>
        [RequiresDynamicCode("Part of IViewLocator. This locator builds no type at run time.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IViewFor? ResolveViewUnsafe(object? viewModel, string? contract) => null;
    }
}
