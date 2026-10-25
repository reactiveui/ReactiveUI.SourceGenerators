// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows how a binding delivers its writes on a sequencer you pass in: the sequencer overloads of the binding APIs.</summary>
public static class SequencerExamples
{
    /// <summary>What the examples print for a control that has no text.</summary>
    private const string NoText = "(no text)";

    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title the user types on the UI thread.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The reference of the transfer the customer is filling in.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer types over the first one.</summary>
    private const string RentAprilReference = "Rent April";

    /// <summary>The reference that replaces the April one before the sequencer runs.</summary>
    private const string RentMayReference = "Rent May";

    /// <summary>The format that shows an amount with two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The amount of the transfer the customer is filling in.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The text of the first amount.</summary>
    private const string RentAmountText = "1200.00";

    /// <summary>The text of the amount the customer types over the first one.</summary>
    private const string RaisedRentAmountText = "1250.50";

    /// <summary>Delivers a one-way binding on a sequencer of your choice: the write, including the first one, waits for the sequencer.</summary>
    public static void BindOneWayOnSequencer()
    {
        Label titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        VirtualClock sequencer = new();

        using (item.BindOneWay(titleLabel, x => x.Title, x => x.Text, sequencer))
        {
            Console.WriteLine(titleLabel.Text ?? NoText);

            sequencer.Start();

            Console.WriteLine(titleLabel.Text);

            item.Title = RenamedTitle;

            Console.WriteLine(titleLabel.Text);

            sequencer.Start();

            Console.WriteLine(titleLabel.Text);
        }

        // Output:
        // (no text)
        // Renew car registration
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Converts with a function and delivers on a sequencer.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindOneWayWithConversionOnSequencerAsync()
    {
        Label remainingLabel = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        VirtualClock sequencer = new();

        using (viewModel.BindOneWay(remainingLabel, x => x.RemainingCount, x => x.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            await viewModel.LoadAsync();
            sequencer.Start();

            Console.WriteLine(remainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Binds two ways on a sequencer when the two sides start with different values: the view shows the view model once the sequencer runs.</summary>
    public static void BindTwoWayOnSequencer()
    {
        Entry referenceBox = new();
        TransferDraft draft = new() { Reference = RentReference };
        VirtualClock sequencer = new();

        using (draft.BindTwoWay(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            Console.WriteLine(referenceBox.Text ?? NoText);

            sequencer.Start();

            Console.WriteLine(referenceBox.Text);
            Console.WriteLine(draft.Reference);

            referenceBox.Text = RentAprilReference;
            sequencer.Start();

            Console.WriteLine(draft.Reference);
        }

        // Output:
        // (no text)
        // Rent March
        // Rent March
        // Rent April
    }

    /// <summary>Edits the view model twice before the sequencer runs: the view gets the second value and the binding goes idle.</summary>
    public static void BindTwoWayBurstOnSequencer()
    {
        Entry referenceBox = new();
        TransferDraft draft = new() { Reference = RentReference };
        VirtualClock sequencer = new();

        using (draft.BindTwoWay(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            sequencer.Start();

            draft.Reference = RentAprilReference;
            draft.Reference = RentMayReference;
            sequencer.Start();

            Console.WriteLine(referenceBox.Text);
            Console.WriteLine(draft.Reference);
        }

        // Output:
        // Rent May
        // Rent May
    }

    /// <summary>Binds two ways with a conversion function for each direction, on a sequencer.</summary>
    public static void BindTwoWayWithConversionOnSequencer()
    {
        Entry amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();

        using (draft.BindTwoWay(
            amountBox,
            x => x.Amount,
            x => x.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            ParseAmount,
            sequencer))
        {
            sequencer.Start();

            Console.WriteLine(amountBox.Text);

            amountBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 1250.50
    }

    /// <summary>Binds a view to its view model two ways, with a conversion function for each direction, on a sequencer.</summary>
    public static void BindViewOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.Bind(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            ParseAmount,
            sequencer))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 1200.00
        // 1250.50
    }

    /// <summary>Binds a view to its view model one way, with a selector, on a sequencer.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task OneWayBindViewOnSequencerAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();

        using (view.OneWayBind(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            await viewModel.LoadAsync();
            sequencer.Start();

            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Converts with a converter and a format hint, and delivers on a sequencer.</summary>
    public static void BindOneWayWithConverterOnSequencer()
    {
        Entry amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();

        using (draft.BindOneWay(amountBox, x => x.Amount, x => x.Text, new DecimalToStringTypeConverter(), TwoDecimalPlaces, sequencer))
        {
            sequencer.Start();

            Console.WriteLine(amountBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Binds two ways with a converter for each direction and a format hint, on a sequencer.</summary>
    public static void BindTwoWayWithConvertersOnSequencer()
    {
        Entry amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();

        using (draft.BindTwoWay(amountBox, x => x.Amount, x => x.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), TwoDecimalPlaces, sequencer))
        {
            sequencer.Start();

            Console.WriteLine(amountBox.Text);

            amountBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 1250.50
    }

    /// <summary>Binds a view to its view model two ways, with a converter for each direction and a format hint, on a sequencer.</summary>
    public static void BindViewWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.Bind(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter(), TwoDecimalPlaces, sequencer))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 1200.00
        // 1250.50
    }

    /// <summary>Binds a view to its view model one way, with a converter and a format hint, on a sequencer.</summary>
    public static void OneWayBindViewWithConverterOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.OneWayBind(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), TwoDecimalPlaces, sequencer))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Resolves a one-way binding on a sequencer at run time, for an expression the generator cannot read.</summary>
    public static void BindOneWayUnsafeOnSequencer()
    {
        Label titleLabel = new();
        TodoItem item = new() { Title = OriginalTitle };
        VirtualClock sequencer = new();

        using (item.BindOneWayUnsafe(titleLabel, x => x.Title, x => x.Text, sequencer))
        {
            sequencer.Start();

            Console.WriteLine(titleLabel.Text);
        }

        // Output:
        // Renew car registration
    }

    /// <summary>Resolves a one-way binding with a conversion function on a sequencer at run time.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindOneWayUnsafeWithConversionOnSequencerAsync()
    {
        Label remainingLabel = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        VirtualClock sequencer = new();

        using (viewModel.BindOneWayUnsafe(remainingLabel, x => x.RemainingCount, x => x.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            await viewModel.LoadAsync();
            sequencer.Start();

            Console.WriteLine(remainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Resolves a one-way binding with a converter and a format hint on a sequencer at run time.</summary>
    public static void BindOneWayUnsafeWithConverterOnSequencer()
    {
        Entry amountBox = new();
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();
        DecimalToStringTypeConverter converter = new();

        using (draft.BindOneWayUnsafe(amountBox, x => x.Amount, x => x.Text, converter, sequencer, TwoDecimalPlaces))
        {
            sequencer.Start();

            Console.WriteLine(amountBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Resolves a two-way binding on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeOnSequencer()
    {
        Entry referenceBox = new() { Text = RentReference };
        TransferDraft draft = new() { Reference = RentReference };
        VirtualClock sequencer = new();

        using (draft.BindTwoWayUnsafe(referenceBox, x => x.Reference, x => x.Text, sequencer))
        {
            sequencer.Start();

            referenceBox.Text = RentAprilReference;
            sequencer.Start();

            Console.WriteLine(draft.Reference);
        }

        // Output:
        // Rent April
    }

    /// <summary>Resolves a two-way binding with a conversion function for each direction on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeWithConversionOnSequencer()
    {
        Entry amountBox = new() { Text = RentAmountText };
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();

        using (draft.BindTwoWayUnsafe(
            amountBox,
            x => x.Amount,
            x => x.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            sequencer.Start();

            amountBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1250.50
    }

    /// <summary>Resolves a two-way binding with a converter for each direction and a format hint on a sequencer at run time.</summary>
    public static void BindTwoWayUnsafeWithConvertersOnSequencer()
    {
        Entry amountBox = new() { Text = RentAmountText };
        TransferDraft draft = new() { Amount = RentAmount };
        VirtualClock sequencer = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWayUnsafe(amountBox, x => x.Amount, x => x.Text, toText, toAmount, sequencer, TwoDecimalPlaces))
        {
            sequencer.Start();

            amountBox.Text = RaisedRentAmountText;
            sequencer.Start();

            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1250.50
    }

    /// <summary>Resolves a two-way view binding with a conversion function for each direction on a sequencer at run time.</summary>
    public static void BindViewUnsafeOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        viewModel.Draft.Amount = RentAmount;
        view.AmountTextBox.Text = RentAmountText;

        using (view.BindUnsafe(
            viewModel,
            x => x.Draft.Amount,
            v => v.AmountTextBox.Text,
            static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture),
            sequencer))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Resolves a two-way view binding with a converter for each direction and a format hint on a sequencer at run time.</summary>
    public static void BindViewUnsafeWithConvertersOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();
        viewModel.Draft.Amount = RentAmount;
        view.AmountTextBox.Text = RentAmountText;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, toText, toAmount, sequencer, TwoDecimalPlaces))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Resolves a one-way view binding with a selector on a sequencer at run time.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task OneWayBindViewUnsafeOnSequencerAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();

        using (view.OneWayBindUnsafe(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), sequencer))
        {
            await viewModel.LoadAsync();
            sequencer.Start();

            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Resolves a one-way view binding with a converter and a format hint on a sequencer at run time.</summary>
    public static void OneWayBindViewUnsafeWithConverterOnSequencer()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        VirtualClock sequencer = new();
        DecimalToStringTypeConverter converter = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.OneWayBindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, converter, sequencer, TwoDecimalPlaces))
        {
            sequencer.Start();

            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 1200.00
    }

    /// <summary>Reads an amount of money the customer typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The amount, or zero when the text is not a number.</returns>
    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0M;
}
