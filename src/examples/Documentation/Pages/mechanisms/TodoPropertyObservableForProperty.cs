// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Observes one <see cref="TodoItem"/> property through <c>PropertyChanged</c>, bidding a chosen affinity, and
/// prints a line each time it serves an observation. It shows whether a bid beats the mechanism the generator picked.
/// </summary>
/// <param name="observedProperty">The name of the property this provider bids for.</param>
/// <param name="bid">The affinity this provider bids for that property.</param>
[System.Diagnostics.DebuggerDisplay("TodoPropertyObservableForProperty: {ObservedProperty}: Bid = {Bid}")]
public sealed class TodoPropertyObservableForProperty(string observedProperty, int bid) : ICreatesObservableForProperty
{
    /// <summary>Observes properties the way an ordinary <c>PropertyChanged</c> type is observed.</summary>
    private readonly INPCObservableForProperty _inner = new();

    /// <summary>Gets the name of the property this provider bids for.</summary>
    public string ObservedProperty => observedProperty;

    /// <summary>Gets the affinity this provider bids for its property.</summary>
    public int Bid => bid;

    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        !beforeChanged && type == typeof(TodoItem) && propertyName == observedProperty ? bid : 0;

    /// <inheritdoc/>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        Console.WriteLine($"{propertyName} is observed by the provider that bids {bid}");
        return _inner.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
    }
}
