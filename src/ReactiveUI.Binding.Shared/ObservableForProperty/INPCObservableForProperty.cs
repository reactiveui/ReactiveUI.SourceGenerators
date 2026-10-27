// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>Observes properties of objects that implement <see cref="INotifyPropertyChanged"/> or <see cref="INotifyPropertyChanging"/>.</summary>
public class INPCObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>The affinity returned when the target type implements the relevant notification interface.</summary>
    private static readonly int SupportedAffinity = BindingAffinity.Explicit;

    /// <summary>
    /// Returns the explicit affinity when the type implements <see cref="INotifyPropertyChanging"/> (before-change)
    /// or <see cref="INotifyPropertyChanged"/> (after-change); otherwise, zero.
    /// </summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property name being observed; not consulted.</param>
    /// <param name="beforeChanged">Whether before-change notifications are requested.</param>
    /// <returns>The affinity score, or zero when the type lacks the required interface.</returns>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        (beforeChanged ? typeof(INotifyPropertyChanging) : typeof(INotifyPropertyChanged)).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
            ? SupportedAffinity
            : 0;

    /// <summary>
    /// Returns a sequence that emits when the sender raises a notification for the property, or a sequence that
    /// never emits when the sender implements neither interface.
    /// </summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property; an indexer matches the name followed by <c>[]</c>.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Whether to observe <see cref="INotifyPropertyChanging"/>; a sender that lacks it is observed through <see cref="INotifyPropertyChanged"/>.</param>
    /// <param name="suppressWarnings">Not used.</param>
    /// <returns>An observable of observed changes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        // Before-change uses INotifyPropertyChanging; after-change (and the before-change fallback for
        // types that only implement INotifyPropertyChanged) uses PropertyChanged. A single fused sink
        // wires the event, filters by name, and emits the observed change directly.
        if ((beforeChanged && sender is INotifyPropertyChanging) || sender is INotifyPropertyChanged)
        {
            var expectedName = expression.NodeType == ExpressionType.Index ? $"{propertyName}[]" : propertyName;
            return new NotifyPropertyChangedObservable(sender, expression, expectedName, beforeChanged);
        }

        return ImmutableNeverSignal<IObservedChange<object, object?>>.Instance;
    }
}
