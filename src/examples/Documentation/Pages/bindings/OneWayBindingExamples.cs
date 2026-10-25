// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows how <c>BindOneWay</c> and <c>OneWayBind</c> copy a view model property onto a view property.</summary>
public static class OneWayBindingExamples
{
    /// <summary>The size of the file the upload example sends: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>The number that turns an upload percentage into the fraction a progress bar shows.</summary>
    private const double PercentPerBar = 100D;

    /// <summary>The position of the checkout issue in the list of open issues, which is newest first.</summary>
    private const int CheckoutIssueIndex = 1;

    /// <summary>The account name of the person the checkout issue is reassigned to.</summary>
    private const string ReassignedLogin = "tomas-berg";

    /// <summary>The balance the everyday account holds after a deposit.</summary>
    private const decimal DepositedBalance = 1200.50M;

    /// <summary>The number format that groups digits and shows no decimals.</summary>
    private const string GroupedFormat = "N0";

    /// <summary>The number format that shows one decimal place.</summary>
    private const string OneDecimalFormat = "N1";

    /// <summary>The hint that asks the priority converter for its dark palette.</summary>
    private const string DarkThemeHint = "dark";

    /// <summary>The text of the filter the customer types.</summary>
    private const string CarFilter = "car";

    /// <summary>The text shown for an open issue.</summary>
    private const string OpenText = "Open";

    /// <summary>The text shown for a closed issue.</summary>
    private const string ClosedText = "Closed";

    /// <summary>The title of the to-do item whose priority the badge examples show.</summary>
    private const string ItemTitle = "Renew car registration";

    /// <summary>The text shown when no issue is selected.</summary>
    private const string NothingSelectedText = "nothing selected";

    /// <summary>The time a sequencer stands still before it runs the writes that wait for it.</summary>
    private static readonly TimeSpan NextTurn = TimeSpan.FromMilliseconds(1);

