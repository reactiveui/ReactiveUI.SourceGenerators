// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Passes a converter object to <c>BindOneWay</c>, <c>BindTwoWay</c>, <c>OneWayBind</c>, <c>Bind</c> and <c>BindTo</c>.</summary>
public static class BindingConverterOverloadsExamples
{
    /// <summary>The title of the car registration task.</summary>
    private const string CarRegistrationTitle = "Renew car registration";

    /// <summary>The identifier of the dentist task in the seeded to-do database.</summary>
    private const int DentistTaskId = 2;

    /// <summary>The amount the customer sends.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>The text the customer types in the amount box.</summary>
    private const string TypedAmount = "99.95";

    /// <summary>The text the customer types in the due date box.</summary>
    private const string TypedDueDate = "2026-05-01";

    /// <summary>Shows the priority of a task as a colour name with <c>BindOneWay</c> and a converter.</summary>
    public static void BindOneWayWithConverter()
    {
        TodoItem item = new() { Title = CarRegistrationTitle, Priority = TodoPriority.High };
        Label badge = new();

        using IDisposable binding = item.BindOneWay(badge, x => x.Priority, x => x.Text, new TodoPriorityToColorConverter());
        Console.WriteLine(badge.Text);

        item.Priority = TodoPriority.Low;
        Console.WriteLine(badge.Text);

        // Output:
        // Red
        // Gray
    }

    /// <summary>Ticks a check box while the priority equals the conversion hint.</summary>
    public static void BindOneWayWithConversionHint()
    {
        TodoItem item = new() { Title = CarRegistrationTitle, Priority = TodoPriority.High };
        CheckBox highPriorityBox = new();

        using IDisposable binding = item.BindOneWay(highPriorityBox, x => x.Priority, x => x.IsChecked, new PriorityMatchesHintConverter(), TodoPriority.High);
        Console.WriteLine(highPriorityBox.IsChecked);

        item.Priority = TodoPriority.Low;
        Console.WriteLine(highPriorityBox.IsChecked);

        // Output:
        // True
        // False
    }

    /// <summary>Keeps a due date and its text box in step with <c>BindTwoWay</c> and a converter for each direction.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task BindTwoWayWithConverters()
    {
        InMemoryTodoStore store = InMemoryTodoStore.CreateSeeded();
        TodoItem item = (await store.GetAsync(DentistTaskId))!;
        Entry dueDateBox = new();

        using IDisposable binding = item.BindTwoWay(
            dueDateBox,
            x => x.DueDate,
            x => x.Text,
            new NullableDateOnlyToStringTypeConverter(),
            new StringToNullableDateOnlyTypeConverter());
        Console.WriteLine(dueDateBox.Text);

        dueDateBox.Text = TypedDueDate;
        Console.WriteLine(item.DueDate);

        // Output:
        // 04/02/2026
        // 05/01/2026
    }

    /// <summary>Shows the total balance as text with <c>OneWayBind</c> and a converter.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task OneWayBindWithConverter()
    {
        AccountsView view = new();
        AccountsViewModel viewModel = new(new InMemoryBankingBackend());
        view.ViewModel = viewModel;

        using IReactiveBinding<AccountsView, string> binding = view.OneWayBind(viewModel, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new DecimalToStringTypeConverter());
        await viewModel.LoadAccountsAsync();

        Console.WriteLine(view.TotalBalanceLabel.Text);

        // Output:
        // 17680.75
    }

    /// <summary>Keeps the transfer amount and its text box in step with <c>Bind</c> and a converter for each direction.</summary>
    public static void BindWithConverters()
    {
        TransferView view = new();
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        view.ViewModel = viewModel;

        using IReactiveBinding<TransferView, BindingChange> binding = view.Bind(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            new DecimalToStringTypeConverter(),
            new StringToDecimalTypeConverter());

        viewModel.Draft.Amount = TransferAmount;
        Console.WriteLine(view.AmountTextBox.Text);

        view.AmountTextBox.Text = TypedAmount;
        Console.WriteLine(viewModel.Draft.Amount);

        // Output:
        // 250.50
        // 99.95
    }

    /// <summary>Ticks a check box from a stream with <c>BindTo</c>, a conversion hint and a converter.</summary>
    public static void BindToWithConversionHintAndConverter()
    {
        TodoItem item = new() { Title = CarRegistrationTitle, Priority = TodoPriority.High };
        CheckBox highPriorityBox = new();

        using (item.WhenChanged(x => x.Priority).BindTo(highPriorityBox, x => x.IsChecked, TodoPriority.High, new PriorityMatchesHintConverter()))
        {
            Console.WriteLine(highPriorityBox.IsChecked);

            item.Priority = TodoPriority.Low;
            Console.WriteLine(highPriorityBox.IsChecked);
        }

        // Output:
        // True
        // False
    }
}
