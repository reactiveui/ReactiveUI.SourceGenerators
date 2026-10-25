// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Finds the registered invoker for the object a binding writes to.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewThreadInvokers
{
    /// <summary>The resolved invokers, or null while none have been resolved yet.</summary>
    private static IViewThreadInvoker[]? _invokers;

    /// <summary>Re-reads the registered invokers, for a host that registers one after its first binding.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Refresh() => Interlocked.Exchange(ref _invokers, null);

    /// <summary>Finds the first registered invoker that claims <paramref name="target"/>.</summary>
    /// <param name="target">The object a binding is about to write to.</param>
    /// <returns>The invoker, or null when <paramref name="target"/> is null or nothing claims it.</returns>
    public static IViewThreadInvoker? ForTarget(object? target)
    {
        if (target is null)
        {
            return null;
        }

        var invokers = Resolve();
        for (var i = 0; i < invokers.Length; i++)
        {
            if (invokers[i].Claims(target))
            {
                return invokers[i];
            }
        }

        return null;
    }

    /// <summary>Resolves the registered invokers once and keeps them.</summary>
    /// <returns>The registered invokers, empty when none is registered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IViewThreadInvoker[] Resolve()
    {
        var resolved = Volatile.Read(ref _invokers);
        if (resolved is not null)
        {
            return resolved;
        }

        IViewThreadInvoker[] built = [.. AppLocator.Current.GetServices<IViewThreadInvoker>()];

        // Two threads racing the first resolve both read the locator and one array is dropped.
        return Interlocked.CompareExchange(ref _invokers, built, null) ?? built;
    }
}
