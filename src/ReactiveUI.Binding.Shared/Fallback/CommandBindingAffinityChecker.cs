// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Reports whether a registered <see cref="ICreatesCommandBinding"/> has a higher affinity for a control type than the
/// mechanism the generator selected, so generated code can defer to it at runtime.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBindingAffinityChecker
{
    /// <summary>Returns <see langword="true"/> if a registered <see cref="ICreatesCommandBinding"/> outranks <paramref name="generatedAffinity"/>.</summary>
    /// <typeparam name="T">The control type being bound to.</typeparam>
    /// <param name="generatedAffinity">The affinity of the source generator's selected plugin.</param>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns><see langword="true"/> if a user plugin should override the generated binding; the registrations are read from the service locator on every call.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    public static bool HasHigherAffinityPlugin<T>(int generatedAffinity, bool hasEventTarget)
    {
        foreach (var plugin in Locator.Current.GetServices<ICreatesCommandBinding>())
        {
            if (plugin.GetAffinityForObject<T>(hasEventTarget) > generatedAffinity)
            {
                return true;
            }
        }

        return false;
    }
}
