// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates <c>BindUnsafe</c> with an update stream. The stream decides when one direction of the binding runs, so
/// a view can hold an edit until the user commits it, or a view model change can wait until the screen refreshes.
/// </summary>
public static class UnsafeTriggerUpdateExamples
{
    /// <summary>The text a user types into the filter box.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text a user types over <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>
    /// Holds a view edit until the stream fires, then writes it to the view model. The view model is not held back.
    /// <see cref="TriggerUpdate.ViewToViewModel"/> is the default direction, so the call names none.
    /// </summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task CommitViewEditOnSignal()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine($"'{viewModel.FilterText}'");

            commit.OnNext(EventArgs.Empty);

            Console.WriteLine($"'{viewModel.FilterText}'");

            viewModel.FilterText = SecondFilterQuery;

            Console.WriteLine($"'{view.FilterTextBox.Text}'");
        }

        // Output:
        // ''
        // 'car'
        // 'tax'
    }

    /// <summary>Holds a view model change until the stream fires, then writes it to the view. The view is not held back.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task RefreshViewOnSignal()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            viewModel.FilterText = FilterQuery;

            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            refresh.OnNext(EventArgs.Empty);

            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            view.FilterTextBox.Text = SecondFilterQuery;

            Console.WriteLine($"'{viewModel.FilterText}'");
        }

        // Output:
        // ''
        // 'car'
        // 'tax'
    }

    /// <summary>Reads the view model when the stream fires, so several changes in between reach the view as one write.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task DeliverLatestViewModelValueOnSignal()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();
        int writes = 0;

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            view.FilterTextBox.PropertyChanged += (_, e) => writes += e.PropertyName == nameof(view.FilterTextBox.Text) ? 1 : 0;

            viewModel.FilterText = FilterQuery;
            viewModel.FilterText = SecondFilterQuery;
            refresh.OnNext(EventArgs.Empty);

            Console.WriteLine(view.FilterTextBox.Text);
            Console.WriteLine(writes);
        }

        // Output:
        // tax
        // 1
    }

    /// <summary>Delivers the view model value to the view when the binding starts, whichever direction the stream drives.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task DeliverInitialViewModelValue()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        viewModel.FilterText = FilterQuery;
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit))
        {
            Console.WriteLine(view.FilterTextBox.Text);
        }

        // Output:
        // car
    }

    /// <summary>Observes both properties when no stream is given, as <c>Bind</c> does.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task ObserveBothPropertiesWithoutSignal()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        IObservable<EventArgs>? noSignal = default;

        using (view.BindUnsafe(viewModel, x => x.FilterText, v => v.FilterTextBox.Text, noSignal))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine(viewModel.FilterText);

            viewModel.FilterText = SecondFilterQuery;

            Console.WriteLine(view.FilterTextBox.Text);
        }

        // Output:
        // car
        // tax
    }

    /// <summary>Commits an amount typed as text, with a conversion function for each direction.</summary>
    public static void CommitConvertedViewEditOnSignal()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            commit))
        {
            view.AmountTextBox.Text = AmountText;

            Console.WriteLine(viewModel.Draft.Amount);

            commit.OnNext(EventArgs.Empty);

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 0
        // 125.50
    }

    /// <summary>Refreshes an amount shown as text when the stream fires, with a conversion function for each direction.</summary>
    public static void RefreshConvertedViewOnSignal()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            refresh,
            TriggerUpdate.ViewModelToView))
        {
            viewModel.Draft.Amount = AmountValue;

            Console.WriteLine(view.AmountTextBox.Text);

            refresh.OnNext(EventArgs.Empty);

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 0.00
        // 125.50
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
