// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Observes <see cref="StorageConnection.State"/> through the plain <see cref="StorageConnection.StateChanged"/>
/// event. The connection implements no notification interface, so no built-in mechanism can observe it.
/// </summary>
public sealed class StorageConnectionObservableForProperty : ICreatesObservableForProperty
{
    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        !beforeChanged && type == typeof(StorageConnection) && propertyName == nameof(StorageConnection.State)
            ? BindingAffinity.WinFormsEvent
            : 0;

    /// <inheritdoc/>
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings)
    {
        if (beforeChanged || sender is not StorageConnection connection)
        {
            return ImmutableNeverSignal<IObservedChange<object, object?>>.Instance;
        }

        Console.WriteLine("The connection state is observed by the StateChanged provider");

        return Signal.FromEventPattern(handler => connection.StateChanged += handler, handler => connection.StateChanged -= handler)
            .Select(_ => new ObservedChange<object, object?>(connection, expression, connection.State));
    }
}
