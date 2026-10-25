// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows how <c>BindTwoWay</c> and <c>Bind</c> keep a view model property and a view property equal in both directions.</summary>
public static class TwoWayBindingExamples
{
    /// <summary>The text the user types to narrow the to-do list to one item.</summary>
    private const string DentistFilter = "dentist";

    /// <summary>The tag the user picks to narrow the to-do list.</summary>
    private const string HealthTag = "health";

    /// <summary>The text a view model sets to narrow the to-do list to one item.</summary>
    private const string CarFilter = "car";

    /// <summary>The access token of the signed-in user, as the user types it.</summary>
    private const string TypedToken = "token-priya";

    /// <summary>The access token a view model sets after a token refresh.</summary>
    private const string RefreshedToken = "token-refreshed";

    /// <summary>The amount of the transfer in the view model.</summary>
    private const decimal RentAmount = 1200M;

    /// <summary>The amount the customer types.</summary>
    private const string TypedAmountText = "85.50";

    /// <summary>The text of an amount that is not a number.</summary>
    private const string NotAnAmountText = "twelve";

    /// <summary>The score the teacher typed.</summary>
    private const string TypedScoreText = "82.5";

    /// <summary>The score the view model starts with.</summary>
    private const decimal FirstScore = 64M;

    /// <summary>The score a teacher records after a re-mark.</summary>
    private const decimal RemarkedScore = 91M;

    /// <summary>The reference of the transfer the customer is filling in.</summary>
    private const string RentMarchReference = "Rent March";

    /// <summary>The reference the customer types over the first one.</summary>
    private const string RentAprilReference = "Rent April";

    /// <summary>The reference that replaces the April one before the sequencer runs.</summary>
    private const string RentMayReference = "Rent May";

    /// <summary>The format the amount converters use to show two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The format the score converters use to show one decimal place.</summary>
    private const string OneDecimalPlace = "F1";

    /// <summary>The time a sequencer stands still before it runs the writes that wait for it.</summary>
    private static readonly TimeSpan NextTurn = TimeSpan.FromMilliseconds(1);

    /// <summary>Keeps the filter of the to-do list and the filter box equal with <c>BindTwoWay</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTodoFilterToTextBox()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();

        using (list.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        {
            list.FilterText = CarFilter;
            Console.WriteLine(view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            Console.WriteLine(list.FilterText);
            Console.WriteLine(list.Items.Count);
        }

        // Disposing the binding disconnects both directions.
        view.FilterTextBox.Text = string.Empty;
        Console.WriteLine(list.FilterText);

        list.FilterText = CarFilter;
        Console.WriteLine($"'{view.FilterTextBox.Text}'");

        // Output:
        // car
        // dentist
        // 1
        // dentist
        // ''
    }

    /// <summary>Keeps the amount of a transfer and the amount box equal, converting with a function in each direction.</summary>
    public static void BindTransferAmountWithConverters()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(draft.Amount);

            // The reverse converter decides what text that is not a number means.
            view.AmountTextBox.Text = NotAnAmountText;
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
        // 0
    }

    /// <summary>Delivers the writes of a two-way binding on a named sequencer, which waits until the host runs it. The box starts empty and the view model holds a token.</summary>
    public static void BindTokenOnSequencer()
    {
        IssueBoardViewModel board = new(InMemoryGitHubServer.CreateSeeded()) { Token = InMemoryGitHubServer.PriyaToken };
        IssueBoardView view = new();
        VirtualClock uiThread = new();

        using (board.BindTwoWay(view, x => x.Token, v => v.TokenTextBox.Text, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.TokenTextBox.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.TokenTextBox.Text}'");
            Console.WriteLine(board.Token);

            board.Token = RefreshedToken;
            Console.WriteLine($"Waiting: '{view.TokenTextBox.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.TokenTextBox.Text}'");

            view.TokenTextBox.Text = TypedToken;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(board.Token);
        }

        // Output:
        // Waiting: ''
        // Written: 'ghp_priya_2f9c1d'
        // ghp_priya_2f9c1d
        // Waiting: 'ghp_priya_2f9c1d'
        // Written: 'token-refreshed'
        // token-priya
    }

