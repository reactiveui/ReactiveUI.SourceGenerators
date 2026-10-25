// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq.Expressions;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates the <c>Unsafe</c> binding overloads that take a sequencer and the overloads that take converters.
/// Each write to the target waits on a <see cref="VirtualClock"/> until the example advances it, as a UI thread does
/// between two turns of its message loop. Each property path here is held in a variable.
/// </summary>
public static class UnsafeSequencerExamples
{
    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text that replaces <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The amount a user types over an amount already shown.</summary>
    private const string SecondAmountText = "10.00";

    /// <summary>Writes a source property to a target property on a sequencer.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (viewModel.BindOneWayUnsafe(view, source, target, sequencer))
        {
            viewModel.FilterText = FilterQuery;

            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            sequencer.Start();

            Console.WriteLine($"'{view.FilterTextBox.Text}'");
        }

        // Output:
        // ''
        // 'car'
    }

    /// <summary>Writes a count to a label as text on a sequencer, through a conversion function.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayWithConversionOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, int>> source = x => x.RemainingCount;
        Expression<Func<TodoView, string>> target = v => v.RemainingLabel.Text;

        using (viewModel.BindOneWayUnsafe(view, source, target, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            Console.WriteLine($"'{view.RemainingLabel.Text}'");

            sequencer.Start();

            Console.WriteLine($"'{view.RemainingLabel.Text}'");
        }

        // Output:
        // ''
        // '3'
    }

    /// <summary>Writes the selected item to a text property on a sequencer, through a converter you name and a conversion hint.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayWithConverterOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string>> target = v => v.SelectedTitleTextBox.Text;

        using (viewModel.BindOneWayUnsafe(view, source, target, new TodoItemTitleConverter(), sequencer, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            sequencer.Start();

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // RENEW CAR REGISTRATION
    }

    /// <summary>Carries a property both ways; the write to the target waits on a sequencer and the write back does not.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindTwoWayOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, string>> source = x => x.FilterText;
        Expression<Func<TodoView, string>> target = v => v.FilterTextBox.Text;

        using (viewModel.BindTwoWayUnsafe(view, source, target, sequencer))
        {
            viewModel.FilterText = FilterQuery;

            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            sequencer.Start();

            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            view.FilterTextBox.Text = SecondFilterQuery;

            Console.WriteLine($"'{viewModel.FilterText}'");
        }

        // Output:
        // ''
        // 'car'
        // 'tax'
    }

    /// <summary>Carries an amount both ways as text on a sequencer, with a conversion function for each direction.</summary>
    public static void BindTwoWayWithConversionsOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (viewModel.BindTwoWayUnsafe(
            view,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            sequencer.Start();
            viewModel.Draft.Amount = AmountValue;
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 125.50
    }

    /// <summary>Carries an amount both ways as text on a sequencer, through a converter for each direction and a conversion hint.</summary>
    public static void BindTwoWayWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new();
        VirtualClock sequencer = new();
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (viewModel.BindTwoWayUnsafe(view, source, target, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), sequencer, null))
        {
            sequencer.Start();
            viewModel.Draft.Amount = AmountValue;
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = SecondAmountText;

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 125.50
        // 10.00
    }

    /// <summary>Carries an amount both ways as text, written view first, on a sequencer, with a conversion function for each direction.</summary>
    public static void BindWithConversionsOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (view.BindUnsafe(
            viewModel,
            source,
            target,
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            sequencer.Start();
            viewModel.Draft.Amount = AmountValue;
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 125.50
    }

    /// <summary>Carries an amount both ways as text, written view first, on a sequencer, through a converter for each direction.</summary>
    public static void BindWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        Expression<Func<TransferViewModel, decimal>> source = x => x.Draft.Amount;
        Expression<Func<TransferView, string>> target = v => v.AmountTextBox.Text;

        using (view.BindUnsafe(viewModel, source, target, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), sequencer, null))
        {
            sequencer.Start();
            viewModel.Draft.Amount = AmountValue;
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 125.50
    }

    /// <summary>Binds a count to a label as text, written view first, on a sequencer, through a selector.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindWithSelectorOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, int>> source = x => x.RemainingCount;
        Expression<Func<TodoView, string>> target = v => v.RemainingLabel.Text;

        using (view.OneWayBindUnsafe(viewModel, source, target, static count => $"{count} left", sequencer))
        {
            Console.WriteLine($"'{view.RemainingLabel.Text}'");

            sequencer.Start();

            Console.WriteLine($"'{view.RemainingLabel.Text}'");
        }

        // Output:
        // ''
        // '3 left'
    }

    /// <summary>Binds the selected item to a text property, written view first, on a sequencer, through a converter you name and a conversion hint.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindWithConverterOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        Expression<Func<TodoListViewModel, TodoItem?>> source = x => x.SelectedItem;
        Expression<Func<TodoView, string>> target = v => v.SelectedTitleTextBox.Text;

        using (view.OneWayBindUnsafe(viewModel, source, target, new TodoItemTitleConverter(), sequencer, TodoItemTitleConverter.UpperCaseHint))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            sequencer.Start();

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // RENEW CAR REGISTRATION
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
