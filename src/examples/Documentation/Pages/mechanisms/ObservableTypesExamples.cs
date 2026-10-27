// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Builds and subscribes to the observable types in <c>ReactiveUI.Binding.Observables</c>. Generated code and the
/// run-time engine construct these types; each example builds one by hand and shows what it delivers.
/// </summary>
public static class ObservableTypesExamples
{
    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title the registration task is renamed to a second time.</summary>
    private const string FinalTitle = "Renew car registration by post";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The address of the storage service.</summary>
    private const string StorageEndpoint = "https://storage.example.test";

    /// <summary>The name of the observed connection state.</summary>
    private const string StateName = nameof(StorageConnection.State);

    /// <summary>Observes whether a to-do item is urgent; a repeated value is dropped.</summary>
    public static void ObserveTodoUrgencyWithoutRepeats()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        PropertyObservable<bool> isUrgent = new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority == TodoPriority.High, true);

        using (isUrgent.Subscribe(Console.WriteLine))
        {
            item.Priority = TodoPriority.Low;
            item.Priority = TodoPriority.High;
        }

        // Output:
        // False
        // True
    }

    /// <summary>Observes whether a to-do item is urgent; every priority change delivers a value.</summary>
    public static void ObserveTodoUrgencyOnEveryChange()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        PropertyObservable<bool> isUrgent = new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority == TodoPriority.High, false);

        using (isUrgent.Subscribe(Console.WriteLine))
        {
            item.Priority = TodoPriority.Low;
            item.Priority = TodoPriority.High;
        }

        // Output:
        // False
        // False
        // True
    }

    /// <summary>Reads the title of an item before each edit is applied.</summary>
    public static void ObserveTitleBeforeEachEdit()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        PropertyChangingObservable<string> beforeChange = new(draft, nameof(DraftTodo.Title), static source => ((DraftTodo)source).Title);

        using (beforeChange.Subscribe(Console.WriteLine))
        {
            draft.Title = RenamedTitle;
            draft.Title = FinalTitle;
            draft.Notes = RegistrationNotes;
        }

        // Output:
        // Renew car registration
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Reports each after-change notification for one property as an observed change that names the sender and the expression.</summary>
    public static void ObserveDraftTitleAfterChange()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        Expression<Func<DraftTodo, string>> title = x => x.Title;
        NotifyPropertyChangedObservable afterChange = new(draft, title.Body, nameof(DraftTodo.Title), false);

        using (afterChange.Subscribe(static change => Console.WriteLine($"{change.GetPropertyName()}: {((DraftTodo)change.Sender).Title}")))
        {
            draft.Title = RenamedTitle;
            draft.Notes = RegistrationNotes;
        }

        // Output:
        // Title: Renew car registration online
    }

    /// <summary>Reports each before-change notification for one property; the sender still holds the old value.</summary>
    public static void ObserveDraftTitleBeforeChange()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        Expression<Func<DraftTodo, string>> title = x => x.Title;
        NotifyPropertyChangedObservable beforeChange = new(draft, title.Body, nameof(DraftTodo.Title), true);

        using (beforeChange.Subscribe(static change => Console.WriteLine($"{change.GetPropertyName()}: {((DraftTodo)change.Sender).Title}")))
        {
            draft.Title = RenamedTitle;
            draft.Notes = RegistrationNotes;
        }

        // Output:
        // Title: Renew car registration
    }

    /// <summary>Observes a property whose owner raises no notification: its value once, and then nothing, without completing.</summary>
    public static void ObserveStorageObjectKey()
    {
        StorageObject file = new() { Key = "photos/2026/launch.png" };
        UnchangingPropertyObservable<string> key = new(file.Key);

        using (key.Subscribe(
            Console.WriteLine,
            static error => Console.WriteLine($"Failed: {error.Message}"),
            static () => Console.WriteLine("Completed")))
        {
            file.Key = "photos/2026/renamed.png";
        }

        // Output:
        // photos/2026/launch.png
    }

    /// <summary>Reports the changes a binding wrote, and stops reporting to a subscriber that unsubscribed.</summary>
    public static void ReportAppliedChanges()
    {
        AppliedChangeObservable applied = new();

        Console.WriteLine($"Observers at the start: {applied.HasObservers}");

        using (applied.Subscribe(static change => Console.WriteLine($"{change.Value}, from the view model: {change.FromViewModel}")))
        {
            Console.WriteLine($"Observers while subscribed: {applied.HasObservers}");

            applied.OnNext(new(RenamedTitle, true));
            applied.OnNext(new(FinalTitle, false));
        }

        applied.OnNext(new(RegistrationTitle, true));

        Console.WriteLine($"Observers after disposing: {applied.HasObservers}");

        // Output:
        // Observers at the start: False
        // Observers while subscribed: True
        // Renew car registration online, from the view model: True
        // Renew car registration by post, from the view model: False
        // Observers after disposing: False
    }

    /// <summary>Reads the changes of a two-way binding, each tagged with the side that produced it.</summary>
    public static void ReadTwoWayBindingChanges()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };

        using IReactiveBinding<TodoView, BindingChange> binding = view.Bind(viewModel, x => x.NewTitle, v => v.NewTitleTextBox.Text);
        using IDisposable subscription = binding.Changed.Subscribe(static change => Console.WriteLine($"{change.Value}, from the view model: {change.FromViewModel}"));

        view.NewTitleTextBox.Text = RenamedTitle;
        viewModel.NewTitle = FinalTitle;

        Console.WriteLine($"The entry shows: {view.NewTitleTextBox.Text}");

        // Output:
        // Renew car registration online, from the view model: False
        // Renew car registration by post, from the view model: True
        // The entry shows: Renew car registration by post
    }

    /// <summary>Observes whether a to-do item is urgent through an observer object instead of a callback.</summary>
    public static void ObserveTodoUrgencyThroughAnObserver()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        PropertyObservable<bool> isUrgent = new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority == TodoPriority.High, true);
        IObserver<bool> observer = Witness.Create<bool>(Console.WriteLine);

        using (isUrgent.Subscribe(observer))
        {
            item.Priority = TodoPriority.Low;
            item.Priority = TodoPriority.High;
        }

        // Output:
        // False
        // True
    }

    /// <summary>Reads the title of an item before an edit is applied, through an observer object.</summary>
    public static void ObserveTitleBeforeAnEditThroughAnObserver()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        PropertyChangingObservable<string> beforeChange = new(draft, nameof(DraftTodo.Title), static source => ((DraftTodo)source).Title);
        IObserver<string> observer = Witness.Create<string>(Console.WriteLine);

        using (beforeChange.Subscribe(observer))
        {
            draft.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration
    }

    /// <summary>Names the property of each after-change notification, through an observer object.</summary>
    public static void ObserveDraftTitleThroughAnObserver()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        Expression<Func<DraftTodo, string>> title = x => x.Title;
        NotifyPropertyChangedObservable afterChange = new(draft, title.Body, nameof(DraftTodo.Title), false);
        IObserver<IObservedChange<object, object?>> observer = Witness.Create<IObservedChange<object, object?>>(static change => Console.WriteLine(change.GetPropertyName()));

        using (afterChange.Subscribe(observer))
        {
            draft.Title = RenamedTitle;
        }

        // Output:
        // Title
    }

    /// <summary>Observes a property whose owner raises no notification, through an observer object.</summary>
    public static void ObserveStorageObjectKeyThroughAnObserver()
    {
        StorageObject file = new() { Key = "photos/2026/launch.png" };
        UnchangingPropertyObservable<string> key = new(file.Key);
        IObserver<string> observer = Witness.Create<string>(Console.WriteLine);

        using IDisposable subscription = key.Subscribe(observer);

        // Output:
        // photos/2026/launch.png
    }

    /// <summary>Reports the changes a binding wrote to an observer object.</summary>
    public static void ReportAppliedChangesToAnObserver()
    {
        AppliedChangeObservable applied = new();
        IObserver<BindingChange> observer = Witness.Create<BindingChange>(static change => Console.WriteLine($"{change.Value}, from the view model: {change.FromViewModel}"));

        using (applied.Subscribe(observer))
        {
            applied.OnNext(new(RenamedTitle, true));
        }

        // Output:
        // Renew car registration online, from the view model: True
    }

    /// <summary>Keeps the generated observation while no registered provider outranks it.</summary>
    /// <param name="item">The item to observe.</param>
    public static void KeepGeneratedObservationWhenNoProviderOutranksIt(TodoItem item)
    {
        Expression<Func<TodoItem, bool>> isDone = x => x.IsDone;
        PropertyObservable<bool> generated = new(item, nameof(TodoItem.IsDone), static source => ((TodoItem)source).IsDone, true);

        IObservable<bool> chosen = PluginObservationSource.Choose(
            item,
            isDone.Body,
            nameof(TodoItem.IsDone),
            false,
            BindingAffinity.Explicit,
            static source => ((TodoItem)source).IsDone,
            generated);

        Console.WriteLine($"The generated observation is kept: {ReferenceEquals(generated, chosen)}");

        // Output:
        // The generated observation is kept: True
    }

    /// <summary>Keeps the generated observation when the provider only ties its affinity.</summary>
    /// <param name="provider">The registered provider.</param>
    public static void KeepGeneratedObservationOnEqualAffinity(StorageConnectionObservableForProperty provider)
    {
        StorageConnection connection = new() { Endpoint = StorageEndpoint };
        Expression<Func<StorageConnection, ConnectionState>> state = x => x.State;
        EventObservable<ConnectionState> generated = new(
            handler => connection.StateChanged += handler,
            handler => connection.StateChanged -= handler,
            () => connection.State,
            true);

        IObservable<ConnectionState> chosen = PluginObservationSource.Choose(
            connection,
            state.Body,
            StateName,
            false,
            provider.GetAffinityForObject(typeof(StorageConnection), StateName, false),
            static source => ((StorageConnection)source).State,
            generated);

        Console.WriteLine($"The generated observation is kept: {ReferenceEquals(generated, chosen)}");

        // Output:
        // The generated observation is kept: True
    }
}
