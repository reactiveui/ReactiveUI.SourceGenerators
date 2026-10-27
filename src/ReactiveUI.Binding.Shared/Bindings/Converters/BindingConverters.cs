// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Provides the process-wide <see cref="ConverterService"/> that generated and runtime bindings convert through.</summary>
/// <remarks>
/// <see cref="Current"/> starts empty. The builder's <c>BuildApp</c> replaces it with the service it configured,
/// which holds the built-in converters.
/// </remarks>
public static class BindingConverters
{
    /// <summary>The backing field for the current converter service instance.</summary>
    private static ConverterService _current = new();

    /// <summary>Gets the converter service in use.</summary>
    public static ConverterService Current => _current;

    /// <summary>Replaces the converter service in use.</summary>
    /// <param name="service">The converter service to use.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    public static void SetService(ConverterService service)
    {
        ArgumentExceptionHelper.ThrowIfNull(service);
        _current = service;
    }
}
