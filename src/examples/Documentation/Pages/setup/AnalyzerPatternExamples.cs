// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>
/// The shapes of binding call that the analyzers accept: a path written in the call, made of public properties,
/// on a type that raises a notification. Each method is the corrected form of a call that one analyzer reports.
/// </summary>
public static class AnalyzerPatternExamples
{
    /// <summary>The title an example gives the first item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The title of a to-do item the user is adding.</summary>
    private const string DentistTitle = "Book dentist appointment";

    /// <summary>An amount that is above zero and within any account.</summary>
    private const decimal SmallAmount = 25M;

    /// <summary>Writes the property path in the call, as a lambda the generator can read.</summary>
    public static void WritePathInTheCall()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };

        using (item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            item.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Observes a public property; a private or protected property is out of reach of the generated code.</summary>
    public static void ObservePublicProperty()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());

        viewModel.NewTitle = DentistTitle;

        using (viewModel.WhenChanged(x => x.NewTitle).Subscribe(Console.WriteLine))
        {
            viewModel.NewTitle = "Buy birthday present for Sam";
        }

        // Output:
        // Book dentist appointment
        // Buy birthday present for Sam
    }

    /// <summary>Follows a path made of properties and instance fields; an indexer or a method call has no notification to follow.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task ObservePathOfProperties()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.WhenChanged(x => x.SelectedItem!.Title).Subscribe(Console.WriteLine))
        {
            viewModel.SelectedItem = viewModel.Items[1];
        }

        // Output:
        // Renew car registration
        // Book dentist appointment
    }

    /// <summary>Observes a type that raises <c>PropertyChanged</c>; a type that raises nothing never reports a change.</summary>
    public static void ObserveNotifyingType()
    {
        TodoItem item = new TodoItem { Title = "File quarterly tax return" };

        using (item.WhenChanged(x => x.IsDone).Subscribe(Console.WriteLine))
        {
            item.IsDone = true;
        }

        // Output:
        // False
        // True
    }

    /// <summary>Observes a property of the view model that mirrors the state of an object that raises nothing.</summary>
    /// <returns>A task that completes when the browser has read the link state.</returns>
    public static async Task ObserveMirroredProperty()
    {
        InMemoryObjectStorage storage = InMemoryObjectStorage.CreateSeeded();
        StorageBrowserViewModel browser = new StorageBrowserViewModel(storage);

        using (browser.WhenChanged(x => x.ConnectionStatus).Subscribe(static state => Console.WriteLine(state)))
        {
            storage.Disconnect();
            await browser.ConnectAsync();
        }

        // Output:
        // Connected
        // Disconnected
        // Connecting
        // Connected
    }

    /// <summary>Observes a value before it changes on a type that raises <c>PropertyChanging</c>.</summary>
    public static void ObserveBeforeChange()
    {
        EditableTodo todo = new EditableTodo { Title = DentistTitle };

        using (todo.WhenChanging(x => x.Title).Subscribe(Console.WriteLine))
        {
            todo.Title = "Book dentist appointment for Friday";
        }

        // Output:
        // Book dentist appointment
        // Book dentist appointment
    }

    /// <summary>Binds a command to a button, which raises an event the binding can use.</summary>
    public static void BindCommandToButton()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        TodoView view = new TodoView { ViewModel = viewModel };

        using (view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton))
        {
            Console.WriteLine(view.AddButton.Command!.CanExecute(null));

            viewModel.NewTitle = DentistTitle;

            Console.WriteLine(view.AddButton.Command.CanExecute(null));
        }

        // Output:
        // False
        // True
    }

    /// <summary>Binds validation errors that the view model exposes as a property; the generator does not read <c>INotifyDataErrorInfo</c>.</summary>
    public static void BindValidationSummary()
    {
        TransferViewModel viewModel = new TransferViewModel(new InMemoryBankingBackend());
        TransferView view = new TransferView { ViewModel = viewModel };

        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        {
            viewModel.Draft.Amount = SmallAmount;

            Console.WriteLine(view.ValidationLabel.Text);
        }

        // Output:
        // Choose the account to pay from. Choose who to pay.
    }

    /// <summary>Binds a handler to a property that is an interaction.</summary>
    /// <returns>A task that completes when the handler has answered.</returns>
    public static async Task BindInteractionProperty()
    {
        IssueBoardViewModel viewModel = new IssueBoardViewModel(InMemoryGitHubServer.CreateSeeded());
        IssueBoardView view = new IssueBoardView { ViewModel = viewModel };

        using (view.BindInteraction(viewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        }))
        {
            bool confirmed = await viewModel.ConfirmClose.Handle(new Issue { Title = "Crash on startup" });

            Console.WriteLine(confirmed);
        }

        // Output:
        // True
    }

    /// <summary>Calls the binding APIs of this package by importing its namespace, not ReactiveUI's own.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task ImportThePackageNamespace()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();

        using (viewModel.WhenAnyValue(x => x.RemainingCount).Subscribe(Console.WriteLine))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteAsync();
        }

        // Output:
        // 3
        // 2
    }

    /// <summary>Calls a binding API from a file under the root namespace of the project.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task ObserveFromRootNamespace()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();

        using (viewModel.WhenChanged(x => x.RemainingCount).Subscribe(Console.WriteLine))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteAsync();
        }

        // Output:
        // 3
        // 2
    }
}
