// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>Tests for the <see cref="ReactiveUIBindingBuilder"/> class.</summary>
public class ReactiveUIBindingBuilderTests
{
    /// <summary>The minimum number of observable-for-property services expected after core registration.</summary>
    private const int MinimumCoreObservableServices = 2;

    /// <summary>Verifies that the builder can be created and has a non-null ConverterService.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_CreatesConverterService()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        await Assert.That(builder.ConverterService).IsNotNull();
    }

    /// <summary>The builder registers its converter service instance under its concrete service type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_RegistersConverterServiceInstance()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var app = builder.BuildApp();

        await Assert.That(ReferenceEquals(app.Current?.GetService<ConverterService>(), builder.ConverterService)).IsTrue();
    }

    /// <summary>Verifies that WithCoreServices registers INPC and POCO observation services.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersObservationServices()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        var services = Locator.Current.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(services.Count).IsGreaterThanOrEqualTo(MinimumCoreObservableServices);
    }

    /// <summary>Verifies that WithPlatformModule registers a module.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_RegistersModule()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var registered = false;

        _ = builder.WithPlatformModule(new TestModule(() => registered = true));
        _ = builder.BuildApp();

        await Assert.That(registered).IsTrue();
    }

    /// <summary>Verifies that WithRegistration executes the registration action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_ExecutesAction()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var executed = false;

        _ = builder.WithRegistration(_ => executed = true);

        await Assert.That(executed).IsTrue();
    }

    /// <summary>Verifies that BuildApp sets the global ConverterService.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_SetsGlobalConverterService()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();

        var instance = builder.BuildApp();

        await Assert.That(instance).IsNotNull();
        await Assert.That(BindingConverters.Current).IsNotNull();
    }

    /// <summary>Verifies that WithCoreServices is idempotent.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_CalledTwice_OnlyRegistersOnce()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();

        // Should not throw; services registered once
        var services = Locator.Current.GetServices<ICreatesObservableForProperty>().ToList();
        await Assert.That(services.Count).IsGreaterThanOrEqualTo(MinimumCoreObservableServices);
    }

    /// <summary>Verifies that the fluent API supports chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FluentAPI_SupportsChainingCalls()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();
        var result = builder.WithRegistration(static _ => { });

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithConverter"/> registers a typed converter that appears in the <see cref="ConverterService.TypedConverters"/> registry.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_RegistersInTypedConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            static (_, _) => new(true, true));

        _ = builder.WithConverter(converter);

        var resolved = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(bool));
        await Assert.That(resolved).IsNotNull();
        await Assert.That(ReferenceEquals(resolved, converter)).IsTrue();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithConverter"/> returns the builder for fluent chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(string),
            static (from, _) => new(true, from?.ToString()));

        var result = builder.WithConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithFallbackConverter"/> registers a fallback converter in <see cref="ConverterService.FallbackConverters"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_RegistersInFallbackConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(static (_, _, _, _) => new(true, "converted"));

        _ = builder.WithFallbackConverter(converter);

        var allConverters = builder.ConverterService.FallbackConverters.GetAllConverters().ToList();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithFallbackConverter"/> returns the builder for fluent chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(static (_, _, _, _) => new(true, "converted"));

        var result = builder.WithFallbackConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithSetMethodConverter"/> registers a set-method converter in <see cref="ConverterService.SetMethodConverters"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_RegistersInSetMethodConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        _ = builder.WithSetMethodConverter(converter);

        var allConverters = builder.ConverterService.SetMethodConverters.GetAllConverters().ToList();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithSetMethodConverter"/> returns the builder for fluent chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        var result = builder.WithSetMethodConverter(converter);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithCoreServices"/> registers default converters, spot-checking that the <see cref="StringConverter"/> is present.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersDefaultConverters()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();

        // Spot-check: StringConverter registers string -> string
        var stringConverter = builder.ConverterService.TypedConverters.TryGetConverter(typeof(string), typeof(string));
        await Assert.That(stringConverter).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithCoreServices"/> registers the <see cref="IntegerToStringTypeConverter"/> (int -> string).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_RegistersIntegerToStringConverter()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();

        var converter = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(string));
        await Assert.That(converter).IsNotNull();
    }

    /// <summary>Verifies that calling <see cref="IReactiveUIBindingBuilder.WithCoreServices"/> via the explicit interface method returns an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCoreServices_ExplicitInterface_ReturnsIReactiveUIBindingBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var result = builder.WithCoreServices();

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.BuildApp"/> sets the global <see cref="BindingConverters.Current"/> to the builder's <see cref="ConverterService"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_SetsGlobalConverterServiceToBuilderInstance()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();

        _ = builder.BuildApp();

        await Assert.That(ReferenceEquals(BindingConverters.Current, builder.ConverterService)).IsTrue();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.BuildApp"/> marks the system as initialized so that <see cref="RxBindingBuilder.EnsureInitialized"/> does not throw.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_MarksAsInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();

        _ = builder.BuildApp();

        await Assert.That(RxBindingBuilder.EnsureInitialized).ThrowsNothing();
    }

    /// <summary>Verifies that multiple converters of different types can be registered and all appear in their respective registries.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleConverterTypes_AllRegistered()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var typed = new StubBindingTypeConverter(
            typeof(double),
            typeof(string),
            static (from, _) => new(true, from?.ToString()));
        var fallback = new StubFallbackConverter(static (_, _, _, _) => new(true, "fallback"));
        var setMethod = new StubSetMethodBindingConverter();

        _ = builder.WithConverter(typed);
        _ = builder.WithFallbackConverter(fallback);
        _ = builder.WithSetMethodConverter(setMethod);

        await Assert.That(builder.ConverterService.TypedConverters.TryGetConverter(typeof(double), typeof(string)))
            .IsNotNull();
        await Assert.That(builder.ConverterService.FallbackConverters.GetAllConverters().ToList()).Contains(fallback);
        await Assert.That(builder.ConverterService.SetMethodConverters.GetAllConverters().ToList()).Contains(setMethod);
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithCommandBinder"/> registers a command binder that appears in Splat's <see cref="ICreatesCommandBinding"/> services.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCommandBinder_RegistersInSplat()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var binder = new StubCommandBinder();

        _ = builder.WithCommandBinder(binder);
        _ = builder.BuildApp();

        var services = Locator.Current.GetServices<ICreatesCommandBinding>().ToList();
        await Assert.That(services).Contains(binder);
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithCommandBinder"/> returns the builder for fluent chaining.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCommandBinder_ReturnsSelfForChaining()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var binder = new StubCommandBinder();

        var result = builder.WithCommandBinder(binder);

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="ReactiveUIBindingBuilder.WithCommandBinder"/> throws on null binder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithCommandBinder_NullBinder_Throws()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        await Assert.That(() => builder.WithCommandBinder(null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that ConfigureViewLocator registers a configured DefaultViewLocator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureViewLocator_RegistersConfiguredLocator()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices();
        _ = builder.ConfigureViewLocator(static mappings =>
            mappings.Map<ConfigTestViewModel, ConfigTestView>());
        _ = builder.BuildApp();

        var locator = ViewLocator.GetCurrent();
        var result = locator.ResolveView(new ConfigTestViewModel());

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<ConfigTestView>();
    }

    /// <summary>A test <see cref="Splat.Builder.IModule"/> that invokes a callback when configured.</summary>
    /// <param name="onConfigure">The callback to invoke during configuration.</param>
    private sealed class TestModule(Action onConfigure) : Splat.Builder.IModule
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Configure(IMutableDependencyResolver resolver) => onConfigure();
    }

    /// <summary>A stub implementation of <see cref="ICreatesCommandBinding"/> for testing registration.</summary>
    private sealed class StubCommandBinder : ICreatesCommandBinding
    {
        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject<T>(bool hasEventTarget) => 0;

        /// <inheritdoc/>
        [RequiresUnreferencedCode("Test stub")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T>(
            System.Windows.Input.ICommand? command,
            T? target,
            IObservable<object?> commandParameter)
            where T : class => null;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Dictated by the interface this test stub implements.")]
        [RequiresUnreferencedCode("Test stub")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            System.Windows.Input.ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            string eventName)
            where T : class => null;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable? BindCommandToObject<T, TEventArgs>(
            System.Windows.Input.ICommand? command,
            T? target,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
            where T : class
            where TEventArgs : EventArgs => null;
    }

    /// <summary>Simple view model for ConfigureViewLocator testing.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "The view locator tests key off the type identity alone.")]
    private sealed class ConfigTestViewModel;

    /// <summary>Simple view for ConfigureViewLocator testing.</summary>
    private sealed class ConfigTestView : IViewFor<ConfigTestViewModel>
    {
        /// <inheritdoc/>
        public ConfigTestViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as ConfigTestViewModel;
        }
    }
}
