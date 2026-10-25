// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Static accessor for the current <see cref="IViewLocator"/> instance.</summary>
public static class ViewLocator
{
    /// <summary>Gets the current <see cref="IViewLocator"/> from the service locator.</summary>
    /// <returns>The current <see cref="IViewLocator"/> instance.</returns>
    /// <exception cref="ViewLocatorNotFoundException">
    /// Thrown when no <see cref="IViewLocator"/> is registered.
    /// </exception>
    public static IViewLocator GetCurrent() =>
        AppLocator.Current.GetService<IViewLocator>() ?? throw new ViewLocatorNotFoundException();
}
