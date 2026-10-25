// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Resolver-scoped entry points for creating <see cref="ReactiveUIBindingBuilder"/> instances.</summary>
public static class RxBindingBuilderMixins
{
    /// <summary>Provides CreateReactiveUIBindingBuilder extension members for <paramref name="resolver"/>.</summary>
    /// <param name="resolver">The dependency resolver to use.</param>
    extension(IMutableDependencyResolver resolver)
    {
        /// <summary>Creates a new <see cref="ReactiveUIBindingBuilder"/> that registers into the specified resolver.</summary>
        /// <returns>A new builder instance that reads services from the resolver when it is also an <see cref="IReadonlyDependencyResolver"/>, and from the current locator otherwise.</returns>
        /// <exception cref="ArgumentNullException">The resolver is null.</exception>
        public ReactiveUIBindingBuilder CreateReactiveUIBindingBuilder()
        {
            ArgumentExceptionHelper.ThrowIfNull(resolver);

            return new(resolver, resolver as IReadonlyDependencyResolver ?? AppLocator.Current);
        }
    }
}
