// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Shows the two built-in observation mechanisms: <see cref="INPCObservableForProperty"/> for a class that raises
/// <c>PropertyChanged</c>, and <see cref="POCOObservableForProperty"/> for a plain class that raises nothing.
/// </summary>
public static class NotifyPropertyChangedExamples
{
    /// <summary>The title given to an item while it is observed.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title given to an item while its <c>PropertyChanged</c> notifications are observed directly.</summary>
    private const string RetitledByPost = "Renew car registration by post";

    /// <summary>The notes written to an item while a different property is observed.</summary>
    private const string EditedNotes = "Bring the old plate as well.";

    /// <summary>The name of the observed title property.</summary>
    private const string TitlePropertyName = nameof(TodoItem.Title);

    /// <summary>The name of the observed size property.</summary>
    private const string SizePropertyName = nameof(StorageObject.Size);

    /// <summary>The size given to a stored object while it is observed.</summary>
    private const long ResizedBytes = 4_096;

    /// <summary>Registers the core observation providers, which the run-time engine resolves by affinity.</summary>
    public static void RegisterCoreObservationServices()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();

        foreach (ICreatesObservableForProperty provider in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
        {
            Console.WriteLine(provider.GetType().Name);
        }

        // Output:
        // INPCObservableForProperty
        // POCOObservableForProperty
    }

    /// <summary>Observes an item through <c>WhenChanged</c>; the generated code listens to <c>PropertyChanged</c>.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClassThroughWhenChanged(TodoItem item)
    {
        using (item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Asks the <c>PropertyChanged</c> provider about an item and listens to it directly.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClassWithInpcProvider(TodoItem item)
    {
        INPCObservableForProperty provider = new();
        Expression<Func<TodoItem, string>> property = x => x.Title;

        Console.WriteLine($"Affinity: {provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName)}");
        Console.WriteLine($"Affinity after a change: {provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName, false)}");

        // TodoItem does not implement INotifyPropertyChanging, so the provider bids nothing for before-change observation.
        Console.WriteLine($"Affinity before a change: {provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName, true)}");

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(item, property.Body, TitlePropertyName).Subscribe(changes.Add))
        {
            item.Notes = EditedNotes;
            item.Title = RetitledByPost;
        }

        Console.WriteLine($"Notifications: {changes.Count}");
        Console.WriteLine($"Sender is the item: {ReferenceEquals(item, changes[0].Sender)}");
        Console.WriteLine($"Property: {changes[0].GetPropertyName()}");

        // Output:
        // Affinity: 5
        // Affinity after a change: 5
        // Affinity before a change: 0
        // Notifications: 1
        // Sender is the item: True
        // Property: Title
    }

    /// <summary>Asks the <c>PropertyChanged</c> provider about a plain class; it bids nothing and reports nothing.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClassWithInpcProvider(StorageObject stored)
    {
        INPCObservableForProperty provider = new();
        Expression<Func<StorageObject, long>> property = x => x.Size;

        Console.WriteLine($"Affinity: {provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName)}");

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(stored, property.Body, SizePropertyName).Subscribe(changes.Add))
        {
            stored.Size = ResizedBytes;
        }

        Console.WriteLine($"Notifications: {changes.Count}");

        // Output:
        // Affinity: 0
        // Notifications: 0
    }

    /// <summary>Asks the fallback provider about a plain class; it delivers one notification when observation starts and no more.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClassWithPocoProvider(StorageObject stored)
    {
        POCOObservableForProperty provider = new();
        Expression<Func<StorageObject, long>> property = x => x.Size;

        Console.WriteLine($"Affinity for a stored object: {provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName)}");
        Console.WriteLine($"Affinity for a to-do item: {provider.GetAffinityForObject(typeof(TodoItem), TitlePropertyName)}");

        List<IObservedChange<object, object?>> changes = [];
        using (provider.GetNotificationForProperty(stored, property.Body, SizePropertyName, false, true).Subscribe(changes.Add))
        {
            Console.WriteLine($"Notifications at the start: {changes.Count}");

            stored.Size = ResizedBytes;
        }

        Console.WriteLine($"Notifications after the change: {changes.Count}");
        Console.WriteLine($"Sender is the stored object: {ReferenceEquals(stored, changes[0].Sender)}");
        Console.WriteLine($"Property: {changes[0].GetPropertyName()}");

        // Output:
        // Affinity for a stored object: 1
        // Affinity for a to-do item: 1
        // Notifications at the start: 1
        // Notifications after the change: 1
        // Sender is the stored object: True
        // Property: Size
    }

    /// <summary>Asks the fallback provider for its bid before and after a change; it bids the same for both.</summary>
    public static void AskFallbackProviderBeforeAndAfterChange()
    {
        POCOObservableForProperty provider = new();

        Console.WriteLine($"Affinity after a change: {provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName, false)}");
        Console.WriteLine($"Affinity before a change: {provider.GetAffinityForObject(typeof(StorageObject), SizePropertyName, true)}");

        // Output:
        // Affinity after a change: 1
        // Affinity before a change: 1
    }

    /// <summary>Observes a notifying view model's selection through <c>WhenChanged</c> and reads the plain class's property on each delivery.</summary>
    /// <param name="browser">The browser whose selection is observed.</param>
    /// <param name="first">The object selected first.</param>
    /// <param name="second">The object selected second.</param>
    public static void ObservePlainClassThroughWhenChanged(StorageBrowserViewModel browser, StorageObject first, StorageObject second)
    {
        browser.SelectedObject = first;

        using (browser.WhenChanged(x => x.SelectedObject!).Subscribe(static selected => Console.WriteLine(selected.Size)))
        {
            first.Size = ResizedBytes;
            browser.SelectedObject = second;
        }

        // Output:
        // 2411724
        // 5562368
    }
}