    /// <summary>Edits the view model twice before the sequencer runs: the view shows the second value and the binding goes idle.</summary>
    public static void BindReferenceBurstOnSequencer()
    {
        TransferDraft draft = new() { Reference = RentMarchReference };
        TransferView view = new();
        VirtualClock uiThread = new();

        using (draft.BindTwoWay(view, x => x.Reference, v => v.ReferenceTextBox.Text, uiThread))
        {
            // The view is empty and the view model holds "Rent March": the view shows the view model once the sequencer runs.
            Console.WriteLine($"Waiting: '{view.ReferenceTextBox.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.ReferenceTextBox.Text}'");

            draft.Reference = RentAprilReference;
            draft.Reference = RentMayReference;
            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine(view.ReferenceTextBox.Text);
            Console.WriteLine(draft.Reference);
        }

        // Output:
        // Waiting: ''
        // Written: 'Rent March'
        // Rent May
        // Rent May
    }

    /// <summary>Converts the amount in each direction and delivers the writes on a named sequencer. The box starts empty.</summary>
    public static void BindTransferAmountWithConvertersOnSequencer()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        VirtualClock uiThread = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, uiThread))
        {
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
    }

    /// <summary>Converts with a converter object in each direction; without a hint the decimal converter uses the current culture's default text.</summary>
    public static void BindTransferAmountWithConverterObjects()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200
        // 85.50
    }

    /// <summary>Passes a hint to both converters: the text is the number format the amount is shown in.</summary>
    public static void BindTransferAmountWithConverterObjectsAndHint()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, TwoDecimalPlaces))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
    }

    /// <summary>Passes converter objects, a hint and a named sequencer.</summary>
    public static void BindTransferAmountWithConverterObjectsOnSequencer()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        VirtualClock uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, TwoDecimalPlaces, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.AmountTextBox.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.AmountTextBox.Text}'");

            view.AmountTextBox.Text = TypedAmountText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // Waiting: ''
        // Written: '1200.00'
        // 85.50
    }

    /// <summary>
    /// Resolves the paths of the same converter-object binding while the app runs, with <c>BindTwoWayUnsafe</c>. Use it
    /// when a path is not an inline lambda.
    /// </summary>
    public static void BindTransferAmountUnsafeWithConverterObjects()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();
        VirtualClock uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toAmount = new();

        using (draft.BindTwoWayUnsafe(view, x => x.Amount, v => v.AmountTextBox.Text, toText, toAmount, uiThread, TwoDecimalPlaces))
        {
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
    }

    /// <summary>Binds a string to a picker's selection, which the picker holds as an object, with the view-first <c>Bind</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindFilterToPickerSelection()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoTagFilterView view = new() { ViewModel = list };

        // No converter is registered from object to string; a string in SelectedItem passes through as it is.
        using (view.Bind(list, x => x.FilterText, v => v.TagPicker.SelectedItem))
        {
            view.TagPicker.SelectedItem = HealthTag;
            Console.WriteLine(list.FilterText);

            list.FilterText = CarFilter;
            Console.WriteLine(view.TagPicker.SelectedItem);
        }

        // Output:
        // health
        // car
    }

    /// <summary>Binds the view's filter box with the view-first <c>Bind</c>, and reads each change from the returned binding.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTodoFilterInView()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };
        List<BindingChange> changes = [];

        using (var binding = view.Bind(list, x => x.FilterText, v => v.FilterTextBox.Text))
        using (binding.Changed.Subscribe(changes.Add))
        {
            Console.WriteLine(binding.Direction);

            list.FilterText = CarFilter;
            Console.WriteLine(view.FilterTextBox.Text);

            view.FilterTextBox.Text = DentistFilter;
            Console.WriteLine(list.FilterText);
        }

        // FromViewModel tells an edit apart from the echo of the binding's own write.
        foreach (BindingChange change in changes)
        {
            Console.WriteLine($"{change.Value}, from the view model: {change.FromViewModel}");
        }

        // Output:
        // TwoWay
        // car
        // dentist
        // car, from the view model: True
        // dentist, from the view model: False
    }

    /// <summary>Keeps the amount of the transfer draft and the amount box equal through the view-first <c>Bind</c>, with a function in each direction.</summary>
    public static void BindTransferAmountInView()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = viewModel };
        viewModel.Draft.Amount = RentAmount;

        using (view.Bind(viewModel, x => x.Draft.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(viewModel.Draft.Amount);
        }

        // Output:
        // 1200.00
        // 85.50
    }

    /// <summary>Converts the score in each direction through the view-first <c>Bind</c> and delivers the writes on a named sequencer.</summary>
    public static void BindScoreInViewOnSequencer()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        VirtualClock uiThread = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, FormatScore, ParseScore, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.ScoreEntry.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.ScoreEntry.Text}'");

            viewModel.ScoreToRecord = RemarkedScore;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(view.ScoreEntry.Text);

            view.ScoreEntry.Text = TypedScoreText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // Waiting: ''
        // Written: '64.0'
        // 91.0
        // 82.5
    }

    /// <summary>Converts the score with a converter object in each direction through the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjects()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, toText, toScore))
        {
            Console.WriteLine(view.ScoreEntry.Text);

            view.ScoreEntry.Text = TypedScoreText;
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // 64
        // 82.5
    }

    /// <summary>Passes a hint to both converters of the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjectsAndHint()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, toText, toScore, OneDecimalPlace))
        {
            Console.WriteLine(view.ScoreEntry.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            Console.WriteLine(view.ScoreEntry.Text);
        }

        // Output:
        // 64.0
        // 91.0
    }

    /// <summary>Passes converter objects, a hint and a named sequencer to the view-first <c>Bind</c>.</summary>
    public static void BindScoreWithConverterObjectsOnSequencer()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        VirtualClock uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, toText, toScore, OneDecimalPlace, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.ScoreEntry.Text}'");

            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine($"Written: '{view.ScoreEntry.Text}'");

            view.ScoreEntry.Text = TypedScoreText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // Waiting: ''
        // Written: '64.0'
        // 82.5
    }

    /// <summary>
    /// Resolves the paths of the same converter-object binding while the app runs, with <c>BindUnsafe</c>. Use it when a
    /// path is not an inline lambda.
    /// </summary>
    public static void BindScoreUnsafeWithConverterObjects()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };
        VirtualClock uiThread = new();
        DecimalToStringTypeConverter toText = new();
        StringToDecimalTypeConverter toScore = new();

        using (view.BindUnsafe(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, toText, toScore, uiThread, OneDecimalPlace))
        {
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(view.ScoreEntry.Text);

            view.ScoreEntry.Text = TypedScoreText;
            uiThread.AdvanceBy(NextTurn);
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // 64.0
        // 82.5
    }

    /// <summary>
    /// Passes a null scheduler to <c>BindTwoWay</c>. Null means the same as naming no scheduler: each write happens on the
    /// thread that owns its object, so an object with no owning thread is written inline in both directions. The argument
    /// is named because a bare <c>null</c> also fits the string parameters of the overload that takes no scheduler.
    /// </summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTodoFilterWithNullScheduler()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();

        using (list.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text, scheduler: null))
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

    /// <summary>Converts the amount in each direction with functions and passes a null scheduler; both writes land before the next line runs.</summary>
    public static void BindTransferAmountWithConvertersAndNullScheduler()
    {
        TransferDraft draft = new() { Amount = RentAmount };
        TransferView view = new();

        using (draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, FormatAmount, ParseAmount, scheduler: null))
        {
            Console.WriteLine(view.AmountTextBox.Text);

            view.AmountTextBox.Text = TypedAmountText;
            Console.WriteLine(draft.Amount);

            draft.Amount = RentAmount;
            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 1200.00
        // 85.50
        // 1200.00
    }

    /// <summary>Converts the score in each direction through the view-first <c>Bind</c> and passes a null scheduler.</summary>
    public static void BindScoreInViewWithNullScheduler()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded()) { ScoreToRecord = FirstScore };
        GradebookView view = new() { ViewModel = viewModel };

        using (view.Bind(viewModel, x => x.ScoreToRecord, v => v.ScoreEntry.Text, FormatScore, ParseScore, scheduler: null))
        {
            Console.WriteLine(view.ScoreEntry.Text);

            viewModel.ScoreToRecord = RemarkedScore;
            Console.WriteLine(view.ScoreEntry.Text);

            view.ScoreEntry.Text = TypedScoreText;
            Console.WriteLine(viewModel.ScoreToRecord);
        }

        // Output:
        // 64.0
        // 91.0
        // 82.5
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
