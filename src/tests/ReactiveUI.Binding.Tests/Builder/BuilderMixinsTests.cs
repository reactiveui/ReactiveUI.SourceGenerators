// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.Tests.TestModels;
using Splat.Builder;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>Tests for the <see cref="BuilderMixins"/> extension methods that bridge <see cref="IAppBuilder"/> to <see cref="IReactiveUIBindingBuilder"/>.</summary>
public class BuilderMixinsTests
{
    /// <summary>The value the stub converter returns.</summary>
    private const int ConvertedValue = 42;

    /// <summary>Verifies that <see cref="BuilderMixins.BuildApp"/> succeeds when the <see cref="IAppBuilder"/> is a valid <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_WithValidBuilder_Succeeds()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();

        var instance = ((IAppBuilder)builder).BuildApp();

        await Assert.That(instance).IsNotNull();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.BuildApp"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BuildApp_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.BuildApp();

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithPlatformModule{T}"/> delegates to the underlying <see cref="IReactiveUIBindingBuilder.WithPlatformModule{T}"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_DelegatesToBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var registered = false;

        var result = ((IAppBuilder)builder).WithPlatformModule(new TestModule(() => registered = true));

        _ = builder.BuildApp();

        await Assert.That(registered).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithPlatformModule{T}"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithPlatformModule_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.WithPlatformModule(new TestModule(static () => { }));

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithRegistration"/> delegates to the underlying <see cref="IReactiveUIBindingBuilder.WithRegistration"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_DelegatesToBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var executed = false;

        var result = ((IAppBuilder)builder).WithRegistration(_ => executed = true);

        await Assert.That(executed).IsTrue();
        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithRegistration"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithRegistration_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.WithRegistration(static _ => { });

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithConverter"/> registers a typed converter via the <see cref="IAppBuilder"/> extension method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            static (_, _) => new(true, true));

        var result = ((IAppBuilder)builder).WithConverter(converter);

