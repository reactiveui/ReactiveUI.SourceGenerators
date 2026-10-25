// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Mixins;
#else
namespace ReactiveUI.Binding.Mixins;
#endif

/// <summary>Extension methods that bridge <see cref="IAppBuilder"/> to <see cref="IReactiveUIBindingBuilder"/> for fluent chaining.</summary>
/// <remarks>
/// These methods allow fluent chains such as:
/// <code>
/// RxBindingBuilder.CreateReactiveUIBindingBuilder()
///     .WithCoreServices()
///     .BuildApp();
/// </code>
/// Because <see cref="IAppBuilder.WithCoreServices"/> returns <see cref="IAppBuilder"/>, these
/// extension methods cast back to <see cref="IReactiveUIBindingBuilder"/> to continue chaining.
/// </remarks>
public static class BuilderMixins
{
    /// <summary>Message used when an <see cref="IAppBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.</summary>
    private const string NotBindingBuilderMessage =
        "The provided IAppBuilder is not an IReactiveUIBindingBuilder. "
        + "Ensure you are using the ReactiveUI.Binding builder pattern.";

    /// <summary>Builds the ReactiveUI.Binding application from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <returns>The configured application instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingInstance BuildApp(this IAppBuilder appBuilder)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.BuildApp();
    }

    /// <summary>Registers a platform-specific module from an <see cref="IAppBuilder"/>.</summary>
    /// <typeparam name="T">The type of the platform module.</typeparam>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="module">The platform module instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder WithPlatformModule<T>(this IAppBuilder appBuilder, T module)
        where T : IModule
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.WithPlatformModule(module);
    }

    /// <summary>Adds a custom registration action from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="configureAction">An action that receives the mutable dependency resolver.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder WithRegistration(
        this IAppBuilder appBuilder,
        Action<IMutableDependencyResolver> configureAction)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.WithRegistration(configureAction);
    }

    /// <summary>Registers a typed binding converter from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="converter">The converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder WithConverter(this IAppBuilder appBuilder, IBindingTypeConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.WithConverter(converter);
    }

    /// <summary>Registers a fallback binding converter from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="converter">The fallback converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder WithFallbackConverter(
        this IAppBuilder appBuilder,
        IBindingFallbackConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.WithFallbackConverter(converter);
    }

    /// <summary>Registers a set-method binding converter from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="converter">The set-method converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder WithSetMethodConverter(
        this IAppBuilder appBuilder,
        ISetMethodBindingConverter converter)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.WithSetMethodConverter(converter);
    }

    /// <summary>Configures the default view locator with explicit view-to-view-model mappings from an <see cref="IAppBuilder"/>.</summary>
    /// <param name="appBuilder">The app builder instance.</param>
    /// <param name="configure">An action that receives a <see cref="ViewMappingBuilder"/> for registering mappings.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="appBuilder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="appBuilder"/> is not an <see cref="IReactiveUIBindingBuilder"/>.
    /// </exception>
    public static IReactiveUIBindingBuilder ConfigureViewLocator(
        this IAppBuilder appBuilder,
        Action<ViewMappingBuilder> configure)
    {
        ArgumentExceptionHelper.ThrowIfNull(appBuilder);

        if (appBuilder is not IReactiveUIBindingBuilder reactiveUiBindingBuilder)
        {
            throw new InvalidOperationException(NotBindingBuilderMessage);
        }

        return reactiveUiBindingBuilder.ConfigureViewLocator(configure);
    }
}
