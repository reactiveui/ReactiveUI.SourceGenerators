// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Registers the MAUI view thread invoker, the Visibility converters and, in the WinUI build, the dependency-property observer with the dependency resolver.</summary>
/// <remarks>
/// The converters are registered with the dependency resolver only; <see cref="ConverterMigrationHelperMixins"/>
/// imports them into a <see cref="ConverterService"/>.
/// </remarks>
public sealed class MauiBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

#if WINUI_TARGET
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () => new DependencyObjectObservableForProperty());
#endif
        resolver.RegisterLazySingleton<IBindingTypeConverter>(static () => new BooleanToVisibilityTypeConverter());
        resolver.RegisterLazySingleton<IBindingTypeConverter>(static () => new VisibilityToBooleanTypeConverter());

        resolver.RegisterLazySingleton<IViewThreadInvoker>(static () => new DispatcherViewThreadInvoker());
    }
}
