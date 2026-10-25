// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Index;

/// <summary>Walks through a first binding: load a to-do list, watch a property, bind it to a MAUI label and stop.</summary>
public static class FirstBindingExamples
{
    /// <summary>The title the first seeded item receives when it is renamed.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>Loads the to-do items into a view model.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task LoadTodosAsync()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());

        await viewModel.LoadAsync();

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 4
    }

    /// <summary>Watches one property with <c>WhenChanged</c>; the current value arrives first, then each change.</summary>
    /// <returns>A task that completes when the items are loaded and renamed.</returns>
    public static async Task ObserveTitleWithWhenChangedAsync()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        TodoItem registration = viewModel.Items[0];

        using (registration.WhenChanged(static x => x.Title).Subscribe(Console.WriteLine))
        {
            registration.Title = RenamedTitle;
        }

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Binds the number of items left to do to the text of a MAUI label with <c>BindOneWay</c>.</summary>
    /// <returns>A task that completes when an item is finished.</returns>
    public static async Task ShowRemainingCountWithBindOneWayAsync()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        TodoView view = new TodoView();
        await viewModel.LoadAsync();

        using (viewModel.BindOneWay(view, static x => x.RemainingCount, static v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            viewModel.SelectedItem = viewModel.Items[0];
            await viewModel.CompleteAsync();

            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
        // 2
    }

    /// <summary>Stops a binding by disposing it; the label keeps its last text.</summary>
    /// <returns>A task that completes when an item is finished.</returns>
    public static async Task StopBindingByDisposingAsync()
    {
        TodoListViewModel viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        TodoView view = new TodoView();
        await viewModel.LoadAsync();

        IDisposable binding = viewModel.BindOneWay(view, static x => x.RemainingCount, static v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture));
        binding.Dispose();

        viewModel.SelectedItem = viewModel.Items[0];
        await viewModel.CompleteAsync();

        Console.WriteLine(view.RemainingLabel.Text);

        // Output:
        // 3
    }
}
