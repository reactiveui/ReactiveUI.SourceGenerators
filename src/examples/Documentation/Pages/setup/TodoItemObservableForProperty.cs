// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Observes <see cref="TodoItem"/> properties through <c>PropertyChanged</c> and counts each observation it serves.</summary>
[System.Diagnostics.DebuggerDisplay("TodoItemObservableForProperty: ObservationCount = {ObservationCount}")]
public sealed class TodoItemObservableForProperty : ICreatesObservableForProperty
{
    /// <summary>The score that outranks the observation the generator writes for a <c>PropertyChanged</c> type.</summary>
    private const int TodoItemAffinity = 100;

    /// <summary>Observes properties the way an ordinary <c>PropertyChanged</c> type is observed.</summary>
    private readonly INPCObservableForProperty _inner = new();

    /// <summary>Gets the number of property observations this provider has served.</summary>
    public int ObservationCount { get; private set; }

    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        type == typeof(TodoItem) && !beforeChanged ? TodoItemAffinity : 0;

    /// <inheritdoc/>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        ObservationCount++;
        return _inner.GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings);
    }
}