    /// <summary>Registers the core services the <c>Unsafe</c> bindings look up while the app runs.</summary>
    public static void BuildApplication()
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
    }

    /// <summary>Copies one property to another of the same type with <c>BindOneWay</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindSelectedIssueTitleToLabel()
    {
        IssueBoardViewModel board = await OpenIssueBoardAsync();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text))
        {
            // The path passes through SelectedIssue, which is null until the user picks an issue.
            Console.WriteLine(view.IssueTitleLabel.Text ?? NothingSelectedText);

            board.SelectedIssue = board.Issues[CheckoutIssueIndex];
            Console.WriteLine(view.IssueTitleLabel.Text);

            board.SelectedIssue = board.Issues[0];
            Console.WriteLine(view.IssueTitleLabel.Text);
        }

        // Output:
        // nothing selected
        // Checkout button unresponsive on Safari
        // Add gift-card support
    }

    /// <summary>Follows a path through an optional property: an issue without an assignee has no login to show.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindSelectedIssueAssigneeToLabel()
    {
        IssueBoardViewModel board = await OpenIssueBoardAsync();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Assignee!.Login, v => v.AssigneeLabel.Text))
        {
            board.SelectedIssue = board.Issues[CheckoutIssueIndex];
            Console.WriteLine(view.AssigneeLabel.Text);

            board.SelectedIssue.Assignee = new User { Login = ReassignedLogin, DisplayName = "Tomas Berg" };
            Console.WriteLine(view.AssigneeLabel.Text);

            board.SelectedIssue = board.Issues[0];
            Console.WriteLine(view.AssigneeLabel.Text ?? NothingSelectedText);
        }

        // Output:
        // priya-nair
        // tomas-berg
        // nothing selected
    }

    /// <summary>Turns an enum into text with a converter function, the second <c>BindOneWay</c> overload.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindSelectedIssueStateToLabel()
    {
        IssueBoardViewModel board = await OpenIssueBoardAsync();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.State, v => v.StateLabel.Text, static state => state == IssueState.Open ? OpenText : ClosedText))
        {
            board.SelectedIssue = board.Issues[CheckoutIssueIndex];
            Console.WriteLine(view.StateLabel.Text);

            board.SelectedIssue.State = IssueState.Closed;
            Console.WriteLine(view.StateLabel.Text);
        }

        // Output:
        // Open
        // Closed
    }

    /// <summary>Shows how many to-do items are left, and stops updating once the binding is disposed.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindRemainingCountToLabel()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            await list.CompleteAsync();
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Disposing the binding disconnects it: the label keeps the last text it was given.
        list.SelectedItem = list.Items[CheckoutIssueIndex];
        await list.CompleteAsync();
        Console.WriteLine(view.RemainingLabel.Text);

        // Output:
        // 3
        // 2
        // 2
    }

    /// <summary>Formats a balance as currency with a converter function.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindBalanceToLabelWithCurrencyFormat()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Balance, v => v.BalanceLabel.Text, FormatBalance))
        {
            accounts.SelectedAccount = accounts.Accounts[0];
            Console.WriteLine(view.BalanceLabel.Text);

            accounts.SelectedAccount.Balance = DepositedBalance;
            Console.WriteLine(view.BalanceLabel.Text);

            accounts.SelectedAccount = accounts.Accounts[1];
            Console.WriteLine(view.BalanceLabel.Text);
        }

        // Output:
        // A$2,450.75
        // A$1,200.50
        // A$15,230.00
    }

    /// <summary>Copies the filter text of the to-do list into the filter box with the view-first <c>OneWayBind</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindFilterTextToBox()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = list };

        using (view.OneWayBind(list, x => x.FilterText, v => v.FilterTextBox.Text))
        {
            list.FilterText = CarFilter;
            Console.WriteLine(view.FilterTextBox.Text);
        }

        // Output:
        // car
    }

    /// <summary>Binds into a named control, which markup declares as a field, with the view-first <c>OneWayBind</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindIntoNamedControl()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoHeadingView view = new() { ViewModel = list };

        // TitleLabel is a field, like every x:Name control; the path runs through it.
        using (view.OneWayBind(list, x => x.SelectedItem!.Title, v => v.TitleLabel.Text))
        {
            list.SelectedItem = list.Items[0];
            Console.WriteLine(view.TitleLabel.Text);

            list.SelectedItem = list.Items[1];
            Console.WriteLine(view.TitleLabel.Text);
        }

        // Output:
        // Renew car registration
        // Book dentist appointment
    }

    /// <summary>Keeps the label as it is while the path to the bound property passes through a null.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindKeepsViewWhilePathIsBroken()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoHeadingView view = new() { ViewModel = list };

        using (view.OneWayBind(list, x => x.SelectedItem!.Title, v => v.TitleLabel.Text))
        {
            // Nothing is selected yet, so there is no title to write and the label is left alone.
            Console.WriteLine(view.TitleLabel.Text ?? NothingSelectedText);

            list.SelectedItem = list.Items[0];
            Console.WriteLine(view.TitleLabel.Text);

            // Clearing the selection breaks the path again; the label keeps the last title it was given.
            list.SelectedItem = null;
            Console.WriteLine(view.TitleLabel.Text);
        }

        // Output:
        // nothing selected
        // Renew car registration
        // Renew car registration
    }

    /// <summary>Fills a progress bar from the upload percentage with a selector that turns a percentage into the fraction the bar shows.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindUploadPercentToProgressBar()
    {
        StorageBrowserViewModel browser = await OpenBucketAsync();
        StorageBrowserView view = new() { ViewModel = browser };

        // A view-first binding reads its view model from the view's ViewModel property.
        using (view.OneWayBind(browser, x => x.UploadPercent, v => v.UploadProgressBar.Progress, static percent => percent / PercentPerBar))
        {
            Console.WriteLine(view.UploadProgressBar.Progress);

            await browser.UploadAsync(new("launch-video.mp4", UploadSizeBytes, "video/mp4"));

            Console.WriteLine(view.UploadProgressBar.Progress);
        }

        // Output:
        // 0
        // 1
    }

    /// <summary>Formats a balance for a label with a selector, the second <c>OneWayBind</c> overload.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindAvailableBalanceWithSelector()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new() { ViewModel = accounts };

        using (view.OneWayBind(accounts, x => x.SelectedAccount!.AvailableBalance, v => v.AvailableBalanceLabel.Text, FormatBalance))
        {
            accounts.SelectedAccount = accounts.Accounts[0];

            // The everyday account has a 500 overdraft on top of its balance.
            Console.WriteLine(view.AvailableBalanceLabel.Text);
        }

        // Output:
        // A$2,950.75
    }

    /// <summary>Names the sequencer that delivers the write; the write waits until the sequencer runs.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayOnSequencer()
    {
        IssueBoardViewModel board = await OpenIssueBoardAsync();
        IssueBoardView view = new();
        VirtualClock uiThread = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text, uiThread))
        {
            board.SelectedIssue = board.Issues[CheckoutIssueIndex];

            Console.WriteLine($"Waiting: '{view.IssueTitleLabel.Text}'");

            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.IssueTitleLabel.Text}'");
        }

        // Output:
        // Waiting: ''
        // Written: 'Checkout button unresponsive on Safari'
    }

    /// <summary>Converts with a function and delivers on a named sequencer.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayConvertedOnSequencer()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();
        VirtualClock uiThread = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), uiThread))
        {
            Console.WriteLine($"Waiting: '{view.RemainingLabel.Text}'");

            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.RemainingLabel.Text}'");
        }

        // Output:
        // Waiting: ''
        // Written: '3'
    }

    /// <summary>Converts with a converter object. Without a hint <see cref="CurrencyTextConverter"/> shows two decimal places.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayWithConverterObject()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter()))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // $17,680.75
    }

    /// <summary>Passes a hint to the converter: the hint is the number format the total is shown in.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayWithConverterObjectAndHint()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), GroupedFormat))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // $17,681
    }

    /// <summary>Passes a converter object, a hint and a named sequencer.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayWithConverterObjectOnSequencer()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();
        VirtualClock uiThread = new();

        using (accounts.BindOneWay(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), GroupedFormat, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.TotalBalanceLabel.Text}'");

            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.TotalBalanceLabel.Text}'");
        }

        // Output:
        // Waiting: ''
        // Written: '$17,681'
    }

    /// <summary>Colours a badge from the priority of a to-do item with a converter object in the view-first <c>OneWayBind</c>.</summary>
    public static void OneWayBindPriorityToBadgeColour()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Color, new PriorityColourConverter()))
        {
            Console.WriteLine(view.PriorityBadge.Color.ToArgbHex());

            item.Priority = TodoPriority.Low;
            Console.WriteLine(view.PriorityBadge.Color.ToArgbHex());
        }

        // Output:
        // #C62828
        // #2E7D32
    }

    /// <summary>Passes the dark-theme hint to the priority converter.</summary>
    public static void OneWayBindPriorityToBadgeColourWithHint()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Color, new PriorityColourConverter(), DarkThemeHint))
        {
            Console.WriteLine(view.PriorityBadge.Color.ToArgbHex());

            item.Priority = TodoPriority.Normal;
            Console.WriteLine(view.PriorityBadge.Color.ToArgbHex());
        }

        // Output:
        // #EF5350
        // #BDBDBD
    }

    /// <summary>Passes the converter object, the hint and a named sequencer to <c>OneWayBind</c>.</summary>
    public static void OneWayBindPriorityToBadgeColourOnSequencer()
    {
        TodoItem item = new() { Title = ItemTitle, Priority = TodoPriority.High };
        TodoItemBadgeView view = new() { ViewModel = item };
        VirtualClock uiThread = new();

        using (view.OneWayBind(item, x => x.Priority, v => v.PriorityBadge.Color, new PriorityColourConverter(), DarkThemeHint, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.PriorityBadge.Color?.ToArgbHex()}'");

            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.PriorityBadge.Color?.ToArgbHex()}'");
        }

        // Output:
        // Waiting: ''
        // Written: '#EF5350'
    }

    /// <summary>Formats with a selector and delivers the write on a named sequencer.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindWithSelectorOnSequencer()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new() { ViewModel = accounts };
        VirtualClock uiThread = new();

        using (view.OneWayBind(accounts, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, FormatBalance, uiThread))
        {
            Console.WriteLine($"Waiting: '{view.TotalBalanceLabel.Text}'");

            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.TotalBalanceLabel.Text}'");
        }

        // Output:
        // Waiting: ''
        // Written: 'A$17,680.75'
    }

    /// <summary>Resolves the same converter-object binding while the app runs, with <c>BindOneWayUnsafe</c>. Use it when a path is not an inline lambda.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayUnsafeWithConverterObject()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();
        VirtualClock uiThread = new();

        using (accounts.BindOneWayUnsafe(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, GroupedFormat))
        {
            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.TotalBalanceLabel.Text}'");
        }

        // Without a hint the converter falls back to two decimal places.
        using (accounts.BindOneWayUnsafe(view, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, null))
        {
            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.TotalBalanceLabel.Text}'");
        }

        // Output:
        // Written: '$17,681'
        // Written: '$17,680.75'
    }

    /// <summary>Resolves the same view-first converter-object binding while the app runs, with <c>OneWayBindUnsafe</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindUnsafeWithConverterObject()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new() { ViewModel = accounts };
        VirtualClock uiThread = new();

        using (view.OneWayBindUnsafe(accounts, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter(), uiThread, OneDecimalFormat))
        {
            uiThread.AdvanceBy(NextTurn);

            Console.WriteLine($"Written: '{view.TotalBalanceLabel.Text}'");
        }

        // Output:
        // Written: '$17,680.8'
    }

    /// <summary>
    /// Passes a null scheduler to <c>BindOneWay</c>. Null means the same as naming no scheduler: the write happens on the
    /// thread that owns the target, so a target with no owning thread is written inline. The argument is named because a
    /// bare <c>null</c> also fits the string parameters of the overload that takes no scheduler.
    /// </summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayWithNullScheduler()
    {
        IssueBoardViewModel board = await OpenIssueBoardAsync();
        IssueBoardView view = new();

        using (board.BindOneWay(view, x => x.SelectedIssue!.Title, v => v.IssueTitleLabel.Text, scheduler: null))
        {
            board.SelectedIssue = board.Issues[CheckoutIssueIndex];
            Console.WriteLine(view.IssueTitleLabel.Text);

            board.SelectedIssue = board.Issues[0];
            Console.WriteLine(view.IssueTitleLabel.Text);
        }

        // Output:
        // Checkout button unresponsive on Safari
        // Add gift-card support
    }

    /// <summary>Converts with a function and passes a null scheduler; the label holds the text as soon as the binding is made.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindOneWayConvertedWithNullScheduler()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();

        using (list.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), scheduler: null))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            await list.CompleteAsync();
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
        // 2
    }

    /// <summary>Formats with a selector and passes a null scheduler to the view-first <c>OneWayBind</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task OneWayBindWithSelectorAndNullScheduler()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new() { ViewModel = accounts };

        using (view.OneWayBind(accounts, x => x.SelectedAccount!.Balance, v => v.BalanceLabel.Text, FormatBalance, scheduler: null))
        {
            accounts.SelectedAccount = accounts.Accounts[0];
            Console.WriteLine(view.BalanceLabel.Text);

            accounts.SelectedAccount.Balance = DepositedBalance;
            Console.WriteLine(view.BalanceLabel.Text);
        }

        // Output:
        // A$2,450.75
        // A$1,200.50
    }

    /// <summary>Formats an amount for a label.</summary>
    /// <param name="amount">The amount of money.</param>
    /// <returns>The amount with a currency sign and two decimal places.</returns>
    private static string FormatBalance(decimal amount) => FormattableString.Invariant($"A${amount:N2}");

    /// <summary>Signs in as Priya, opens the web shop repository and loads its open issues.</summary>
    /// <returns>A task that completes with the board, which lists two open issues, newest first.</returns>
    private static async Task<IssueBoardViewModel> OpenIssueBoardAsync()
    {
        IssueBoardViewModel board = new(InMemoryGitHubServer.CreateSeeded()) { Token = InMemoryGitHubServer.PriyaToken };

        await board.SignInAsync();
        board.SelectedRepository = board.Repositories[0];
        await board.LoadIssuesAsync();
        return board;
    }

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>A task that completes with the list, which holds three unfinished items and one finished item.</returns>
    private static async Task<TodoListViewModel> OpenTodoListAsync()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());

        await list.LoadAsync();
        return list;
    }

    /// <summary>Loads the accounts of the seeded bank.</summary>
    /// <returns>A task that completes with the accounts screen, which lists an everyday and a savings account and has nothing selected.</returns>
    private static async Task<AccountsViewModel> OpenAccountsAsync()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend());

        await accounts.LoadAccountsAsync();
        return accounts;
    }

    /// <summary>Lists the buckets of the storage service and picks the first.</summary>
    /// <returns>A task that completes with the browser, which has a bucket selected.</returns>
    private static async Task<StorageBrowserViewModel> OpenBucketAsync()
    {
        StorageBrowserViewModel browser = new(InMemoryObjectStorage.CreateSeeded());

        await browser.LoadBucketsAsync();
        browser.SelectedBucket = browser.Buckets[0];
        return browser;
    }
}
