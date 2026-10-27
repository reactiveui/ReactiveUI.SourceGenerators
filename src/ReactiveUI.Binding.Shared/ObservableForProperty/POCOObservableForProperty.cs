// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>Observes a property on a type that offers no change notification, at the lowest affinity.</summary>
/// <remarks>
/// The notification sequence emits one observed change on subscribe, which carries the sender and expression but no
/// value, then never emits or completes. Unless warnings are suppressed, the first observation of each type and
/// property writes a debug message.
/// </remarks>
public sealed class POCOObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>Tracks which (type, property) pairs have already emitted a POCO warning to avoid duplicate messages.</summary>
    private static readonly ConcurrentDictionary<ObservedPropertyKey, byte> HasWarned = new();

    /// <summary>Returns the fallback affinity for every type and property.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property name being observed.</param>
    /// <param name="beforeChanged">Whether before-change notifications are requested; not consulted.</param>
    /// <returns>The lowest positive affinity, so any other registered mechanism outranks it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="propertyName"/> is <see langword="null"/>.</exception>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(type);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        return BindingAffinity.Fallback;
    }

    /// <summary>Returns a sequence that emits one observed change on subscribe and then stays silent.</summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Whether before-change notifications are requested; not consulted.</param>
    /// <param name="suppressWarnings"><see langword="true"/> to skip the debug message written the first time a type and property are observed.</param>
    /// <returns>An observable that emits once and never completes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/>, <paramref name="expression"/> or <paramref name="propertyName"/> is <see langword="null"/>.</exception>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);

        if (!suppressWarnings)
        {
            WarnOnce(sender, propertyName);
        }

        // Emit the current value once, then never complete (so the binding stays alive).
        return new LeadSignal<IObservedChange<object, object?>>(
            ImmutableNeverSignal<IObservedChange<object, object?>>.Instance,
            new ObservedChange<object, object?>(sender, expression, default));
    }

    /// <summary>
    /// Emits a debug warning the first time a POCO property is observed, indicating that
    /// no change notifications will be sent after the initial value.
    /// </summary>
    /// <param name="sender">The object being observed.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WarnOnce(object sender, string propertyName)
    {
        var type = sender.GetType();
        if (!HasWarned.TryAdd(new(type, propertyName), 0))
        {
            return;
        }

        Debug.WriteLine(
            $"[ReactiveUI.Binding] Warning: The class {type.FullName} property {propertyName} is a POCO type and won't send change notifications, WhenAny will only return a single value!");
    }
}
