// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>
/// Demonstrates the runtime fallback that serves a call the generator could not read. The fallback types are the
/// engine behind every <c>Unsafe</c> method; you can call them directly with an expression and the text to report when
/// a write fails.
/// </summary>
public static class FallbackRuntimeExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The status a draft item shows, held as an object the way a picker holds its selection.</summary>
    private const string DraftStatus = "Draft";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text that replaces <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The title an example adds through a bound button.</summary>
    private const string NewItemTitle = "Water the plants";

    /// <summary>The amount of a transfer, as the view shows it.</summary>
    private const string AmountText = "125.50";

    /// <summary>The amount of a transfer.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The text a failed write reports as the bound expression, for the filter.</summary>
    private const string FilterExpression = "x => x.FilterText";

    /// <summary>The text a failed write reports as the bound expression, for the amount.</summary>
    private const string AmountExpression = "x => x.Draft.Amount";

    /// <summary>The text a failed write reports as the bound expression, for the selected title.</summary>
    private const string SelectedTitleExpression = "v => v.SelectedTitleTextBox.Text";

    /// <summary>The text a failed write reports as the bound expression, for the add command.</summary>
    private const string AddCommandExpression = "x => x.AddCommand";

    /// <summary>The text a failed write reports as the bound expression, for the remaining count.</summary>
    private const string RemainingCountExpression = "x => x.RemainingCount";

    /// <summary>The name of the click event of a button.</summary>
    private const string ClickedEventName = nameof(Button.Clicked);

    /// <summary>Writes a source property to a target property on the thread that owns the target.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayBetweenProperties()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();

        using (RuntimeBindingFallback.BindOneWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            viewModel.FilterText = FilterQuery;
        }

        Console.WriteLine(view.FilterTextBox.Text);

        // Output:
        // car
    }

    /// <summary>Delivers each write on a sequencer you supply, here one that runs its work when the example asks.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayOnSequencer()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        VirtualClock sequencer = new();

        using (RuntimeBindingFallback.BindOneWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, sequencer, FilterExpression))
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

    /// <summary>Writes a count to a label as text, through a conversion function.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindOneWayWithConversion()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();

        using (RuntimeBindingFallback.BindOneWay(
            viewModel,
            view,
            x => x.RemainingCount,
            v => v.RemainingLabel.Text,
            static count => count.ToString(CultureInfo.InvariantCulture),
            null,
            RemainingCountExpression))
        {
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
    }

    /// <summary>Carries a property both ways between two objects.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindTwoWayBetweenProperties()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();

        using (RuntimeBindingFallback.BindTwoWay(viewModel, view, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
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

    /// <summary>Carries an amount both ways as text, with a pair of conversions.</summary>
    public static void BindTwoWayWithConverterPair()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new();

        using (RuntimeBindingFallback.BindTwoWay(viewModel, view, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), null, AmountExpression))
        {
            view.AmountTextBox.Text = AmountText;

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 125.50
    }

    /// <summary>Binds a view model property to a view property, written view first.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindToView()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.OneWayBind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            viewModel.FilterText = FilterQuery;
        }

        Console.WriteLine(view.FilterTextBox.Text);

        // Output:
        // car
    }

    /// <summary>Binds a count to a label as text, written view first, through a conversion function.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task OneWayBindToViewWithConversion()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            x => x.RemainingCount,
            v => v.RemainingLabel.Text,
            static count => $"{count} left",
            null,
            RemainingCountExpression))
        {
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3 left
    }

    /// <summary>Carries a property both ways, written view first.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindViewAndViewModel()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, null, FilterExpression))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine(viewModel.FilterText);
        }

        // Output:
        // car
    }

    /// <summary>Carries an amount both ways as text, written view first, with a pair of conversions.</summary>
    public static void BindViewAndViewModelWithConverterPair()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), null, AmountExpression))
        {
            view.AmountTextBox.Text = AmountText;

            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 125.50
    }

    /// <summary>Holds a view edit until a stream fires, then writes it to the view model.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindViewAndViewModelOnSignal()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        Signal<EventArgs> commit = new();

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.FilterText, v => v.FilterTextBox.Text, commit, TriggerUpdate.ViewToViewModel))
        {
            view.FilterTextBox.Text = FilterQuery;

            Console.WriteLine($"'{viewModel.FilterText}'");

            commit.OnNext(EventArgs.Empty);

            Console.WriteLine($"'{viewModel.FilterText}'");
        }

        // Output:
        // ''
        // 'car'
    }

    /// <summary>Holds an amount until a stream fires, then writes it to the view as text, with a pair of conversions.</summary>
    public static void BindViewAndViewModelWithConverterPairOnSignal()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<EventArgs> refresh = new();

        using (RuntimeBindingFallback.Bind(view, viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, CreateAmountConverters(), refresh, TriggerUpdate.ViewModelToView))
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

    /// <summary>Writes each selected item of a stream to a text property, through the converter registered for the two types.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindStreamToProperty()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        IObservable<TodoItem?> selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);

        using (RuntimeBindingFallback.BindTo(selected, view, v => v.SelectedTitleTextBox.Text, null, null, null, SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // Renew car registration
    }

    /// <summary>Writes each selected item of a stream to a text property, through a converter you name and a conversion hint.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindStreamToPropertyWithHintAndConverter()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new();
        IObservable<TodoItem?> selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);

        using (RuntimeBindingFallback.BindTo(
            selected,
            view,
            v => v.SelectedTitleTextBox.Text,
            TodoItemTitleConverter.UpperCaseHint,
            new TodoItemTitleConverter(),
            null,
            SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(view.SelectedTitleTextBox.Text);
        }

        // Output:
        // RENEW CAR REGISTRATION
    }

    /// <summary>Drops the values of a stream when the target is null, as a view has no property to write before it exists.</summary>
    /// <returns>A task that completes when the view model has loaded.</returns>
    public static async Task BindStreamToNullTarget()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        IObservable<TodoItem?> selected = RuntimeObservationFallback.WhenChanged(viewModel, x => x.SelectedItem);
        TodoView? missing = default;

        using (RuntimeBindingFallback.BindTo(selected, missing, v => v.SelectedTitleTextBox.Text, null, null, null, SelectedTitleExpression))
        {
            viewModel.SelectedItem = viewModel.Items[0];

            Console.WriteLine(ReferenceEquals(viewModel.Items[0], viewModel.SelectedItem));
        }

        // Output:
        // True
    }

    /// <summary>Converts a value with the converter registered for the two types.</summary>
    public static void ConvertWithRegisteredConverter()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        bool converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, null, null, out var text);

        Console.WriteLine(converted);
        Console.WriteLine(text);

        // Output:
        // True
        // Renew car registration
    }

    /// <summary>Converts a value with a conversion hint that the converter reads.</summary>
    public static void ConvertWithConversionHint()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        bool converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, TodoItemTitleConverter.UpperCaseHint, null, out var text);

        Console.WriteLine(converted);
        Console.WriteLine(text);

        // Output:
        // True
        // RENEW CAR REGISTRATION
    }

    /// <summary>Converts a value with a converter you name, which takes precedence over the registered ones.</summary>
    public static void ConvertWithConverterOverride()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        bool converted = RuntimeBindingConverter.TryConvert<TodoItem, string>(item, TodoItemTitleConverter.UpperCaseHint, new TodoItemTitleConverter(), out var text);

        Console.WriteLine(converted);
        Console.WriteLine(text);

        // Output:
        // True
        // RENEW CAR REGISTRATION
    }

    /// <summary>Reports failure when no converter is registered for the two types.</summary>
    public static void ConvertWithoutConverterFails()
    {
        TodoItem item = new TodoItem { Title = OriginalTitle };

        bool converted = RuntimeBindingConverter.TryConvert<TodoItem, int>(item, null, null, out var number);

        Console.WriteLine(converted);
        Console.WriteLine(number);

        // Output:
        // False
        // 0
    }

    /// <summary>Passes a value through when nothing is registered for the two types and the value already is the target type.</summary>
    public static void ConvertValueThatAlreadyHasTheTargetType()
    {
        object status = DraftStatus;

        // Nothing converts object to string, but the value is a string, so it passes through as it is.
        bool converted = RuntimeBindingConverter.TryConvert<object, string>(status, null, null, out var text);

        Console.WriteLine(converted);
        Console.WriteLine(text);

        // Output:
        // True
        // Draft
    }

    /// <summary>Pairs a forward and a reverse conversion with <see cref="TwoWayConverters.Create"/>, which infers both types.</summary>
    public static void CreateConverterPairWithFactory()
    {
        TwoWayConverterPair<decimal, string> pair = TwoWayConverters.Create<decimal, string>(
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture));

        Console.WriteLine(pair.Forward(AmountValue));
        Console.WriteLine(pair.Reverse(AmountText));

        // Output:
        // 125.50
        // 125.50
    }

    /// <summary>Pairs a forward and a reverse conversion with the constructor; a pair is a value that compares by its two functions.</summary>
    public static void CreateConverterPairWithConstructor()
    {
        Func<decimal, string> forward = static amount => amount.ToString("F2", CultureInfo.InvariantCulture);
        Func<string, decimal> reverse = static text => decimal.Parse(text, CultureInfo.InvariantCulture);
        TwoWayConverterPair<decimal, string> pair = new(forward, reverse);
        TwoWayConverterPair<decimal, string> same = new(forward, reverse);
        TwoWayConverterPair<decimal, string> swapped = pair with { Forward = static amount => amount.ToString("F0", CultureInfo.InvariantCulture) };

        Console.WriteLine(pair.Equals(same));
        Console.WriteLine(pair.Equals(swapped));
        Console.WriteLine(swapped.Forward(AmountValue));

        // Output:
        // True
        // False
        // 126
    }

    /// <summary>Binds a command to a button; each click runs the command.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButton()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        TaskCompletionSource itemAdded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.Items))
            {
                _ = itemAdded.TrySetResult();
            }
        };

        using (RuntimeCommandBindingFallback.BindCommand(view, viewModel, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), null, AddCommandExpression))
        {
            viewModel.NewTitle = NewItemTitle;
            ((IButtonController)view.AddButton).SendClicked();
            await itemAdded.Task;
        }

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 5
    }

    /// <summary>Binds a command to a named event of a button.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task BindCommandToButtonEvent()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        TodoView view = new() { ViewModel = viewModel };
        TaskCompletionSource itemAdded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.Items))
            {
                _ = itemAdded.TrySetResult();
            }
        };

        using (RuntimeCommandBindingFallback.BindCommand(view, viewModel, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), ClickedEventName, AddCommandExpression))
        {
            viewModel.NewTitle = NewItemTitle;
            ((IButtonController)view.AddButton).SendClicked();
            await itemAdded.Task;
        }

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 5
    }

    /// <summary>Does nothing while there is no view model, the ordinary state before one is assigned.</summary>
    public static void BindCommandWithoutViewModel()
    {
        TodoView view = new();
        TodoListViewModel? missing = default;

        using (RuntimeCommandBindingFallback.BindCommand(view, missing, x => x.AddCommand, v => v.AddButton, Signal.Never<object?>(), null, AddCommandExpression))
        {
            Console.WriteLine(view.AddButton.Command is null);
        }

        // Output:
        // True
    }

    /// <summary>Runs a command for each selected item of a stream.</summary>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task InvokeCommandForStream()
    {
        TodoListViewModel viewModel = await LoadTodoAsync();
        IObservable<TodoItem?> selected = RuntimeObservationFallback.WhenAnyValue(viewModel, x => x.SelectedItem);
        TaskCompletionSource completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.RemainingCount))
            {
                _ = completed.TrySetResult();
            }
        };

        using (RuntimeCommandFallback.InvokeCommand(selected, viewModel, x => x.CompleteCommand))
        {
            viewModel.SelectedItem = viewModel.Items[0];
            await completed.Task;
        }

        Console.WriteLine(viewModel.RemainingCount);

        // Output:
        // 2
    }

    /// <summary>Registers a handler against the interaction a property holds.</summary>
    /// <returns>A task that completes when the interaction has been answered.</returns>
    public static async Task BindInteractionToHandler()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());

        using (RuntimeInteractionFallback.BindInteraction(
            viewModel,
            x => x.ConfirmTransfer,
            static interaction => interaction.RegisterHandler(static context =>
            {
                context.SetOutput(true);
                return Task.CompletedTask;
            }),
            "x => x.ConfirmTransfer"))
        {
            bool confirmed = await viewModel.ConfirmTransfer.Handle(viewModel.Draft);

            Console.WriteLine(confirmed);
        }

        // Output:
        // True
    }

    /// <summary>Builds the conversions that show an amount as text.</summary>
    /// <returns>The forward and reverse conversions.</returns>
    private static TwoWayConverterPair<decimal, string> CreateAmountConverters() =>
        TwoWayConverters.Create<decimal, string>(
            static amount => amount.ToString("F2", CultureInfo.InvariantCulture),
            static text => decimal.Parse(text, CultureInfo.InvariantCulture));

    /// <summary>Creates a view model that has loaded the seeded items.</summary>
    /// <returns>The loaded view model.</returns>
    private static async Task<TodoListViewModel> LoadTodoAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        await viewModel.LoadAsync();
        return viewModel;
    }
}
