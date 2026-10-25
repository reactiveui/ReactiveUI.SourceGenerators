// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows the binding that <c>OneWayBind</c> and <c>Bind</c> return, and the update streams that decide when a two-way binding writes.</summary>
public static class ReactiveBindingExamples
{
    /// <summary>The size of the file the upload example sends: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>The number that turns an upload percentage into the fraction a progress bar shows.</summary>
    private const double PercentPerBar = 100D;

    /// <summary>The text of the filter the customer types.</summary>
    private const string CarFilter = "car";

    /// <summary>The text of a filter the view model sets.</summary>
    private const string DentistFilter = "dentist";

    /// <summary>The reference the draft starts with.</summary>
    private const string MarchReference = "Rent March";

    /// <summary>The reference the customer types.</summary>
    private const string AprilReference = "Rent April";

    /// <summary>The reference the view model sets.</summary>
    private const string MayReference = "Rent May";

    /// <summary>The amount the draft starts with.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The amount the customer types, as the box shows it.</summary>
    private const string TypedAmountText = "85.50";

    /// <summary>The score the view model starts with.</summary>
    private const decimal FirstScore = 64M;

    /// <summary>The score after a re-mark.</summary>
    private const decimal RemarkedScore = 91M;

    /// <summary>The score the teacher types.</summary>
    private const string TypedScoreText = "82.5";

    /// <summary>The format the amount converters use to show two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The format the score converters use to show one decimal place.</summary>
    private const string OneDecimalPlace = "F1";

    /// <summary>Reads what a one-way binding reports: its view, its direction and every value it writes, starting with the current one.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task ReadOneWayBinding()
    {
        StorageBrowserViewModel browser = new(InMemoryObjectStorage.CreateSeeded());
        await browser.LoadBucketsAsync();
        browser.SelectedBucket = browser.Buckets[0];
        StorageBrowserView view = new() { ViewModel = browser };
        List<double> written = [];

        using (var binding = view.OneWayBind(browser, x => x.UploadPercent, v => v.UploadProgressBar.Progress, static percent => percent / PercentPerBar))
        {
            Console.WriteLine(binding.Direction);
            Console.WriteLine(ReferenceEquals(view, binding.View));

            using (binding.Changed.Subscribe(written.Add))
            {
                await browser.UploadAsync(new("launch-video.mp4", UploadSizeBytes, "video/mp4"));
            }
        }

        // Changed replays the bar's starting value, so the empty bar is the first value a subscriber sees.
        Console.WriteLine(string.Join(", ", written));

        // Output:
        // OneWay
        // True
        // 0, 0.25, 0.5, 0.75, 1
    }

    /// <summary>Reads what a two-way binding reports: each <see cref="BindingChange"/> says which side produced the value.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task ReadTwoWayBinding()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };
        List<BindingChange> changes = [];

        using (var binding = view.Bind(list, x => x.FilterText, v => v.FilterTextBox.Text))
        using (binding.Changed.Subscribe(changes.Add))
        {
            Console.WriteLine(binding.Direction);
            Console.WriteLine(ReferenceEquals(view, binding.View));

            list.FilterText = CarFilter;
            view.FilterTextBox.Text = DentistFilter;
        }

        foreach (BindingChange change in changes)
        {
            Console.WriteLine($"{change.Value}, from the view model: {change.FromViewModel}");
        }

        // A change is a value with two parts, so it compares, deconstructs and copies like any record struct.
        var (value, fromViewModel) = changes[0];
        Console.WriteLine(value);
        Console.WriteLine(fromViewModel);
        BindingChange copy = changes[0] with { FromViewModel = false };
        Console.WriteLine($"{copy.Value}, from the view model: {copy.FromViewModel}");

