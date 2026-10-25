// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Registers the WPF dependency-property observer and view thread invoker with the dependency resolver.</summary>
/// <remarks>
/// WPF command binding is handled at compile time by the source generator.
/// </remarks>
public sealed class WpfBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () =>
            new DependencyObjectObservableForProperty());

        resolver.RegisterLazySingleton<IViewThreadInvoker>(static () => new DispatcherViewThreadInvoker());
    }
}
