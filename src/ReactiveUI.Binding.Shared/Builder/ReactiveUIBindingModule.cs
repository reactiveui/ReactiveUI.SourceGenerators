// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Registers the <see cref="ICreatesObservableForProperty"/> implementations for INotifyPropertyChanged and plain object properties.</summary>
/// <remarks>
/// Command binding for known patterns (Command property, event+Enabled, basic event) is handled
/// at compile time by source generator plugins; only custom <see cref="ICreatesCommandBinding"/>
/// implementations are resolved at runtime. Register the WPF, WinForms and MAUI modules separately with
/// <c>WithPlatformModule</c> or <c>UsingModule</c>.
/// </remarks>
public sealed class ReactiveUIBindingModule : IModule
{
    /// <summary>Registers the INPC and POCO property observation services with the resolver.</summary>
    /// <param name="resolver">The dependency resolver to configure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="resolver"/> is null.</exception>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        // Register core ICreatesObservableForProperty implementations
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () => new INPCObservableForProperty());
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () => new POCOObservableForProperty());
    }
}
