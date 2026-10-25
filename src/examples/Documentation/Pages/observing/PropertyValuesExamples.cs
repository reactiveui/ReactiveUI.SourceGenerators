// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows <c>PropertyValues</c>, the value <c>WhenChanged</c> and <c>WhenChanging</c> deliver when you observe two to
/// sixteen properties without a value selector. It is a read-only record struct with one member per observed property
/// (<c>Property1</c> to <c>Property16</c>), a constructor and a deconstructor that take the values in the same order,
/// and value equality.
/// </summary>
public static class PropertyValuesExamples
{
    /// <summary>The identifier of the registration task.</summary>
    private const int RegistrationId = 7;

    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The label of an errand.</summary>
    private const string ErrandsTag = "errands";

    /// <summary>The label of a car task.</summary>
    private const string CarTag = "car";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The account name of the person the issue is assigned to.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The display name of the person the issue is assigned to.</summary>
    private const string PriyaName = "Priya Nair";

    /// <summary>The label of a defect.</summary>
    private const string BugLabel = "bug";

    /// <summary>The label of the checkout area.</summary>
    private const string CheckoutLabel = "checkout";

    /// <summary>The identifier of the account the money leaves.</summary>
    private const string EverydayId = "ACC-1001";

    /// <summary>The name of the account the money leaves.</summary>
    private const string EverydayName = "Everyday";

    /// <summary>The currency of the account the money leaves.</summary>
    private const string Currency = "AUD";

    /// <summary>The balance of the account the money leaves.</summary>
    private const decimal EverydayBalance = 4200.50M;

    /// <summary>How far below zero the account may go.</summary>
    private const decimal EverydayOverdraft = 500M;

    /// <summary>The identifier of the payee.</summary>
    private const int PayeeId = 3;

    /// <summary>The most the customer allows in one transfer to the payee.</summary>
    private const decimal PayeeLimit = 1000M;

    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>The reference the customer starts with.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The deadline of the registration task.</summary>
    private static readonly DateOnly _registrationDue = new(2026, 3, 20);

    /// <summary>The labels of the registration task.</summary>
    private static readonly IReadOnlyList<string> _registrationTags = [ErrandsTag, CarTag];

    /// <summary>The labels of the checkout issue.</summary>
    private static readonly IReadOnlyList<string> _checkoutLabels = [BugLabel, CheckoutLabel];

    /// <summary>The comments of a new issue.</summary>
    private static readonly IReadOnlyList<IssueComment> _noComments = [];

    /// <summary>When the checkout issue last changed.</summary>
    private static readonly DateTimeOffset _checkoutUpdatedAt = new(2026, 3, 2, 15, 5, 0, TimeSpan.Zero);

    /// <summary>The person the issue is assigned to.</summary>
    private static readonly User _priya = new() { Login = PriyaLogin, DisplayName = PriyaName };

    /// <summary>The person who reported the issue.</summary>
    private static readonly User _maria = new() { Login = "maria-santos", DisplayName = "Maria Santos" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>Handles each emission in a subscription that reads the members it needs.</summary>
    public static void SubscribeAndReadTodoValues()
    {
        TodoItem item = CreateRegistrationTask();
        List<string> lines = [];

        using (item
            .WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority)
            .Subscribe(values => lines.Add($"{values.Property1} ({values.Property3}, done: {values.Property2})")))
        {
            item.Priority = TodoPriority.High;
        }

        Console.WriteLine(string.Join(", ", lines));

        // Output:
        // Renew car registration (Normal, done: False), Renew car registration (High, done: False)
    }

    /// <summary>Compares emissions of two items; equal members make equal values.</summary>
    /// <returns>A task that completes when the values are compared.</returns>
    public static async Task CompareTodoValues()
    {
        TodoItem first = CreateRegistrationTask();
        TodoItem second = CreateRegistrationTask();

        PropertyValues<string, bool> left = await first.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();
        PropertyValues<string, bool> right = await second.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();
        object boxed = (object)right;

        Console.WriteLine(left == right);
        Console.WriteLine(left != right);
        Console.WriteLine(left.Equals(right));
        Console.WriteLine(left.Equals(boxed));
        Console.WriteLine(left.GetHashCode() == right.GetHashCode());

        second.IsDone = true;
        PropertyValues<string, bool> finished = await second.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();

        Console.WriteLine(left == finished);

        // Output:
        // True
        // False
        // True
        // True
        // True
        // False
    }

    /// <summary>Copies an emission with one member replaced; the original keeps its values.</summary>
    /// <returns>A task that completes when the values are printed.</returns>
    public static async Task ReplaceOneTodoValue()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, bool> original = await item.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();
        PropertyValues<string, bool> finished = original with { Property2 = true };

        Console.WriteLine(original.Property2);
        Console.WriteLine(finished.Property2);

