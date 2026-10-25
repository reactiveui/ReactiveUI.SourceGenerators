// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension members that import Splat-registered converters into a <see cref="ConverterService"/>.</summary>
public static class ConverterMigrationHelperMixins
{
    /// <summary>Provides ImportFrom extension members for <paramref name="converterService"/>.</summary>
    /// <param name="converterService">The converter service to import into.</param>
    extension(ConverterService converterService)
    {
        /// <summary>Registers the converters held by a Splat resolver with this service.</summary>
        /// <param name="resolver">The Splat resolver to import converters from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="converterService"/> or <paramref name="resolver"/> is null.
        /// </exception>
        /// <remarks>
        /// The import is a one-time copy; converters registered with the resolver afterwards are not picked up.
        /// </remarks>
        public void ImportFrom(
            IReadonlyDependencyResolver resolver)
        {
            ArgumentExceptionHelper.ThrowIfNull(converterService);
            ArgumentExceptionHelper.ThrowIfNull(resolver);

            var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

            foreach (var converter in typed)
            {
                converterService.TypedConverters.Register(converter);
            }

            foreach (var converter in fallback)
            {
                converterService.FallbackConverters.Register(converter);
            }

            foreach (var converter in setMethod)
            {
                converterService.SetMethodConverters.Register(converter);
            }
        }
    }
}
