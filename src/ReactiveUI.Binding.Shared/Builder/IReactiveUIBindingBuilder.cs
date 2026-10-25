// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>
/// Fluent builder that configures ReactiveUI.Binding services, converters, and platform modules
/// before building an application instance.
/// </summary>
/// <remarks>
/// <para>
/// The builder wraps <see cref="AppBuilder"/> to provide ReactiveUI.Binding-specific configuration
/// for property observation and binding services. Platform modules (WPF, WinForms, MAUI) are
/// registered via extension methods such as <c>WithWpf()</c>, <c>WithWinForms()</c>, and <c>WithMaui()</c>.
/// </para>
/// </remarks>
/// <seealso cref="IAppBuilder" />
public interface IReactiveUIBindingBuilder : IAppBuilder
{
    /// <summary>
    /// Registers the core ReactiveUI.Binding services (INPC/POCO observation, default converters, default view locator).
    /// Hides <see cref="IAppBuilder.WithCoreServices"/> to return <see cref="IReactiveUIBindingBuilder"/>
    /// for fluent chaining.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    new IReactiveUIBindingBuilder WithCoreServices();

    /// <summary>Registers a platform-specific module with the builder.</summary>
    /// <typeparam name="T">The type of the platform module. Must implement <see cref="IModule"/>.</typeparam>
    /// <param name="module">The platform module instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithPlatformModule<T>(T module)
        where T : IModule;

    /// <summary>Runs a registration action against the mutable dependency resolver.</summary>
    /// <param name="configureAction">An action that receives the mutable dependency resolver.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithRegistration(Action<IMutableDependencyResolver> configureAction);

    /// <summary>Registers a typed binding converter.</summary>
    /// <param name="converter">The converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithConverter(IBindingTypeConverter converter);

    /// <summary>Registers a fallback binding converter.</summary>
    /// <param name="converter">The fallback converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithFallbackConverter(IBindingFallbackConverter converter);

    /// <summary>Registers a set-method binding converter.</summary>
    /// <param name="converter">The set-method converter instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithSetMethodConverter(ISetMethodBindingConverter converter);

    /// <summary>Registers a custom command binder for binding <see cref="System.Windows.Input.ICommand"/> instances to UI controls.</summary>
    /// <param name="binder">The command binder instance to register.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder WithCommandBinder(ICreatesCommandBinding binder);

    /// <summary>Registers a default view locator holding the explicit view-to-view-model mappings.</summary>
    /// <param name="configure">An action that receives a <see cref="ViewMappingBuilder"/> for registering mappings.</param>
    /// <returns>The builder instance for chaining.</returns>
    IReactiveUIBindingBuilder ConfigureViewLocator(Action<ViewMappingBuilder> configure);

    /// <summary>Builds the application, publishes the converter service and marks ReactiveUI.Binding initialized.</summary>
    /// <returns>The configured application instance.</returns>
    IReactiveUIBindingInstance BuildApp();
}
