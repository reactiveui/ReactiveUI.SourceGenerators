// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>Shows how <see cref="EventObservable{T}"/> observes a class that announces a change through a plain event.</summary>
public static class EventNotificationExamples
{
    /// <summary>The address of the storage service.</summary>
    private const string StorageEndpoint = "https://storage.example.test";

    /// <summary>Observes whether a storage connection is open through its plain event; a repeated value is dropped.</summary>
    public static void ObserveConnectionOpenWithoutRepeats()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        EventObservable<bool> isOpen = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State == ConnectionState.Connected,
            true);

        using (isOpen.Subscribe(Console.WriteLine))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
            connection.State = ConnectionState.Disconnected;
        }

        connection.State = ConnectionState.Connected;

        // Output:
        // False
        // True
        // False
    }

    /// <summary>Observes whether a storage connection is open through its plain event; every event delivers a value.</summary>
    public static void ObserveConnectionOpenOnEveryEvent()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        EventObservable<bool> isOpen = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State == ConnectionState.Connected,
            false);

        using (isOpen.Subscribe(Console.WriteLine))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
            connection.State = ConnectionState.Disconnected;
        }

        // Output:
        // False
        // False
        // True
        // False
    }

    /// <summary>Observes whether a storage connection is open through its plain event, and hands the values to an observer object.</summary>
    public static void ObserveConnectionOpenThroughAnObserver()
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        EventObservable<bool> isOpen = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State == ConnectionState.Connected,
            true);
        IObserver<bool> observer = Witness.Create<bool>(Console.WriteLine);

        using (isOpen.Subscribe(observer))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // False
        // True
    }
}
