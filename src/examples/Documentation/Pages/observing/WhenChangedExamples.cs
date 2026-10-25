// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows every <c>WhenChanged</c> overload, from one observed property to sixteen, each with and without a value
/// selector. <c>WhenChanged</c> delivers the current values when you subscribe and again after each change.
/// </summary>
public static class WhenChangedExamples
{
    /// <summary>The identifier of the registration task.</summary>
    private const int RegistrationId = 7;

    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the registration task.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The label of an errand.</summary>
    private const string ErrandsTag = "errands";

    /// <summary>The label of a car task.</summary>
    private const string CarTag = "car";

    /// <summary>The label of an urgent task.</summary>
    private const string UrgentTag = "urgent";

    /// <summary>The prefix of a finished checklist line.</summary>
    private const string DoneMark = "[x] ";

    /// <summary>The prefix of an open checklist line.</summary>
    private const string OpenMark = "[ ] ";

    /// <summary>The format of a deadline in a headline.</summary>
    private const string IsoDay = "yyyy-MM-dd";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

    /// <summary>The title the checkout issue is renamed to.</summary>
    private const string RenamedIssueTitle = "Checkout button unresponsive on Safari 17";

    /// <summary>The account name of the person the issue is assigned to.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The display name of the person the issue is assigned to.</summary>
    private const string PriyaName = "Priya Nair";

    /// <summary>The account name of the person the issue is reassigned to.</summary>
    private const string TomasLogin = "tomas-berg";

    /// <summary>The text that stands for nobody working on an issue.</summary>
    private const string Unassigned = "unassigned";

    /// <summary>The label of a defect.</summary>
    private const string BugLabel = "bug";

    /// <summary>The label of the checkout area.</summary>
    private const string CheckoutLabel = "checkout";

    /// <summary>The comment the user is typing on the selected issue.</summary>
    private const string CommentDraft = "Fixed in the next release.";

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

    /// <summary>An amount above the payee's limit and below the available balance.</summary>
    private const decimal AbovePayeeLimit = 1500M;

    /// <summary>An amount above the available balance.</summary>
    private const decimal AboveAvailable = 9000M;

    /// <summary>The reference the customer starts with.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer changes to.</summary>
    private const string RentReferenceRevised = "Rent March, paid early";

    /// <summary>The deadline of the registration task.</summary>
    private static readonly DateOnly _registrationDue = new(2026, 3, 20);

    /// <summary>The deadline the registration task moves to.</summary>
    private static readonly DateOnly _movedDue = new(2026, 3, 27);

    /// <summary>The labels of the registration task.</summary>
    private static readonly IReadOnlyList<string> _registrationTags = [ErrandsTag, CarTag];

    /// <summary>The labels of the registration task once it is urgent.</summary>
    private static readonly IReadOnlyList<string> _urgentTags = [ErrandsTag, CarTag, UrgentTag];

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

    /// <summary>The person the issue is reassigned to.</summary>
    private static readonly User _tomas = new() { Login = TomasLogin, DisplayName = "Tomas Berg" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>Observes the title of a to-do item; a single property delivers the value itself.</summary>
    public static void ObserveTodoTitle()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine);

        item.Title = RenamedTitle;

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Observes the title and the done flag of a to-do item together.</summary>
    public static void ObserveTodoTitleAndStatus()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenChanged(x => x.Title, x => x.IsDone).Subscribe(static values => Console.WriteLine(values.Property2));

        item.IsDone = true;