        var resolved = builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(bool));

        await Assert.That(result).IsNotNull();
        await Assert.That(resolved).IsNotNull();
        await Assert.That(ReferenceEquals(resolved, converter)).IsTrue();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithConverter"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(int),
            typeof(bool),
            static (_, _) => new(true, true));

        var action = () => fakeBuilder.WithConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithFallbackConverter"/> registers a fallback converter via the <see cref="IAppBuilder"/> extension method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubFallbackConverter(static (_, _, _, _) => new(true, "converted"));

        var result = ((IAppBuilder)builder).WithFallbackConverter(converter);

        var allConverters = builder.ConverterService.FallbackConverters.GetAllConverters().ToList();

        await Assert.That(result).IsNotNull();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithFallbackConverter"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithFallbackConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();
        var converter = new StubFallbackConverter(static (_, _, _, _) => new(true, "converted"));

        var action = () => fakeBuilder.WithFallbackConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithSetMethodConverter"/> registers a set-method converter via the <see cref="IAppBuilder"/> extension method.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_RegistersConverterViaAppBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubSetMethodBindingConverter();

        var result = ((IAppBuilder)builder).WithSetMethodConverter(converter);

        var allConverters = builder.ConverterService.SetMethodConverters.GetAllConverters().ToList();

        await Assert.That(result).IsNotNull();
        await Assert.That(allConverters).Contains(converter);
    }

    /// <summary>Verifies that <see cref="BuilderMixins.WithSetMethodConverter"/> throws when the builder is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSetMethodConverter_WithNonReactiveUIBuilder_ThrowsInvalidOperationException()
    {
        var fakeBuilder = new FakeAppBuilder();
        var converter = new StubSetMethodBindingConverter();

        var action = () => fakeBuilder.WithSetMethodConverter(converter);

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that chaining multiple extension methods through <see cref="IAppBuilder"/> works correctly and returns the builder each time.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FluentChaining_ThroughIAppBuilder_Works()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        var converter = new StubBindingTypeConverter(
            typeof(string),
            typeof(int),
            static (_, _) => new(true, ConvertedValue));
        var fallbackConverter = new StubFallbackConverter(static (_, _, _, _) => new(true, "fallback"));
        var setConverter = new StubSetMethodBindingConverter();

        var result = ((IAppBuilder)builder)
            .WithConverter(converter)
            .WithFallbackConverter(fallbackConverter)
            .WithSetMethodConverter(setConverter);

        await Assert.That(result).IsNotNull();
        await Assert.That(builder.ConverterService.TypedConverters.TryGetConverter(typeof(string), typeof(int)))
            .IsNotNull();
        await Assert.That(builder.ConverterService.FallbackConverters.GetAllConverters().ToList())
            .Contains(fallbackConverter);
        await Assert.That(builder.ConverterService.SetMethodConverters.GetAllConverters().ToList())
            .Contains(setConverter);
    }

    /// <summary>Verifies that <see cref="BuilderMixins.ConfigureViewLocator"/> delegates to the underlying <see cref="IReactiveUIBindingBuilder.ConfigureViewLocator"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureViewLocator_DelegatesToBuilder()
    {
        RxBindingBuilder.ResetForTesting();
        var appBuilder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = appBuilder
            .WithCoreServices()
            .ConfigureViewLocator(static mappings =>
                mappings.Map<MixinTestViewModel, MixinTestView>())
            .BuildApp();

        var locator = ViewLocator.GetCurrent();
        var result = locator.ResolveView(new MixinTestViewModel());

        await Assert.That(result).IsNotNull();
    }

    /// <summary>Verifies that <see cref="BuilderMixins.ConfigureViewLocator"/> throws when the <see cref="IAppBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConfigureViewLocator_WithNonReactiveUIBuilder_Throws()
    {
        var fakeBuilder = new FakeAppBuilder();

        var action = () => fakeBuilder.ConfigureViewLocator(static _ => { });

        await Assert.That(action).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Simple view model for mixin testing.</summary>
    [SuppressMessage("Design", "SST1436:Empty type", Justification = "The tests only need a distinct view model type; members would not be exercised.")]
    private sealed class MixinTestViewModel;

    /// <summary>Simple view for mixin testing.</summary>
    private sealed class MixinTestView : IViewFor<MixinTestViewModel>
    {
        /// <inheritdoc/>
        public MixinTestViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as MixinTestViewModel;
        }
    }

    /// <summary>A fake <see cref="IAppBuilder"/> that does NOT implement <see cref="IReactiveUIBindingBuilder"/> to test the guard clauses in <see cref="BuilderMixins"/>.</summary>
    private sealed class FakeAppBuilder : IAppBuilder
    {
        /// <summary>Registers core services.</summary>
        /// <returns>The builder instance.</returns>
        public IAppBuilder WithCoreServices() => this;

        /// <summary>Uses the current Splat locator.</summary>
        /// <returns>The builder instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAppBuilder UseCurrentSplatLocator() => WithCoreServices();

        /// <summary>Registers a module.</summary>
        /// <typeparam name="T">The module type.</typeparam>
        /// <param name="registrationModule">The module instance.</param>
        /// <returns>The builder instance.</returns>
        public IAppBuilder UsingModule<T>(T registrationModule)
            where T : IModule => this;

        /// <summary>Registers a custom action.</summary>
        /// <param name="configureAction">The configuration action.</param>
        /// <returns>The builder instance.</returns>
        public IAppBuilder WithCustomRegistration(Action<IMutableDependencyResolver> configureAction) => this;

        /// <summary>Builds the application instance.</summary>
        /// <returns>The application instance.</returns>
        /// <exception cref="NotSupportedException">Always; the fake exists only to fail the builder type check.</exception>
        public IAppInstance Build() => throw new NotSupportedException();
    }

    /// <summary>A test <see cref="IModule"/> that invokes a callback when configured.</summary>
    private sealed class TestModule : IModule
    {
        /// <summary>A callback action invoked during the configuration process of the <see cref="TestModule"/>.</summary>
        private readonly Action _onConfigure;

        /// <summary>Initializes a new instance of the <see cref="TestModule"/> class.</summary>
        /// <param name="onConfigure">The callback to invoke during configuration.</param>
        public TestModule(Action onConfigure) => _onConfigure = onConfigure;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Configure(IMutableDependencyResolver resolver) => _onConfigure();
    }
}
