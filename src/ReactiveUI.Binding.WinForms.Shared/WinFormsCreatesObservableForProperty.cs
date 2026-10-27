// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Observes a WinForms <c>Component</c> property by subscribing to its public <c>{PropertyName}Changed</c> event through reflection.</summary>
/// <remarks>
/// Each notification carries no value; the observer reads the property. Before-change observation is not supported.
/// </remarks>
[RequiresUnreferencedCode(
    "Uses reflection to find and subscribe to {PropertyName}Changed events on WinForms components.")]
public class WinFormsCreatesObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>
    /// A concurrent dictionary cache used to store information about events corresponding
    /// to property changes on WinForms components.
    /// The key is a tuple consisting of a type and property name, and the value is the associated <see cref="EventInfo"/>
    /// if the {PropertyName}Changed event exists, or null if it does not.
    /// </summary>
    private static readonly ConcurrentDictionary<EventCacheKey, EventInfo?> EventInfoCache = new();

    /// <summary>Returns the WinForms event affinity when the component type has a public instance <c>{propertyName}Changed</c> event.</summary>
    /// <param name="type">The type that owns the property.</param>
    /// <param name="propertyName">The property name, without the <c>Changed</c> suffix.</param>
    /// <param name="beforeChanged"><see langword="true"/> always yields zero, since WinForms raises no before-change event.</param>
    /// <returns><see cref="BindingAffinity.WinFormsEvent"/> for a <c>Component</c> type with such an event; otherwise zero.</returns>
    [RequiresUnreferencedCode("Uses reflection to find {PropertyName}Changed events.")]
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged)
    {
        if (beforeChanged)
        {
            return 0;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            return 0;
        }

        return GetEventInfo(type, propertyName) is not null ? BindingAffinity.WinFormsEvent : 0;
    }

    /// <summary>Returns an observable that raises whenever <paramref name="sender"/> raises its <c>{propertyName}Changed</c> event.</summary>
    /// <param name="sender">The component to observe.</param>
    /// <param name="expression">The expression carried on each notification.</param>
    /// <param name="propertyName">The property name, without the <c>Changed</c> suffix.</param>
    /// <param name="beforeChanged">Ignored; notifications are always after the change.</param>
    /// <param name="suppressWarnings">Ignored.</param>
    /// <returns>An observable that adds the event handler on subscription and removes it on disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/> is null.</exception>
    /// <exception cref="ArgumentException">The sender's type has no such event.</exception>
    [RequiresUnreferencedCode("Uses reflection to subscribe to {PropertyName}Changed events.")]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        System.Linq.Expressions.Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);

        var ei = GetEventInfo(sender.GetType(), propertyName) ?? throw new ArgumentException(
            $"Could not find event {propertyName}Changed on type {sender.GetType().Name}",
            nameof(propertyName));

        return new AnonymousObservable<IObservedChange<object, object?>>(subj =>
        {
            var handler = new EventHandler((_, _) =>
                subj.OnNext(new ObservedChange<object, object?>(sender, expression, default)));

            ei.AddEventHandler(sender, handler);
            return Scope.Create<EventSubscription>(
                new(ei, sender, handler),
                static state => state.EventInfo.RemoveEventHandler(state.Sender, state.Handler));
        });
    }

    /// <summary>Retrieves the <see cref="EventInfo"/> for the specified {PropertyName}Changed event on the given type.</summary>
    /// <param name="type">The type to be inspected for the event.</param>
    /// <param name="propertyName">The name of the property whose corresponding event information is to be retrieved.</param>
    /// <returns>
    /// An <see cref="EventInfo"/> object if the {PropertyName}Changed event is found; otherwise, null.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EventInfo? GetEventInfo(Type type, string propertyName) =>
        EventInfoCache.GetOrAdd(
            new(type, propertyName),
            static key => key.Type.GetEvent(
                $"{key.PropertyName}Changed",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
}