        // Output:
        // TwoWay
        // True
        // car, from the view model: True
        // dentist, from the view model: False
        // car
        // True
        // car, from the view model: False
    }

    /// <summary>Reads the two expressions of a binding. A generated binding reads its paths at compile time, so it carries no expression trees.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task ReadBindingExpressions()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };

        using IReactiveBinding<TodoView, BindingChange> binding = view.Bind(list, x => x.FilterText, v => v.FilterTextBox.Text);

        Console.WriteLine(binding.ViewModelExpression is null);
        Console.WriteLine(binding.ViewExpression is null);

        // Output:
        // True
        // True
    }

    /// <summary>Wraps a subscription in a <see cref="ReactiveBinding{TView, TValue}"/>, which disposes it once however often the binding is disposed.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task CreateReactiveBinding()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };
        Signal<int> remainingChanges = new();
        IDisposable subscription = Scope.Create(static () => Console.WriteLine("subscription disposed"));

        ReactiveBinding<TodoView, int> binding = new(view, remainingChanges, BindingDirection.AsyncOneWay, subscription);
        List<int> seen = [];
        using IDisposable reader = binding.Changed.Subscribe(seen.Add);

        remainingChanges.OnNext(list.RemainingCount);

        Console.WriteLine(binding.Direction);
        Console.WriteLine(ReferenceEquals(view, binding.View));
        Console.WriteLine(binding.ViewExpression is null);
        Console.WriteLine(binding.ViewModelExpression is null);
        Console.WriteLine(string.Join(", ", seen));

        binding.Dispose();
        binding.Dispose();

        // Output:
        // AsyncOneWay
        // True
        // True
        // True
        // 3
        // subscription disposed
    }

    /// <summary>
    /// Drives the view-to-view-model direction from a stream: the customer's typing stays in the box until the box is
    /// left, and view model changes still reach the box at once. <see cref="TriggerUpdate.ViewToViewModel"/> is the default.
    /// </summary>
    public static void CommitReferenceWhenBoxIsLeft()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<RxVoid> boxLeft = new();
        viewModel.Draft.Reference = MarchReference;

        using (view.BindUnsafe(viewModel, x => x.Draft.Reference, v => v.ReferenceTextBox.Text, boxLeft))
        {
            Console.WriteLine(view.ReferenceTextBox.Text);

            view.ReferenceTextBox.Text = AprilReference;
            Console.WriteLine(viewModel.Draft.Reference);

            boxLeft.OnNext(RxVoid.Default);
            Console.WriteLine(viewModel.Draft.Reference);

            viewModel.Draft.Reference = MayReference;
            Console.WriteLine(view.ReferenceTextBox.Text);
        }

        // Output:
        // Rent March
        // Rent March
        // Rent April
        // Rent May
    }

    /// <summary>Converts the amount in each direction; the typed amount is read when the box is left, as <see cref="TriggerUpdate.ViewToViewModel"/> does by default.</summary>
    public static void CommitAmountWhenBoxIsLeft()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        Signal<RxVoid> boxLeft = new();
        viewModel.Draft.Amount = RentAmount;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, boxLeft))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(viewModel.Draft.Amount);

            boxLeft.OnNext(RxVoid.Default);
            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 1200.00
        // 1200
        // 85.50
    }

    /// <summary>
    /// Drives the view-model-to-view direction from a stream: the view model's score reaches the box when the stream
    /// signals a refresh, while the teacher's typing reaches the view model at once.
    /// </summary>
    public static void RefreshScoreOnSignal()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        Signal<RxVoid> refresh = new();

        using (view.BindUnsafe(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, FormatScore, ParseScore, refresh, TriggerUpdate.ViewModelToView))
        {
            Console.WriteLine(view.ScoreEntry.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            Console.WriteLine(view.ScoreEntry.Text);

            refresh.OnNext(RxVoid.Default);
            Console.WriteLine(view.ScoreEntry.Text);

            view.ScoreEntry.Text = TypedScoreText;
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // 64.0
        // 64.0
        // 91.0
        // 82.5
    }

    /// <summary>Uses <see cref="TriggerUpdate.ViewModelToView"/> without converters; the registered converter for the pair runs.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task RefreshFilterOnSignal()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };
        Signal<RxVoid> refresh = new();

        using (view.BindUnsafe(list, x => x.FilterText, v => v.FilterTextBox.Text, refresh, TriggerUpdate.ViewModelToView))
        {
            list.FilterText = CarFilter;
            Console.WriteLine($"'{view.FilterTextBox.Text}'");

            refresh.OnNext(RxVoid.Default);
            Console.WriteLine(view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            Console.WriteLine(list.FilterText);
        }

        // Output:
        // ''
        // car
        // dentist
    }

    /// <summary>Passes no stream: both properties are observed, so the binding writes in both directions as they change.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task ObserveBothWithoutStream()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };

        using (view.BindUnsafe(list, x => x.FilterText, v => v.FilterTextBox.Text, (IObservable<RxVoid>?)null))
        {
            list.FilterText = CarFilter;
            Console.WriteLine(view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            Console.WriteLine(list.FilterText);
        }

        // Output:
        // car
        // dentist
    }

    /// <summary>Passes no stream to the converting overload, so both properties are observed.</summary>
    public static void ObserveBothWithConvertersAndWithoutStream()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        viewModel.Draft.Amount = RentAmount;

        using (view.BindUnsafe(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, (IObservable<RxVoid>?)null))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
    }

    /// <summary>Shows the amount of money with two decimal places.</summary>
    /// <param name="amount">The amount of money.</param>
    /// <returns>The amount as text.</returns>
    private static string FormatAmount(decimal amount) => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture);

    /// <summary>Reads an amount of money the customer typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The amount, or zero when the text is not a number.</returns>
    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0M;

    /// <summary>Shows a score with one decimal place.</summary>
    /// <param name="score">The score.</param>
    /// <returns>The score as text.</returns>
    private static string FormatScore(decimal score) => score.ToString(OneDecimalPlace, CultureInfo.InvariantCulture);

    /// <summary>Reads a score the teacher typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The score, or zero when the text is not a number.</returns>
    private static decimal ParseScore(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var score) ? score : 0M;

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>A task that completes with the list, which holds three unfinished items and one finished item.</returns>
    private static async Task<TodoListViewModel> OpenTodoListAsync()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());

        await list.LoadAsync();
        return list;
    }
}
