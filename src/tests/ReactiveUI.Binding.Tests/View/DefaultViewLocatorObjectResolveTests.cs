// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestExecutors;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.View;

/// <summary>
/// Covers resolving a view model held as an <see cref="object"/>: the ahead-of-time safe <c>ResolveView</c>, which
/// never asks the service locator, and <c>ResolveViewUnsafe</c>, which adds the service locator step.
/// </summary>
[NotInParallel]
[TestExecutor<BindingBuilderTestExecutor>]
public class DefaultViewLocatorObjectResolveTests
{
    /// <summary>The contract the contract-specific cases register and resolve under.</summary>
    private const string Contract = "compact";

    /// <summary>A view model of a value type, which <c>IViewFor&lt;T&gt;</c> cannot be closed over.</summary>
    private const int ValueTypeViewModel = 42;

    /// <summary>ResolveView finds a view from the generated lookup and sets its view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_GeneratedLookup_ReturnsTheViewWithItsViewModel()
    {
        var generatedView = new TestView();
        DefaultViewLocator.SetGeneratedViewDispatch((viewModel, contract) => viewModel is TestViewModel && contract.Length == 0 ? generatedView : null);
        object viewModel = new TestViewModel();

        var result = new DefaultViewLocator().ResolveView(viewModel);

        await Assert.That(result).IsSameReferenceAs(generatedView);
        await Assert.That(result!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>ResolveView finds a mapping keyed by the view model's runtime type, under each contract.</summary>
    /// <param name="contract">The contract the mapping is added and resolved under.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null)]
    [Arguments(Contract)]
    public async Task ResolveView_MappingForTheRuntimeType_ReturnsTheView(string? contract)
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>(contract);
        object viewModel = new TestViewModel();

        var result = locator.ResolveView(viewModel, contract);

        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(result!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>ResolveView does not use a mapping added under another contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_MappingUnderAnotherContract_ReturnsNull()
    {
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>(Contract);
        object viewModel = new TestViewModel();

        var result = locator.ResolveView(viewModel);

        await Assert.That(result).IsNull();
    }

    /// <summary>ResolveView never asks the service locator, so a view registered only there is not built.</summary>
    /// <param name="contract">The contract the view is registered and resolved under.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null)]
    [Arguments(Contract)]
    public async Task ResolveView_ServiceLocatorOnlyRegistration_ReturnsNullWithoutAskingTheServiceLocator(string? contract)
    {
        var factoryCalls = 0;
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(
            () =>
            {
                factoryCalls++;
                return new TestView();
            },
            contract);
        object viewModel = new TestViewModel();

        var result = new DefaultViewLocator().ResolveView(viewModel, contract);

        await Assert.That(result).IsNull();
        await Assert.That(factoryCalls).IsEqualTo(0);
    }

    /// <summary>ResolveViewUnsafe finds a view registered only in the service locator, under each contract.</summary>
    /// <param name="contract">The contract the view is registered and resolved under.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(null)]
    [Arguments(Contract)]
    public async Task ResolveViewUnsafe_ServiceLocatorOnlyRegistration_ReturnsTheView(string? contract)
    {
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView(), contract);
        object viewModel = new TestViewModel();

        var result = new DefaultViewLocator().ResolveViewUnsafe(viewModel, contract);

        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(result!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>ResolveViewUnsafe does not use a service locator registration under another contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_RegistrationUnderAnotherContract_ReturnsNull()
    {
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView(), Contract);
        object viewModel = new TestViewModel();

        var result = new DefaultViewLocator().ResolveViewUnsafe(viewModel, null);

        await Assert.That(result).IsNull();
    }

    /// <summary>ResolveViewUnsafe tries the mappings before the service locator, and does not build the registered view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_MappingAndRegistration_PrefersTheMapping()
    {
        var factoryCalls = 0;
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(() =>
        {
            factoryCalls++;
            return new OtherTestView();
        });
        var locator = new DefaultViewLocator();
        locator.Map<TestViewModel, TestView>();
        object viewModel = new TestViewModel();

        var result = locator.ResolveViewUnsafe(viewModel, null);

        await Assert.That(result).IsTypeOf<TestView>();
        await Assert.That(factoryCalls).IsEqualTo(0);
    }

    /// <summary>ResolveViewUnsafe tries the generated lookup first.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_GeneratedLookup_ReturnsTheGeneratedView()
    {
        var generatedView = new TestView();
        DefaultViewLocator.SetGeneratedViewDispatch((viewModel, _) => viewModel is TestViewModel ? generatedView : null);
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new OtherTestView());
        object viewModel = new TestViewModel();

        var result = new DefaultViewLocator().ResolveViewUnsafe(viewModel, null);

        await Assert.That(result).IsSameReferenceAs(generatedView);
        await Assert.That(result!.ViewModel).IsSameReferenceAs(viewModel);
    }

    /// <summary>ResolveViewUnsafe answers null for a null view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_NullViewModel_ReturnsNull()
    {
        var result = new DefaultViewLocator().ResolveViewUnsafe(null, null);

        await Assert.That(result).IsNull();
    }

    /// <summary>ResolveViewUnsafe answers null when nothing maps or registers a view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_NothingMappedOrRegistered_ReturnsNull()
    {
        var result = new DefaultViewLocator().ResolveViewUnsafe(new TestViewModel(), null);

        await Assert.That(result).IsNull();
    }

    /// <summary>ResolveViewUnsafe answers null for a value type, which <c>IViewFor&lt;T&gt;</c> cannot be closed over.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafe_ValueTypeViewModel_ReturnsNull()
    {
        var result = new DefaultViewLocator().ResolveViewUnsafe(ValueTypeViewModel, null);

        await Assert.That(result).IsNull();
    }

    /// <summary>The ResolveViewUnsafe extension resolves under the default contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafeExtension_UsesTheDefaultContract()
    {
        AppLocator.CurrentMutable.Register<IViewFor<TestViewModel>>(static () => new TestView());
        DefaultViewLocator locator = new();
        object viewModel = new TestViewModel();

        var result = locator.ResolveViewUnsafe(viewModel);

        await Assert.That(result).IsTypeOf<TestView>();
    }

    /// <summary>The ResolveViewUnsafe extension throws for a null locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewUnsafeExtension_NullLocator_Throws()
    {
        IViewLocator locator = null!;
        object viewModel = new TestViewModel();

        await Assert.That(() => locator.ResolveViewUnsafe(viewModel)).Throws<ArgumentNullException>();
    }

    /// <summary>The ResolveView extension for an object throws for a null locator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveViewExtension_NullLocator_Throws()
    {
        IViewLocator locator = null!;
        object viewModel = new TestViewModel();

        await Assert.That(() => locator.ResolveView(viewModel)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// ResolveView warns when it finds no generated or mapped view, naming ResolveViewUnsafe for a view registered only in
    /// the service locator. ResolveViewUnsafe, which asks the service locator itself, gives no such warning.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResolveView_NoGeneratedOrMappedView_LogsAWarning()
    {
        var logger = new CapturingLogger();
        AppLocator.CurrentMutable.RegisterConstant<ILogManager>(new CapturingLogManager(logger));
        object viewModel = new TestViewModel();
        var locator = new DefaultViewLocator();

        _ = locator.ResolveViewUnsafe(viewModel, null);
        var afterUnsafe = logger.Warnings.Count;
        _ = locator.ResolveView(viewModel);

        await Assert.That(afterUnsafe).IsEqualTo(0);
        await Assert.That(logger.Warnings).HasSingleItem();
        await Assert.That(logger.Warnings[0]).Contains("TestViewModel");
        await Assert.That(logger.Warnings[0]).Contains("ResolveViewUnsafe");
    }

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

    /// <summary>Records the warnings written to it.</summary>
    private sealed class CapturingLogger : ILogger
    {
        /// <summary>Gets the warning messages, in the order they were written.</summary>
        public List<string> Warnings { get; } = [];

        /// <inheritdoc/>
        public LogLevel Level => LogLevel.Debug;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string message, LogLevel logLevel) => Record(message, logLevel);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Exception exception, string message, LogLevel logLevel) => Record(message, logLevel);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string message, Type type, LogLevel logLevel) => Record(message, logLevel);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Exception exception, string message, Type type, LogLevel logLevel) => Record(message, logLevel);

        /// <summary>Keeps a message written at warning level.</summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The level it was written at.</param>
        private void Record(string message, LogLevel logLevel)
        {
            if (logLevel == LogLevel.Warn)
            {
                Warnings.Add(message);
            }
        }
    }

    /// <summary>Hands every type the same capturing logger.</summary>
    /// <param name="logger">The logger.</param>
    private sealed class CapturingLogManager(CapturingLogger logger) : ILogManager
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IFullLogger GetLogger(Type type) => new WrappingFullLogger(logger);
    }

    /// <summary>A second view for <see cref="TestViewModel"/>, so a test can tell which step supplied the view.</summary>
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
}
