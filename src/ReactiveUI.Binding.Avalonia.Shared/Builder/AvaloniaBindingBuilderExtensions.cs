// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia.Builder;
#else
namespace ReactiveUI.Binding.Avalonia.Builder;
#endif

/// <summary>Avalonia-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class AvaloniaBindingBuilderExtensions
{
    /// <summary>Provides WithAvalonia extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance; it must be an <see cref="IReactiveUIBindingBuilder"/>.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>Registers the Avalonia module, which adds Avalonia property observation, command binding and the view thread invoker.</summary>
        /// <returns>The builder instance for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReactiveUIBindingBuilder WithAvalonia() =>
            ((IReactiveUIBindingBuilder)builder).WithAvalonia();
    }

    /// <summary>Provides WithAvalonia extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>Registers the Avalonia module, which adds Avalonia property observation, command binding and the view thread invoker.</summary>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">The builder is null.</exception>
        public IReactiveUIBindingBuilder WithAvalonia()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new AvaloniaBindingModule());
        }
    }
}
