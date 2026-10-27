// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;
using Splat;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Shows how a provider for <see cref="StorageConnection"/> is registered and how every part of the runtime
/// (the generated bindings, the chain engine and the plugin observables) consults it.
/// </summary>
public static class CustomProviderExamples
{
    /// <summary>The name of the observed property.</summary>
    private const string StatePropertyName = nameof(StorageConnection.State);

    /// <summary>Registers the provider through the builder and clears the cached votes so the next observation sees it.</summary>
    /// <param name="provider">The provider to register.</param>
    public static void RegisterProviderThroughBuilder(StorageConnectionObservableForProperty provider)
    {
        bool outranksBefore = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(StorageConnection), StatePropertyName, BindingAffinity.Fallback, false);

        Console.WriteLine($"Provider outranks the fallback before registering: {outranksBefore}");

        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(provider))
            .BuildApp();

        ObservationAffinityChecker.Refresh();

        Console.WriteLine($"Provider is registered: {AppLocator.Current.GetServices<ICreatesObservableForProperty>().Contains(provider)}");
        bool outranksAfter = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(StorageConnection), StatePropertyName, BindingAffinity.Fallback, false);

        Console.WriteLine($"Provider outranks the fallback after registering: {outranksAfter}");

        // Output:
        // Provider outranks the fallback before registering: False
        // Provider is registered: True
        // Provider outranks the fallback after registering: True
    }

    /// <summary>
    /// Observes the connection at run time; each state change reaches the subscriber through the provider. The
    /// generator writes no dispatch for a type with no built-in mechanism, so this observation is the run-time one.
    /// </summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    /// <param name="storage">The storage service that owns the connection.</param>
    /// <returns>A task that completes when the reconnect has finished.</returns>
    public static async Task ObserveConnectionThroughProvider(StorageBrowserViewModel browser, InMemoryObjectStorage storage)
    {
        using (browser.Connection.ObservableForProperty(x => x.State, false).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            storage.Disconnect();
            await browser.ConnectAsync().ConfigureAwait(false);
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connecting
        // Connected
    }

    /// <summary>Observes the connection with <c>WhenChanged</c>; the registered provider delivers each state change.</summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    /// <param name="storage">The storage service that owns the connection.</param>
    public static void ObserveConnectionWithWhenChanged(StorageBrowserViewModel browser, InMemoryObjectStorage storage)
    {
        using (browser.Connection.WhenChanged(x => x.State).Subscribe(static state => Console.WriteLine(state)))
        {
            storage.Disconnect();
            storage.Connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Calls the provider directly, with the overloads that fill in the after-change defaults.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void CallProviderThroughMixins(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;

        Console.WriteLine($"Affinity for State: {provider.GetAffinityForObject(typeof(StorageConnection), StatePropertyName)}");
        Console.WriteLine($"Affinity for Endpoint: {provider.GetAffinityForObject(typeof(StorageConnection), nameof(StorageConnection.Endpoint))}");
        Console.WriteLine($"Affinity for State before a change: {provider.GetAffinityForObject(typeof(StorageConnection), StatePropertyName, true)}");

        List<IObservedChange<object, object?>> after = [];
        using (provider.GetNotificationForProperty(connection, property.Body, StatePropertyName).Subscribe(after.Add))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        Console.WriteLine($"Notifications: {after.Count}");
        Console.WriteLine($"Sender is the connection: {ReferenceEquals(connection, after[0].Sender)}");
        Console.WriteLine($"First value: {after[0].Value}");
        Console.WriteLine($"Second value: {after[1].Value}");

        List<IObservedChange<object, object?>> before = [];
        using (provider.GetNotificationForProperty(connection, property.Body, StatePropertyName, true).Subscribe(before.Add))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        Console.WriteLine($"Notifications before a change: {before.Count}");

        // Output:
        // Affinity for State: 8
        // Affinity for Endpoint: 0
        // Affinity for State before a change: 0
        // The connection state is observed by the StateChanged provider
        // Notifications: 2
        // Sender is the connection: True
        // First value: Connecting
        // Second value: Connected
        // Notifications before a change: 0
    }

    /// <summary>Picks the registered provider with the highest bid, as the run-time engine does, and calls it through <see cref="ICreatesObservableForProperty"/>.</summary>
    /// <param name="connection">The connection to observe.</param>
    public static void CallProviderThroughItsInterface(StorageConnection connection)
    {
        ICreatesObservableForProperty registered = AppLocator.Current
            .GetServices<ICreatesObservableForProperty>()
            .MaxBy(static candidate => candidate.GetAffinityForObject(typeof(StorageConnection), StatePropertyName, false))!;
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;

        Console.WriteLine($"Affinity for State: {registered.GetAffinityForObject(typeof(StorageConnection), StatePropertyName, false)}");

        using (registered.GetNotificationForProperty(connection, property.Body, StatePropertyName, false, true).Subscribe(static change => Console.WriteLine(change.Value)))
        {
            connection.State = ConnectionState.Connecting;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // Affinity for State: 8
        // The connection state is observed by the StateChanged provider
        // Connecting
        // Connected
    }

    /// <summary>Asks which registered provider outranks a mechanism the generator picked, at several generated scores.</summary>
    public static void FindProviderThatOutranksGeneratedMechanism()
    {
        Type connectionType = typeof(StorageConnection);

        bool outranksFallback = ObservationAffinityChecker.HasHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Fallback, false);
        ICreatesObservableForProperty? winnerOverPropertyChanged = ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Explicit, false);

        Console.WriteLine($"Outranks the fallback: {outranksFallback}");
        Console.WriteLine($"Outranks the PropertyChanged provider: {winnerOverPropertyChanged?.GetType().Name}");

        // A tie goes to the generated mechanism, and so does a higher generated score.
        bool outranksEqual = ObservationAffinityChecker.HasHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.WinFormsEvent, false);
        ICreatesObservableForProperty? winnerOverHigher = ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Kvo, false);

        Console.WriteLine($"Outranks an equal score: {outranksEqual}");
        Console.WriteLine($"Nothing outranks a higher score: {winnerOverHigher is null}");

        // The provider answers for one property and only after a change.
        ICreatesObservableForProperty? forEndpoint = ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, nameof(StorageConnection.Endpoint), BindingAffinity.Fallback, false);
        ICreatesObservableForProperty? beforeChange = ObservationAffinityChecker.FindHigherAffinityPlugin(connectionType, StatePropertyName, BindingAffinity.Fallback, true);

        Console.WriteLine($"Nothing answers for Endpoint: {forEndpoint is null}");
        Console.WriteLine($"Nothing answers before a change: {beforeChange is null}");

        // Output:
        // Outranks the fallback: True
        // Outranks the PropertyChanged provider: StorageConnectionObservableForProperty
        // Outranks an equal score: False
        // Nothing outranks a higher score: True
        // Nothing answers for Endpoint: True
        // Nothing answers before a change: True
    }

    /// <summary>Observes the state through <see cref="PluginPropertyObservable{T}"/>, which reads each value with a getter.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughPluginPropertyObservable(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        PluginPropertyObservable<ConnectionState> observable = new(
            provider,
            connection,
            property.Body,
            StatePropertyName,
            static source => ((StorageConnection)source).State,
            false,
            true);

        using (observable.Subscribe(static state => Console.WriteLine(state)))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Observes the state through <see cref="PluginPropertyObservable{T}"/> and hands the values to an observer object.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughPluginPropertyObservableWithAnObserver(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        PluginPropertyObservable<ConnectionState> observable = new(
            provider,
            connection,
            property.Body,
            StatePropertyName,
            static source => ((StorageConnection)source).State,
            false,
            true);
        IObserver<ConnectionState> observer = Witness.Create<ConnectionState>(static state => Console.WriteLine(state));

        using (observable.Subscribe(observer))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Lets <see cref="PluginObservationSource"/> choose between the generated observation and the provider.</summary>
    /// <param name="connection">The connection to observe.</param>
    public static void ChooseBetweenGeneratedObservationAndProvider(StorageConnection connection)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        UnchangingPropertyObservable<ConnectionState> generated = new(connection.State);

        IObservable<ConnectionState> lowGenerated = PluginObservationSource.Choose(
            connection,
            property.Body,
            StatePropertyName,
            false,
            BindingAffinity.Fallback,
            static source => ((StorageConnection)source).State,
            generated);
        IObservable<ConnectionState> highGenerated = PluginObservationSource.Choose(
            connection,
            property.Body,
            StatePropertyName,
            false,
            BindingAffinity.Kvo,
            static source => ((StorageConnection)source).State,
            generated);

        Console.WriteLine($"A low score hands over to the provider: {lowGenerated is PluginPropertyObservable<ConnectionState>}");
        Console.WriteLine($"A high score keeps the generated observation: {ReferenceEquals(generated, highGenerated)}");

        using (lowGenerated.Subscribe(static state => Console.WriteLine(state)))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // A low score hands over to the provider: True
        // A high score keeps the generated observation: True
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Walks <c>x.Connection.State</c> with the chain engine; the last link is observed through the provider.</summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    public static void ObserveThroughExpressionChain(StorageBrowserViewModel browser)
    {
        Expression<Func<StorageBrowserViewModel, ConnectionState>> property = x => x.Connection.State;
        Expression[] links = [.. Reflection.Rewrite(property.Body).GetExpressionChain()];
        ExpressionChainParameters<StorageBrowserViewModel> parameters = new(browser, property.Body, links, false, false, true, true);
        ExpressionChainSink<StorageBrowserViewModel, ConnectionState> sink = new(
            parameters.Source,
            parameters.Expression,
            parameters.Links,
            parameters.BeforeChange,
            parameters.SkipInitial,
            parameters.IsDistinct,
            parameters.SuppressWarnings);

        using (sink.Subscribe(change => Console.WriteLine($"{change.Value}, sent by the browser: {ReferenceEquals(browser, change.Sender)}")))
        {
            browser.Connection.State = ConnectionState.Disconnected;
            browser.Connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected, sent by the browser: True
        // Disconnected, sent by the browser: True
        // Connected, sent by the browser: True
    }

    /// <summary>Walks <c>x.Connection.State</c> with the chain engine and hands the changes to an observer object.</summary>
    /// <param name="browser">The browser whose connection is observed.</param>
    public static void ObserveThroughExpressionChainWithAnObserver(StorageBrowserViewModel browser)
    {
        Expression<Func<StorageBrowserViewModel, ConnectionState>> property = x => x.Connection.State;
        Expression[] links = [.. Reflection.Rewrite(property.Body).GetExpressionChain()];
        ExpressionChainSink<StorageBrowserViewModel, ConnectionState> sink = new(browser, property.Body, links, false, false, true, true);
        IObserver<IObservedChange<StorageBrowserViewModel, ConnectionState>> observer =
            Witness.Create<IObservedChange<StorageBrowserViewModel, ConnectionState>>(static change => Console.WriteLine(change.Value));

        using (sink.Subscribe(observer))
        {
            browser.Connection.State = ConnectionState.Disconnected;
            browser.Connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Wraps the provider's notifications in <see cref="ObservableForPropertySink{TSender, TValue}"/>, which reads the value on each one.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughObservableForPropertySink(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        IObservable<IObservedChange<object, object?>> notifications = provider.GetNotificationForProperty(connection, property.Body, StatePropertyName);
        ObservableForPropertySink<StorageConnection, ConnectionState> sink = new(
            connection,
            property.Body,
            notifications,
            () => connection.State,
            false,
            true);

        using (sink.Subscribe(static change => Console.WriteLine(change.Value)))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }

    /// <summary>Wraps the provider's notifications in <see cref="ObservableForPropertySink{TSender, TValue}"/> and hands the values to an observer object.</summary>
    /// <param name="connection">The connection to observe.</param>
    /// <param name="provider">The registered provider.</param>
    public static void ObserveThroughObservableForPropertySinkWithAnObserver(StorageConnection connection, StorageConnectionObservableForProperty provider)
    {
        Expression<Func<StorageConnection, ConnectionState>> property = x => x.State;
        IObservable<IObservedChange<object, object?>> notifications = provider.GetNotificationForProperty(connection, property.Body, StatePropertyName);
        ObservableForPropertySink<StorageConnection, ConnectionState> sink = new(
            connection,
            property.Body,
            notifications,
            () => connection.State,
            false,
            true);
        IObserver<IObservedChange<StorageConnection, ConnectionState>> observer = Witness.Create<IObservedChange<StorageConnection, ConnectionState>>(static change => Console.WriteLine(change.Value));

        using (sink.Subscribe(observer))
        {
            connection.State = ConnectionState.Disconnected;
            connection.State = ConnectionState.Connected;
        }

        // Output:
        // The connection state is observed by the StateChanged provider
        // Connected
        // Disconnected
        // Connected
    }
}
