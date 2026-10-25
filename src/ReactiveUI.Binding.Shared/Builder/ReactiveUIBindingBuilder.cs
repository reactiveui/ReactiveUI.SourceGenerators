// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Configures ReactiveUI.Binding services, converters and platform modules on a Splat <see cref="AppBuilder"/>.</summary>
[DebuggerDisplay("ReactiveUIBindingBuilder: CoreServicesRegistered = {_coreRegistered}")]
public sealed class ReactiveUIBindingBuilder : AppBuilder, IReactiveUIBindingBuilder, IReactiveUIBindingInstance
{
    /// <summary>Tracks whether core services are registered, so a repeat call registers nothing.</summary>
    private bool _coreRegistered;

    /// <summary>Initializes a new instance of the <see cref="ReactiveUIBindingBuilder"/> class, initializing Splat on the resolver and registering <see cref="ConverterService"/> with it.</summary>
    /// <param name="resolver">The dependency resolver to configure.</param>
    /// <param name="current">The resolver that reads the configured services; may be null.</param>
    public ReactiveUIBindingBuilder(IMutableDependencyResolver resolver, IReadonlyDependencyResolver? current)
        : base(resolver, current)
    {
        CurrentMutable.InitializeSplat();

        // Register the ConverterService instance so it's accessible to registrations
        CurrentMutable.RegisterConstant(ConverterService);
    }

    /// <summary>Gets the converter service used for binding type conversions.</summary>
    /// <remarks>
    /// This service provides access to three specialized registries:
    /// <list type="bullet">
    /// <item><description><see cref="ConverterService.TypedConverters"/> - For exact type-pair converters.</description></item>
    /// <item><description><see cref="ConverterService.FallbackConverters"/> - For fallback converters with runtime type checking.</description></item>
    /// <item><description><see cref="ConverterService.SetMethodConverters"/> - For set-method converters.</description></item>
    /// </list>
    /// Use the <c>WithConverter*</c> methods to register converters during application initialization.
    /// </remarks>
    public ConverterService ConverterService { get; } = new();

    /// <summary>Registers a platform-specific module with the builder.</summary>
    /// <typeparam name="T">The type of the platform module. Must implement <see cref="IModule"/>.</typeparam>
    /// <param name="module">The platform module instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is null.</exception>
    public IReactiveUIBindingBuilder WithPlatformModule<T>(T module)
        where T : IModule
    {
        ArgumentExceptionHelper.ThrowIfNull(module);
        _ = UsingModule(module);
        return this;
    }

    /// <summary>Runs a registration action against the mutable dependency resolver immediately.</summary>
    /// <param name="configureAction">An action that receives the mutable dependency resolver.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configureAction"/> is null.</exception>
    public IReactiveUIBindingBuilder WithRegistration(Action<IMutableDependencyResolver> configureAction)
    {
        ArgumentExceptionHelper.ThrowIfNull(configureAction);
        configureAction(CurrentMutable);
        return this;
    }

    /// <summary>Registers a typed binding converter.</summary>
    /// <param name="converter">The converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
    public IReactiveUIBindingBuilder WithConverter(IBindingTypeConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.TypedConverters.Register(converter);
        return this;
    }

    /// <summary>Registers a fallback binding converter.</summary>
    /// <param name="converter">The fallback converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
    public IReactiveUIBindingBuilder WithFallbackConverter(IBindingFallbackConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.FallbackConverters.Register(converter);
        return this;
    }

    /// <summary>Registers a set-method binding converter.</summary>
    /// <param name="converter">The set-method converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
    public IReactiveUIBindingBuilder WithSetMethodConverter(ISetMethodBindingConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(converter);
        ConverterService.SetMethodConverters.Register(converter);
        return this;
    }

    /// <summary>Registers a custom command binder for binding commands to UI controls.</summary>
    /// <param name="binder">The command binder instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="binder"/> is null.</exception>
    public IReactiveUIBindingBuilder WithCommandBinder(ICreatesCommandBinding binder)
    {
        ArgumentExceptionHelper.ThrowIfNull(binder);
        CurrentMutable.RegisterLazySingleton(() => binder);
        return this;
    }

    /// <summary>Creates a <see cref="DefaultViewLocator"/> holding the explicit mappings and registers it as the <see cref="IViewLocator"/>.</summary>
    /// <param name="configure">An action that receives a <see cref="ViewMappingBuilder"/> for registering mappings.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is null.</exception>
    public IReactiveUIBindingBuilder ConfigureViewLocator(Action<ViewMappingBuilder> configure)
    {
        ArgumentExceptionHelper.ThrowIfNull(configure);

        var locator = new DefaultViewLocator();
        var mappingBuilder = new ViewMappingBuilder(locator);
        configure(mappingBuilder);
        CurrentMutable.RegisterConstant<IViewLocator>(locator);
        return this;
    }

    /// <summary>Registers the default converters, the INPC and POCO observation services and the default view locator; calling it again registers nothing more.</summary>
    /// <returns>The builder instance for chaining.</returns>
    public override IAppBuilder WithCoreServices()
    {
        if (!_coreRegistered)
        {
            // Register all standard converters to the ConverterService
            DefaultConverterRegistration.RegisterDefaults(ConverterService);

            // Register core observation services
            _ = WithPlatformModule(new ReactiveUIBindingModule());

            // Register default view locator
            CurrentMutable.RegisterLazySingleton<IViewLocator>(static () => new DefaultViewLocator());

            _coreRegistered = true;
        }

        return this;
    }

    /// <inheritdoc/>
    IReactiveUIBindingBuilder IReactiveUIBindingBuilder.WithCoreServices() =>
        (IReactiveUIBindingBuilder)WithCoreServices();

    /// <summary>Builds the application, publishes <see cref="ConverterService"/> through <see cref="BindingConverters"/> and marks ReactiveUI.Binding initialized.</summary>
    /// <returns>The configured application instance.</returns>
    /// <exception cref="InvalidOperationException">The build produced no usable instance.</exception>
    public IReactiveUIBindingInstance BuildApp()
    {
        var appInstance = (IReactiveUIBindingInstance)Build();

        ThrowIfCurrentNull(appInstance);

        // Set the global converter service
        BindingConverters.SetService(ConverterService);

        // Mark as initialized
        RxBindingBuilder.MarkAsInitialized();

        return appInstance;
    }

    /// <summary>
    /// Throws if the app instance's Current resolver is null after building.
    /// This is a defensive guard that should never be hit in practice because
    /// <see cref="AppBuilder.Build"/> always sets Current.
    /// </summary>
    /// <param name="appInstance">The built app instance to validate.</param>
    /// <exception cref="InvalidOperationException"><paramref name="appInstance"/> has no Current resolver, so the build did not produce a usable instance.</exception>
    [ExcludeFromCodeCoverage]
    private static void ThrowIfCurrentNull(IReactiveUIBindingInstance appInstance)
    {
        if (appInstance.Current is not null)
        {
            return;
        }

        throw new InvalidOperationException("Failed to create ReactiveUIBindingInstance instance");
    }
}
