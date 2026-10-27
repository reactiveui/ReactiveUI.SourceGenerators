// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia;
#else
namespace ReactiveUI.Binding.Avalonia;
#endif

/// <summary>Registers the Avalonia property observer, command binder and view thread invoker with the dependency resolver.</summary>
public sealed class AvaloniaBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () => new AvaloniaObjectObservableForProperty());

        resolver.RegisterLazySingleton<ICreatesCommandBinding>(static () => new AvaloniaCreatesCommandBinding());

        resolver.RegisterLazySingleton<IViewThreadInvoker>(static () => AvaloniaViewThreadInvoker.Instance);
    }
}