        // Output:
        // False
        // True
    }

    /// <summary>Prints an emission; the text lists each member by name.</summary>
    /// <returns>A task that completes when the values are printed.</returns>
    public static async Task PrintTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, bool> values = await item.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();

        Console.WriteLine($"Emission: {values}.");

        // Output:
        // Emission: PropertyValues { Property1 = Renew car registration, Property2 = False }.
    }

    /// <summary>Reads the title and done flag of a to-do item from a two-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadTwoTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, bool> values = await item.WhenChanged(x => x.Title, x => x.IsDone).FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);

        var (title, isDone) = values;

        Console.WriteLine(values.Equals(new(title, isDone)));

        // Output:
        // Renew car registration
        // False
        // True
    }

    /// <summary>Reads the title, done flag and priority of a to-do item from a three-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadThreeTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, bool, TodoPriority> values = await item.WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority).FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);

        var (title, isDone, priority) = values;

        Console.WriteLine(values.Equals(new(title, isDone, priority)));

        // Output:
        // Renew car registration
        // False
        // Normal
        // True
    }

    /// <summary>Reads the title, notes, done flag and deadline of a to-do item from a four-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadFourTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, string, bool, DateOnly?> values = await item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);

        var (title, notes, isDone, dueDate) = values;

        Console.WriteLine(values.Equals(new(title, notes, isDone, dueDate)));

        // Output:
        // Renew car registration
        // Bring the insurance certificate
        // False
        // 03/20/2026
        // True
    }

    /// <summary>Reads the title, notes, done flag, deadline and priority of a to-do item from a five-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadFiveTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, string, bool, DateOnly?, TodoPriority> values = await item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);

        var (title, notes, isDone, dueDate, priority) = values;

        Console.WriteLine(values.Equals(new(title, notes, isDone, dueDate, priority)));

        // Output:
        // Renew car registration
        // Bring the insurance certificate
        // False
        // 03/20/2026
        // Normal
        // True
    }

    /// <summary>Reads every editable property of a to-do item from a six-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadSixTodoValues()
    {
        TodoItem item = CreateRegistrationTask();

        PropertyValues<string, string, bool, DateOnly?, TodoPriority, IReadOnlyList<string>> values = await item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(string.Join(", ", values.Property6));

        var (title, notes, isDone, dueDate, priority, tags) = values;

        Console.WriteLine(values.Equals(new(title, notes, isDone, dueDate, priority, tags)));

        // Output:
        // Renew car registration
        // Bring the insurance certificate
        // False
        // 03/20/2026
        // Normal
        // errands, car
        // True
    }

    /// <summary>Reads seven properties of an issue from a seven-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadSevenIssueValues()
    {
        Issue issue = CreateCheckoutIssue();

        PropertyValues<int, string, IssueState, User, User, IReadOnlyList<string>, DateTimeOffset> values = await issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4.Login);
        Console.WriteLine(values.Property5?.Login);
        Console.WriteLine(string.Join(", ", values.Property6));
        Console.WriteLine(values.Property7);

        var (number, title, state, author, assignee, labels, updatedAt) = values;

        Console.WriteLine(values.Equals(new(number, title, state, author, assignee, labels, updatedAt)));

        // Output:
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // maria-santos
        // priya-nair
        // bug, checkout
        // 03/02/2026 15:05:00 +00:00
        // True
    }

    /// <summary>Reads eight properties of an issue from an eight-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadEightIssueValues()
    {
        Issue issue = CreateCheckoutIssue();

        PropertyValues<int, string, IssueState, User, User, IReadOnlyList<string>, DateTimeOffset, IReadOnlyList<IssueComment>> values = await issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                x => x.Comments)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4.Login);
        Console.WriteLine(values.Property5?.Login);
        Console.WriteLine(string.Join(", ", values.Property6));
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8.Count);

        var (number, title, state, author, assignee, labels, updatedAt, comments) = values;

        Console.WriteLine(values.Equals(new(number, title, state, author, assignee, labels, updatedAt, comments)));

        // Output:
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // maria-santos
        // priya-nair
        // bug, checkout
        // 03/02/2026 15:05:00 +00:00
        // 0
        // True
    }

    /// <summary>Reads a transfer draft and the fields of its source account from a nine-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadNineTransferValues()
    {
        TransferDraft draft = CreateTransferDraft();

        PropertyValues<decimal, string, string, string, AccountKind, string, decimal, decimal?, decimal> values = await draft
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(values.Property9);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance) = values;

        Console.WriteLine(values.Equals(new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance)));

        // Output:
        // 250
        // Rent March
        // ACC-1001
        // Everyday
        // Everyday
        // AUD
        // 4200.50
        // 500
        // 4700.50
        // True
    }

    /// <summary>Reads the payee of a transfer draft besides its source account fields from a ten-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadTenTransferValues()
    {
        TransferDraft draft = CreateTransferDraft();

        PropertyValues<decimal, string, string, string, AccountKind, string, decimal, decimal?, decimal, Payee> values = await draft
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(values.Property9);
        Console.WriteLine(values.Property10?.Name);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee) = values;

        Console.WriteLine(values.Equals(new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee)));

        // Output:
        // 250
        // Rent March
        // ACC-1001
        // Everyday
        // Everyday
        // AUD
        // 4200.50
        // 500
        // 4700.50
        // Harbour Realty
        // True
    }

    /// <summary>Reads the source account itself besides its fields from an eleven-value emission.</summary>
    /// <returns>A task that completes when the values are read.</returns>
    public static async Task ReadElevenTransferValues()
    {
        TransferDraft draft = CreateTransferDraft();

        PropertyValues<decimal, string, string, string, AccountKind, string, decimal, decimal?, decimal, Payee, Account> values = await draft
            .WhenChanged(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Id,
                x => x.Source!.Name,
                x => x.Source!.Kind,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(values.Property9);
        Console.WriteLine(values.Property10?.Name);
        Console.WriteLine(values.Property11?.Name);

        var (amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee, source) = values;

        Console.WriteLine(values.Equals(new(amount, reference, sourceId, sourceName, sourceKind, currency, balance, overdraftLimit, availableBalance, payee, source)));

        // Output:
        // 250
        // Rent March
        // ACC-1001
        // Everyday
        // Everyday
        // AUD
        // 4200.50
        // 500
        // 4700.50
        // Harbour Realty
        // Everyday
        // True
    }

    /// <summary>Reads the session, selected issue and rate limit of the issue board from a twelve-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadTwelveIssueBoardValues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        PropertyValues<bool, string, string, int, string, IssueState, string, string, IReadOnlyList<string>, DateTimeOffset, string, int> values = await board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(string.Join(", ", values.Property9));
        Console.WriteLine(values.Property10);
        Console.WriteLine(values.Property11.Length);
        Console.WriteLine(values.Property12);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining) = values;

        Console.WriteLine(values.Equals(new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining)));

        // Output:
        // True
        // priya-nair
        // Priya Nair
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // priya-nair
        // Priya Nair
        // bug, checkout
        // 03/03/2026 09:00:00 +00:00
        // 0
        // 57
        // True
    }

    /// <summary>Reads the last error message of the issue board from a thirteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadThirteenIssueBoardValues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        PropertyValues<bool, string, string, int, string, IssueState, string, string, IReadOnlyList<string>, DateTimeOffset, string, int, string> values = await board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(string.Join(", ", values.Property9));
        Console.WriteLine(values.Property10);
        Console.WriteLine(values.Property11.Length);
        Console.WriteLine(values.Property12);
        Console.WriteLine(values.Property13.Length);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error) = values;

        Console.WriteLine(values.Equals(new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error)));

        // Output:
        // True
        // priya-nair
        // Priya Nair
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // priya-nair
        // Priya Nair
        // bug, checkout
        // 03/03/2026 09:00:00 +00:00
        // 0
        // 57
        // 0
        // True
    }

    /// <summary>Reads the access token of the issue board from a fourteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadFourteenIssueBoardValues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        PropertyValues<bool, string, string, int, string, IssueState, string, string, IReadOnlyList<string>, DateTimeOffset, string, int, string, string> values = await board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(string.Join(", ", values.Property9));
        Console.WriteLine(values.Property10);
        Console.WriteLine(values.Property11.Length);
        Console.WriteLine(values.Property12);
        Console.WriteLine(values.Property13.Length);
        Console.WriteLine(values.Property14);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token) = values;

        Console.WriteLine(values.Equals(new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token)));

        // Output:
        // True
        // priya-nair
        // Priya Nair
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // priya-nair
        // Priya Nair
        // bug, checkout
        // 03/03/2026 09:00:00 +00:00
        // 0
        // 57
        // 0
        // ghp_priya_2f9c1d
        // True
    }

    /// <summary>Reads the open issues of the issue board from a fifteen-value emission.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadFifteenIssueBoardValues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        PropertyValues<bool, string, string, int, string, IssueState, string, string, IReadOnlyList<string>, DateTimeOffset, string, int, string, string, IReadOnlyList<Issue>> values = await board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(string.Join(", ", values.Property9));
        Console.WriteLine(values.Property10);
        Console.WriteLine(values.Property11.Length);
        Console.WriteLine(values.Property12);
        Console.WriteLine(values.Property13.Length);
        Console.WriteLine(values.Property14);
        Console.WriteLine(values.Property15.Count);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues) = values;

        Console.WriteLine(values.Equals(new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues)));

        // Output:
        // True
        // priya-nair
        // Priya Nair
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // priya-nair
        // Priya Nair
        // bug, checkout
        // 03/03/2026 09:00:00 +00:00
        // 0
        // 57
        // 0
        // ghp_priya_2f9c1d
        // 2
        // True
    }

    /// <summary>Reads the repositories of the issue board from a sixteen-value emission, the most one emission holds.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ReadSixteenIssueBoardValues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        PropertyValues<
            bool,
            string,
            string,
            int,
            string,
            IssueState,
            string,
            string,
            IReadOnlyList<string>,
            DateTimeOffset,
            string,
            int,
            string,
            string,
            IReadOnlyList<Issue>,
            IReadOnlyList<Repository>> values = await board
            .WhenChanged(
                x => x.IsSignedIn,
                x => x.CurrentUser!.Login,
                x => x.CurrentUser!.DisplayName,
                x => x.SelectedIssue!.Number,
                x => x.SelectedIssue!.Title,
                x => x.SelectedIssue!.State,
                x => x.SelectedIssue!.Assignee!.Login,
                x => x.SelectedIssue!.Assignee!.DisplayName,
                x => x.SelectedIssue!.Labels,
                x => x.SelectedIssue!.UpdatedAt,
                x => x.NewCommentText,
                x => x.RateLimitRemaining,
                x => x.ErrorMessage,
                x => x.Token,
                x => x.Issues,
                x => x.Repositories)
            .FirstAsync();

        Console.WriteLine(values.Property1);
        Console.WriteLine(values.Property2);
        Console.WriteLine(values.Property3);
        Console.WriteLine(values.Property4);
        Console.WriteLine(values.Property5);
        Console.WriteLine(values.Property6);
        Console.WriteLine(values.Property7);
        Console.WriteLine(values.Property8);
        Console.WriteLine(string.Join(", ", values.Property9));
        Console.WriteLine(values.Property10);
        Console.WriteLine(values.Property11.Length);
        Console.WriteLine(values.Property12);
        Console.WriteLine(values.Property13.Length);
        Console.WriteLine(values.Property14);
        Console.WriteLine(values.Property15.Count);
        Console.WriteLine(values.Property16.Count);

        var (signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues, repos) = values;

        Console.WriteLine(values.Equals(new(signedIn, login, name, number, title, state, assignee, assigneeName, labels, updatedAt, comment, remaining, error, token, issues, repos)));

        // Output:
        // True
        // priya-nair
        // Priya Nair
        // 101
        // Checkout button unresponsive on Safari
        // Open
        // priya-nair
        // Priya Nair
        // bug, checkout
        // 03/03/2026 09:00:00 +00:00
        // 0
        // 57
        // 0
        // ghp_priya_2f9c1d
        // 2
        // 2
        // True
    }

    /// <summary>Creates the to-do item the examples observe.</summary>
    /// <returns>An open, unfinished item.</returns>
    private static TodoItem CreateRegistrationTask() => new()
    {
        Id = RegistrationId,
        Title = RegistrationTitle,
        Notes = RegistrationNotes,
        DueDate = _registrationDue,
        Priority = TodoPriority.Normal,
        Tags = _registrationTags,
    };

    /// <summary>Creates the issue the examples observe.</summary>
    /// <returns>An open issue reported by Maria and assigned to Priya.</returns>
    private static Issue CreateCheckoutIssue() => new()
    {
        Number = CheckoutNumber,
        Title = CheckoutTitle,
        State = IssueState.Open,
        Author = _maria,
        Assignee = _priya,
        Labels = _checkoutLabels,
        Comments = _noComments,
        UpdatedAt = _checkoutUpdatedAt,
    };

    /// <summary>Creates the transfer the examples observe.</summary>
    /// <returns>A draft that sends the rent from the everyday account to the landlord.</returns>
    private static TransferDraft CreateTransferDraft() =>
        new() { Source = CreateEverydayAccount(), Payee = _landlord, Amount = RentAmount, Reference = RentReference };

    /// <summary>Creates the account the transfer draws on.</summary>
    /// <returns>An everyday account with an overdraft.</returns>
    private static Account CreateEverydayAccount() => new()
    {
        Id = EverydayId,
        Name = EverydayName,
        Kind = AccountKind.Everyday,
        Currency = Currency,
        Balance = EverydayBalance,
        OverdraftLimit = EverydayOverdraft,
    };

    /// <summary>Signs in to a seeded server, loads the open issues of the webshop repository and selects the checkout issue.</summary>
    /// <returns>A task that returns the board once the issue is selected.</returns>
    private static async Task<IssueBoardViewModel> CreateBoardWithSelectedIssueAsync()
    {
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueBoardViewModel board = new(server) { Token = InMemoryGitHubServer.PriyaToken };

        await board.SignInAsync();

        board.SelectedRepository = board.Repositories[0];
        await board.LoadIssuesAsync();

        board.SelectedIssue = board.Issues[1];
        return board;
    }
}
