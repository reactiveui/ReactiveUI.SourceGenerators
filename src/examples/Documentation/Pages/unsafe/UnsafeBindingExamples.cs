// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates the <c>Unsafe</c> twins of the binding methods. Each property path here is held in a variable, so the
/// generator cannot read it and only the reflection-based overload can serve the call.
/// </summary>
public static class UnsafeBindingExamples
{
    /// <summary>The title an example adds through a bound button.</summary>
    private const string NewItemTitle = "Water the plants";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The new title the first to-do item is given.</summary>
    private const string RenamedTitle = "Renew registration online";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>Writes a source property to a target property, with both paths held in variables.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayBetweenPathsChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (viewModel.BindOneWayUnsafe(view, source, target))
        {
            viewModel.FilterText = FilterQuery;
        }

        Console.WriteLine(view.FilterTextBox.Text);

        // Output:
        // car
    }

    /// <summary>Writes a source property to a target property of another type through a conversion function.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayWithConversionChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, int>> source = x => x.RemainingCount;
        Expression<Func<TodoView, string>> target = v => v.RemainingLabel.Text;

        using (viewModel.BindOneWayUnsafe(view, source, target, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Carries a property both ways between two paths held in variables.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindTwoWayBetweenPathsChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (viewModel.BindTwoWayUnsafe(view, source, target))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine(viewModel.FilterText);
        }

        // Output:
        // car
    }

    /// <summary>Carries an amount both ways as text, with a conversion function for each direction.</summary>
    public static void BindTwoWayWithConversionsChosenAtRunTime()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new();
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (viewModel.BindTwoWayUnsafe(
            view,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture)))
        {
            view.AmountTextBox.Text = AmountText;

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 125.50
    }

    /// <summary>Binds a view model property to a view property, written view first.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (view.OneWayBindUnsafe(viewModel, source, target))
        {
            viewModel.FilterText = FilterQuery;
        }

        Console.WriteLine(view.FilterTextBox.Text);

        // Output:
        // car
    }

    /// <summary>Binds a view model property to a view property, written view first, through a selector.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindWithSelectorChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, int>> source = x => x.RemainingCount;
        Expression<Func<TodoView, string>> target = v => v.RemainingLabel.Text;

        using (view.OneWayBindUnsafe(viewModel, source, target, static count => $"{count} left"))
        {
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3 left
    }

    /// <summary>Binds through an indexer, a path the generator leaves to the runtime, written view first.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindThroughIndexer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, string>> source = x => x.Items[0].Title;
        Expression<Func<TodoView, string>> target = v => v.SelectedTitleTextBox.Text;

        using (view.OneWayBindUnsafe(viewModel, source, target))
        {
            Console.WriteLine(view.SelectedTitleTextBox.Text);

            // The binding observes the item the indexer returns, so renaming it updates the box.
            viewModel.Items[0].Title = RenamedTitle;
            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // Renew car registration
        // Renew registration online
    }

    /// <summary>Carries a property both ways, written view first.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (view.BindUnsafe(viewModel, source, target))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine(viewModel.FilterText);
        }

        // Output:
        // car
    }

    /// <summary>Carries an amount both ways as text, written view first, with a conversion function for each direction.</summary>
    public static void BindWithConversionsChosenAtRunTime()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (view.BindUnsafe(
            viewModel,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture)))
        {
            view.AmountTextBox.Text = AmountText;

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 125.50
    }

    /// <summary>Writes each selected item of a stream to a text property, through the converter registered for the two types.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindToTargetChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string?>> target = v => v.SelectedTitleTextBox.Text;

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // Renew car registration
    }

    /// <summary>Writes each selected item of a stream to a text property, through a converter you name.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindToWithConverterOverride()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string?>> target = v => v.SelectedTitleTextBox.Text;

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, new TodoItemTitleConverter()))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // Renew car registration
    }

    /// <summary>Writes each selected item of a stream to a text property, passing a conversion hint and a converter you name.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindToWithHintAndConverterOverride()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string?>> target = v => v.SelectedTitleTextBox.Text;

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, TodoItemTitleConverter.UpperCaseHint, new TodoItemTitleConverter()))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // RENEW CAR REGISTRATION
    }

    /// <summary>Writes each selected item of a stream to a text property, passing a conversion hint to the registered converter.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindToWithConversionHint()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string?>> target = v => v.SelectedTitleTextBox.Text;

        using (viewModel.WhenChangedUnsafe(source).BindToUnsafe(view, target, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // RENEW CAR REGISTRATION
    }

    /// <summary>Runs a command for each selected item of a stream.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task InvokeCommandChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoListViewModel, ICommand?>> command = x => x.CompleteCommand;
        TaskCompletionSource completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.RemainingCount))
            {
                _ = completed.TrySetResult();
            }
        };

        using (viewModel.WhenChangedUnsafe(source).InvokeCommandUnsafe(viewModel, command))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await completed.Task;
        }

        Console.WriteLine(viewModel.RemainingCount);

        // Output:
        // 2
    }

    /// <summary>Binds a command to a button, with both paths held in variables.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButtonChosenAtRunTime()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, Command?>> command = x => x.AddCommand;
        Expression<Func<TodoView, Button>> button = v => v.AddButton;
        TaskCompletionSource itemAdded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.Items))
            {
                _ = itemAdded.TrySetResult();
            }
        };

        using (view.BindCommandUnsafe(viewModel, command, button, null))
        {
            viewModel.NewTitle = NewItemTitle;
            ((IButtonController)view.AddButton).SendClicked();
            await itemAdded.Task;
        }

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 5
    }

    /// <summary>Binds a command to a named event of a button, passing a stream of parameters.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandWithParameterStream()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, Command?>> command = x => x.AddCommand;
        Expression<Func<TodoView, Button>> button = v => v.AddButton;
        TaskCompletionSource itemAdded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.Items))
            {
                _ = itemAdded.TrySetResult();
            }
        };

        using (view.BindCommandUnsafe(viewModel, command, button, Signal.Return<string?>(NewItemTitle), nameof(Button.Clicked)))
        {
            viewModel.NewTitle = NewItemTitle;
            ((IButtonController)view.AddButton).SendClicked();
            await itemAdded.Task;
        }

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 5
    }

    /// <summary>Binds a command to a button, taking the parameter from a view model property.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandWithParameterProperty()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Expression<Func<TodoListViewModel, Command?>> command = x => x.AddCommand;
        Expression<Func<TodoView, Button>> button = v => v.AddButton;
        Expression<Func<TodoListViewModel, string?>> parameter = x => x.NewTitle;
        TaskCompletionSource itemAdded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.Items))
            {
                _ = itemAdded.TrySetResult();
            }
        };

        using (view.BindCommandUnsafe(viewModel, command, button, parameter, null))
        {
            viewModel.NewTitle = NewItemTitle;
            ((IButtonController)view.AddButton).SendClicked();
            await itemAdded.Task;
        }

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 5
    }

    /// <summary>Registers an asynchronous handler for an interaction held in a variable.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionWithTaskHandler()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Expression<Func<TransferViewModel, IInteraction<TransferDraft, bool>>> interaction = x => x.ConfirmTransfer;

        using (view.BindInteractionUnsafe(
            viewModel,
            interaction,
            static context =>
            {
                context.SetOutput(true);
                return Task.CompletedTask;
            }))
        {
            bool confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            Console.WriteLine(confirmed);
        }

        // Output:
        // True
    }

    /// <summary>Registers an observable handler for an interaction held in a variable.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionWithObservableHandler()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Expression<Func<TransferViewModel, IInteraction<TransferDraft, bool>>> interaction = x => x.ConfirmTransfer;

        using (view.BindInteractionUnsafe(
            viewModel,
            interaction,
            static context =>
            {
                context.SetOutput(true);
                return Signal.Return(true);
            }))
        {
            bool confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            Console.WriteLine(confirmed);
        }

        // Output:
        // True
    }

    /// <summary>Creates a view model that has loaded the seeded items.</summary>
    /// <returns>The loaded view model.</returns>
    private static async Task<TodoListViewModel> LoadTodoAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        await viewModel.LoadAsync();
        return viewModel;
    }
}
