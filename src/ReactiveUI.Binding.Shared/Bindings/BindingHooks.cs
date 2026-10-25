// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Lets a registered <see cref="IPropertyBindingHook"/> inspect, or refuse, a binding as it is created.</summary>
/// <remarks>
/// <para>
/// A hook is consulted once per binding, before anything is wired, and a hook that returns false cancels the
/// binding outright. Hooks are registered with the service locator, so the set is only known at run time.
/// </para>
/// <para>
/// The locator is read on first use and the hooks are cached until <see cref="Refresh"/> is called.
/// <see cref="Any"/> is the guard a caller tests first, so a binding builds the property-chain closures
/// <see cref="ShouldBind"/> needs only when a hook exists.
/// </para>
/// </remarks>
public static class BindingHooks
{
    /// <summary>Guards <see cref="_hooks"/> while it is (re)resolved.</summary>
    /// <remarks>
    /// Only the first resolve after a refresh takes it. Holding it while the locator is read stops a resolve
    /// that races <see cref="Refresh"/> from publishing the set that refresh dropped.
    /// </remarks>
    private static readonly Lock Gate = new();

    /// <summary>The resolved hooks, or null while none has been resolved yet.</summary>
    private static IPropertyBindingHook[]? _hooks;

    /// <summary>Gets a value indicating whether any hook was registered when the hooks were last read.</summary>
    /// <remarks>
    /// Test this before building the arguments to <see cref="ShouldBind"/>: the closures they need cost more
    /// than the check, and are wasted whenever the answer is false.
    /// </remarks>
    public static bool Any => Resolve().Length > 0;

    /// <summary>Discards the cached hooks so the next use reads them from the service locator again.</summary>
    public static void Refresh()
    {
        lock (Gate)
        {
            _hooks = null;
        }
    }

    /// <summary>Asks the registered hooks in turn whether this binding may be created, stopping at the first refusal.</summary>
    /// <param name="source">The source object, typically the view model; may be null.</param>
    /// <param name="target">The target object, typically the view.</param>
    /// <param name="getSourceProperties">Reads the current source-side values.</param>
    /// <param name="getTargetProperties">Reads the current target-side values.</param>
    /// <param name="direction">Which way the binding runs.</param>
    /// <returns><see langword="true"/> when the binding may proceed, including when no hook is registered; <see langword="false"/> when a hook refused it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="target"/>, <paramref name="getSourceProperties"/> or <paramref name="getTargetProperties"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool ShouldBind(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getSourceProperties,
        Func<IObservedChange<object, object>[]> getTargetProperties,
        BindingDirection direction)
    {
        ArgumentExceptionHelper.ThrowIfNull(target);
        ArgumentExceptionHelper.ThrowIfNull(getSourceProperties);
        ArgumentExceptionHelper.ThrowIfNull(getTargetProperties);

        var hooks = Resolve();
        for (var i = 0; i < hooks.Length; i++)
        {
            if (!hooks[i].ExecuteHook(source, target, getSourceProperties, getTargetProperties, direction))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Resolves the registered hooks once and keeps them.</summary>
    /// <returns>The registered hooks, empty when none is registered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IPropertyBindingHook[] Resolve()
    {
        var resolved = Volatile.Read(ref _hooks);
        if (resolved is not null)
        {
            return resolved;
        }

        lock (Gate)
        {
            _hooks ??= [.. AppLocator.Current.GetServices<IPropertyBindingHook>()];
            return _hooks;
        }
    }
}
