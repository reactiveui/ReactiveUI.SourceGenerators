// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.CommandBinding;
#else
namespace ReactiveUI.Binding.CommandBinding;
#endif

/// <summary>
/// Service that resolves the best <see cref="ICreatesCommandBinding"/> for a given
/// control type using affinity scoring. Follows the same resolution pattern as
/// property observation via <see cref="ICreatesObservableForProperty"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBinderService
{
    /// <summary>Gets the highest-affinity <see cref="ICreatesCommandBinding"/> registered for the specified control type.</summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns>The best binder; the first registered wins a tie. Null when every registered binder returns an affinity of zero or less.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    public static ICreatesCommandBinding? GetBinder<T>(bool hasEventTarget)
    {
        var binders = Locator.Current.GetServices<ICreatesCommandBinding>();

        ICreatesCommandBinding? bestBinder = null;
        var bestAffinity = 0;

        foreach (var binder in binders)
        {
            var affinity = binder.GetAffinityForObject<T>(hasEventTarget);
            if (affinity <= bestAffinity)
            {
                continue;
            }

            bestAffinity = affinity;
            bestBinder = binder;
        }

        return bestBinder;
    }
}
