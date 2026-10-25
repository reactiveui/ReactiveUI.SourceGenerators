// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Reads converters that were registered with a Splat dependency resolver.</summary>
public static class ConverterMigrationHelper
{
    /// <summary>Collects every typed, fallback and set-method converter registered with a Splat resolver, without registering them anywhere.</summary>
    /// <param name="resolver">The Splat resolver to read.</param>
    /// <returns>The converters the resolver holds, in the order it returns them; a null registration is skipped.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="resolver"/> is null.</exception>
    public static ExtractedConverters ExtractConverters(IReadonlyDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        var typed = new List<IBindingTypeConverter>();
        foreach (var converter in resolver.GetServices<IBindingTypeConverter>())
        {
            if (converter is not null)
            {
                typed.Add(converter);
            }
        }

        var fallback = new List<IBindingFallbackConverter>();
        foreach (var converter in resolver.GetServices<IBindingFallbackConverter>())
        {
            if (converter is not null)
            {
                fallback.Add(converter);
            }
        }

        var setMethod = new List<ISetMethodBindingConverter>();
        foreach (var converter in resolver.GetServices<ISetMethodBindingConverter>())
        {
            if (converter is not null)
            {
                setMethod.Add(converter);
            }
        }

        return new(typed, fallback, setMethod);
    }
}
