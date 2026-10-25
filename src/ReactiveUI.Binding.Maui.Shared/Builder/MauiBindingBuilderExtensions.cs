// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui.Builder;
#else
namespace ReactiveUI.Binding.Maui.Builder;
#endif

/// <summary>MAUI-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class MauiBindingBuilderExtensions
{
    /// <summary>Provides WithMaui extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance; it must be an <see cref="IReactiveUIBindingBuilder"/>.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>Registers the MAUI module, which adds the view thread invoker, the Visibility converters and, in the WinUI build, dependency-property observation.</summary>
        /// <returns>The builder instance for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReactiveUIBindingBuilder WithMaui() =>
            ((IReactiveUIBindingBuilder)builder).WithMaui();
    }

    /// <summary>Provides WithMaui extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>Registers the MAUI module, which adds the view thread invoker, the Visibility converters and, in the WinUI build, dependency-property observation.</summary>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">The builder is null.</exception>
        public IReactiveUIBindingBuilder WithMaui()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new MauiBindingModule());
        }
    }
}