        // Output:
        // False
        // True
    }

    /// <summary>Turns the title and the done flag into one checklist line with a value selector.</summary>
    public static void ProjectTodoChecklistLine()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.IsDone, static (title, isDone) => (isDone ? DoneMark : OpenMark) + title)
            .Subscribe(Console.WriteLine);

        item.IsDone = true;

        // Output:
        // [ ] Renew car registration
        // [x] Renew car registration
    }

    /// <summary>Observes the title, the done flag and the priority of a to-do item.</summary>
    public static void ObserveTodoTitleStatusAndPriority()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority)
            .Subscribe(static values => Console.WriteLine(values.Property3));

        item.Priority = TodoPriority.High;

        // Output:
        // Normal
        // High
    }

    /// <summary>Works out how urgent a to-do item is from three properties; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone ? TodoPriority.Low : priority)
            .Subscribe(static priority => Console.WriteLine(priority));

        item.Priority = TodoPriority.High;
        item.IsDone = true;

        // Output:
        // Normal
        // High
        // Low
    }

    /// <summary>Observes the title, notes, done flag and deadline of a to-do item.</summary>
    public static void ObserveTodoDeadline()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate)
            .Subscribe(static values => Console.WriteLine(values.Property4));

        item.DueDate = _movedDue;

        // Output:
        // 03/20/2026
        // 03/27/2026
    }

    /// <summary>Builds a headline with the deadline from four properties.</summary>
    public static void ProjectTodoHeadline()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) => isDone || due is null ? title : $"{title} due {due.Value.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Subscribe(Console.WriteLine);

        item.DueDate = _movedDue;

        // Output:
        // Renew car registration due 2026-03-20
        // Renew car registration due 2026-03-27
    }

    /// <summary>Observes the title, notes, done flag, deadline and priority of a to-do item.</summary>
    public static void ObserveTodoWithPriority()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority)
            .Subscribe(static values => Console.WriteLine(values.Property5));

        item.Priority = TodoPriority.High;

        // Output:
        // Normal
        // High
    }

    /// <summary>Decides from five properties whether a to-do item needs attention now.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone && due is not null && priority == TodoPriority.High)
            .Subscribe(Console.WriteLine);

        item.Priority = TodoPriority.High;

        // Output:
        // False
        // True
    }

    /// <summary>Observes every editable property of a to-do item; six properties is the whole form.</summary>
    public static void ObserveWholeTodo()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority, x => x.Tags)
            .Subscribe(static values => Console.WriteLine(string.Join(", ", values.Property6)));

        item.Tags = _urgentTags;

        // Output:
        // errands, car
        // errands, car, urgent
    }

    /// <summary>Lists the labels after the title from six properties.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanged(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (title, _, _, _, _, tags) => $"{title} #{string.Join(" #", tags)}")
            .Subscribe(Console.WriteLine);

        item.Tags = _urgentTags;

        // Output:
        // Renew car registration #errands #car
        // Renew car registration #errands #car #urgent
    }

    /// <summary>Observes the assignee of an issue; the property may be null, and the path needs no null-forgiving operator.</summary>
    public static void ObserveIssueAssignee()
    {
        Issue issue = CreateCheckoutIssue();

        using IDisposable subscription = issue.WhenChanged(x => x.Assignee)
            .Subscribe(static assignee => Console.WriteLine(assignee?.Login ?? Unassigned));

        issue.Assignee = null;
        issue.Assignee = _tomas;

        // Output:
        // priya-nair
        // unassigned
        // tomas-berg
    }

    /// <summary>Observes seven properties of a GitHub issue.</summary>
    public static void ObserveIssueSummary()
    {
        Issue issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanged(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt)
            .Subscribe(static values => Console.WriteLine(values.Property2));

        issue.Title = RenamedIssueTitle;

        // Output:
        // Checkout button unresponsive on Safari
        // Checkout button unresponsive on Safari 17
    }

    /// <summary>Names the number, title and assignee of an issue from seven properties.</summary>
    public static void ProjectIssueAssignment()
    {
        Issue issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                static (number, title, _, _, assignee, _, _) => $"#{number} {title} -> {assignee?.Login ?? Unassigned}")
            .Subscribe(Console.WriteLine);

        issue.Assignee = _tomas;

        // Output:
        // #101 Checkout button unresponsive on Safari -> priya-nair
        // #101 Checkout button unresponsive on Safari -> tomas-berg
    }

    /// <summary>Observes eight properties of a GitHub issue.</summary>
    public static void ObserveIssueWithComments()
    {
        Issue issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanged(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt, x => x.Comments)
            .Subscribe(static values => Console.WriteLine(values.Property3));

        issue.State = IssueState.Closed;

        // Output:
        // Open
        // Closed
    }

    /// <summary>Finds out from eight properties whether an open issue still waits for its first reply.</summary>
    public static void ProjectIssueAwaitsFirstReply()
    {
        Issue issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanged(
                x => x.Number,
                x => x.Title,
                x => x.State,
                x => x.Author,
                x => x.Assignee!,
                x => x.Labels,
                x => x.UpdatedAt,
                x => x.Comments,
                static (_, _, state, _, _, _, _, comments) => state == IssueState.Open && comments.Count == 0)
            .Subscribe(Console.WriteLine);

        issue.State = IssueState.Closed;

        // Output:
        // True
        // False
    }

    /// <summary>Observes a transfer draft and the fields of its source account, nine properties in all.</summary>
    public static void ObserveTransferDraftAndSourceAccount()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
            .Subscribe(static values => Console.WriteLine(values.Property2));

        draft.Reference = RentReferenceRevised;

        // Output:
        // Rent March
        // Rent March, paid early
    }

    /// <summary>Checks from nine properties whether the account can cover the amount.</summary>
    public static void ProjectTransferIsAffordable()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
                static (amount, _, _, _, _, _, _, _, available) => amount <= available)
            .Subscribe(Console.WriteLine);

        draft.Amount = AboveAvailable;

        // Output:
        // True
        // False
    }

    /// <summary>Adds the payee to the transfer draft and its source account fields, ten properties in all.</summary>
    public static void ObserveTransferDraftWithPayee()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
            .Subscribe(static values => Console.WriteLine(values.Property2));

        draft.Reference = RentReferenceRevised;

        // Output:
        // Rent March
        // Rent March, paid early
    }

    /// <summary>Checks from ten properties whether the amount fits the account and the payee's limit.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
                static (amount, _, _, _, _, _, _, _, available, payee) =>
                    payee is not null && amount <= available && (payee.TransferLimit is not { } limit || amount <= limit))
            .Subscribe(Console.WriteLine);

        draft.Amount = AbovePayeeLimit;

        // Output:
        // True
        // False
    }

    /// <summary>Adds the source account itself, eleven properties in all.</summary>
    public static void ObserveTransferDraftWithSourceAccount()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
            .Subscribe(static values => Console.WriteLine(values.Property2));

        draft.Reference = RentReferenceRevised;

        // Output:
        // Rent March
        // Rent March, paid early
    }

    /// <summary>Checks from eleven properties whether a transfer has an account, a payee and enough money.</summary>
    public static void ProjectTransferIsReady()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
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
                x => x.Source!,
                static (amount, _, _, _, _, _, _, _, available, payee, source) => source is not null && payee is not null && amount <= available)
            .Subscribe(Console.WriteLine);

        draft.Amount = AboveAvailable;

        // Output:
        // True
        // False
    }

    /// <summary>Observes the session, the selected issue and the rate limit of the issue board, twelve properties in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardRateLimit()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
            .Subscribe(static values => Console.WriteLine(values.Property11.Length));

        board.NewCommentText = CommentDraft;

        // Output:
        // 0
        // 26
    }

    /// <summary>Decides from twelve properties whether the board can accept a comment.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardAcceptsComments()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
                static (signedIn, _, _, _, _, state, _, _, _, _, _, remaining) => signedIn && state == IssueState.Open && remaining > 0)
            .Subscribe(Console.WriteLine);

        board.SelectedIssue!.State = IssueState.Closed;

        // Output:
        // True
        // False
    }

    /// <summary>Adds the last error message to the issue board properties, thirteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardErrorMessage()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();
        Issue issue = board.SelectedIssue!;

        using IDisposable subscription = board
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
            .Subscribe(static values => Console.WriteLine(values.Property5));

        issue.Title = RenamedIssueTitle;

        // Output:
        // Checkout button unresponsive on Safari
        // Checkout button unresponsive on Safari 17
    }

    /// <summary>Decides from thirteen properties whether the board can accept a comment and has no error.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardAcceptsCommentsWithoutError()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
                static (signedIn, _, _, _, _, state, _, _, _, _, _, remaining, error) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0)
            .Subscribe(Console.WriteLine);

        board.SelectedIssue!.State = IssueState.Closed;

        // Output:
        // True
        // False
    }

    /// <summary>Adds the access token to the issue board properties, fourteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardToken()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();
        Issue issue = board.SelectedIssue!;

        using IDisposable subscription = board
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
            .Subscribe(static values => Console.WriteLine(values.Property5));

        issue.Title = RenamedIssueTitle;

        // Output:
        // Checkout button unresponsive on Safari
        // Checkout button unresponsive on Safari 17
    }

    /// <summary>Decides from fourteen properties whether the board can accept a comment with a token in place.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardHasTokenAndAcceptsComments()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
                static (signedIn, _, _, _, _, state, _, _, _, _, _, remaining, error, token) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0)
            .Subscribe(Console.WriteLine);

        board.SelectedIssue!.State = IssueState.Closed;

        // Output:
        // True
        // False
    }

    /// <summary>Adds the open issues of the repository to the issue board properties, fifteen in all.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardIssues()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
            .Subscribe(static values => Console.WriteLine(values.Property11.Length));

        board.NewCommentText = CommentDraft;

        // Output:
        // 0
        // 26
    }

    /// <summary>Decides from fifteen properties whether the board can accept a comment and lists issues.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardListsIssuesAndAcceptsComments()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
                static (signedIn, _, _, _, _, state, _, _, _, _, _, remaining, error, token, issues) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0 && issues.Count > 0)
            .Subscribe(Console.WriteLine);

        board.SelectedIssue!.State = IssueState.Closed;

        // Output:
        // True
        // False
    }

    /// <summary>Observes sixteen properties of the issue board, the most one call accepts.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ObserveIssueBoardRepositories()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
            .Subscribe(static values => Console.WriteLine(values.Property11.Length));

        board.NewCommentText = CommentDraft;

        // Output:
        // 0
        // 26
    }

    /// <summary>Decides from sixteen properties whether the board can accept a comment, has repositories and lists issues.</summary>
    /// <returns>A task that completes when the checks finish.</returns>
    public static async Task ProjectBoardIsReadyForComments()
    {
        IssueBoardViewModel board = await CreateBoardWithSelectedIssueAsync();

        using IDisposable subscription = board
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
                x => x.Repositories,
                static (signedIn, _, _, _, _, state, _, _, _, _, _, remaining, error, token, issues, repositories) =>
                    signedIn && state == IssueState.Open && remaining > 0 && error.Length == 0 && token.Length > 0 && issues.Count > 0 && repositories.Count > 0)
            .Subscribe(Console.WriteLine);

        board.SelectedIssue!.State = IssueState.Closed;

        // Output:
        // True
        // False
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
