// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf.Builder;
#else
namespace ReactiveUI.Binding.Wpf.Builder;
#endif

/// <summary>WPF-specific extensions for the ReactiveUI.Binding builder.</summary>
public static class WpfBindingBuilderExtensions
{
    /// <summary>Provides WithWpf extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance; it must be an <see cref="IReactiveUIBindingBuilder"/>.</param>
    extension(IAppBuilder builder)
    {
        /// <summary>Registers the WPF module, which adds dependency-property observation and the dispatcher view thread invoker.</summary>
        /// <returns>The builder instance for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReactiveUIBindingBuilder WithWpf() =>
            ((IReactiveUIBindingBuilder)builder).WithWpf();
    }

    /// <summary>Provides WithWpf extension members for <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder instance.</param>
    extension(IReactiveUIBindingBuilder builder)
    {
        /// <summary>Registers the WPF module, which adds dependency-property observation and the dispatcher view thread invoker.</summary>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">The builder is null.</exception>
        public IReactiveUIBindingBuilder WithWpf()
        {
            ArgumentExceptionHelper.ThrowIfNull(builder);

            return builder.WithPlatformModule(new WpfBindingModule());
        }
    }
}
