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
/// Shows every <c>WhenChanging</c> overload, from one observed property to sixteen, each with and without a value
/// selector. <c>WhenChanging</c> delivers the current values when you subscribe. It delivers them again just before each
/// change, so the values are the ones the change is about to replace.
/// </summary>
public static class WhenChangingExamples
{
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

    /// <summary>The status text of a finished task.</summary>
    private const string DoneText = "done";

    /// <summary>The status text of an open task.</summary>
    private const string OpenText = "open";

    /// <summary>The format of a day in a headline.</summary>
    private const string IsoDay = "yyyy-MM-dd";

    /// <summary>The number of the checkout issue.</summary>
    private const int CheckoutNumber = 101;

    /// <summary>The title of the checkout issue.</summary>
    private const string CheckoutTitle = "Checkout button unresponsive on Safari";

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

    /// <summary>The first reply on the checkout issue.</summary>
    private const string FirstReplyText = "Reproduced on Safari 17.";

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

    /// <summary>The money the customer can spend: the balance plus the overdraft limit.</summary>
    private const decimal EverydayAvailable = 4700.50M;

    /// <summary>The identifier of the payee.</summary>
    private const int PayeeId = 3;

    /// <summary>The identifier of the payee the customer switches to.</summary>
    private const int OtherPayeeId = 4;

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

    /// <summary>The private note the customer starts with.</summary>
    private const string MemoText = "First payment under the new lease";

    /// <summary>The private note the customer changes to.</summary>
    private const string MemoRevised = "Second payment under the new lease";

    /// <summary>What the transfer pays for.</summary>
    private const string PurposeText = "Housing";

    /// <summary>What the transfer pays for once the customer changes it.</summary>
    private const string PurposeRevised = "Rent";

    /// <summary>The fee the bank charges for the transfer.</summary>
    private const decimal TransferFee = 2.50M;

    /// <summary>The text of a transfer that repeats.</summary>
    private const string MonthlyText = "monthly";

    /// <summary>The text of a transfer that is sent once.</summary>
    private const string OnceText = "once";

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

    /// <summary>When the checkout issue last changed once it has a reply.</summary>
    private static readonly DateTimeOffset _repliedAt = new(2026, 3, 3, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The comments of the checkout issue once it has a reply.</summary>
    private static readonly IReadOnlyList<IssueComment> _firstReply = [new(PriyaLogin, FirstReplyText, _repliedAt)];

    /// <summary>The person the issue is assigned to.</summary>
    private static readonly User _priya = new() { Login = PriyaLogin, DisplayName = PriyaName };

    /// <summary>The person who reported the issue.</summary>
    private static readonly User _maria = new() { Login = "maria-santos", DisplayName = "Maria Santos" };

    /// <summary>The person the issue is reassigned to.</summary>
    private static readonly User _tomas = new() { Login = TomasLogin, DisplayName = "Tomas Berg" };

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, "Harbour Realty", "062-000 1234 5678", PayeeLimit);

    /// <summary>The payee the customer switches to.</summary>
    private static readonly Payee _strata = new(OtherPayeeId, "Bayside Strata", "062-000 8765 4321", null);

    /// <summary>The day the transfer is sent.</summary>
    private static readonly DateOnly _scheduledFor = new(2026, 4, 1);

    /// <summary>The day the transfer is moved to.</summary>
    private static readonly DateOnly _rescheduledFor = new(2026, 4, 8);

    /// <summary>Observes the title of a to-do item; a single property delivers the value itself.</summary>
    public static void ObserveTodoTitle()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenChanging(x => x.Title).Subscribe(Console.WriteLine);

        item.Title = RenamedTitle;

        Console.WriteLine(item.Title);

        // Output:
        // Renew car registration
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Observes the title and the done flag of a to-do item together.</summary>
    public static void ObserveTodoTitleAndStatus()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenChanging(x => x.Title, x => x.IsDone).Subscribe(static values => Console.WriteLine(values.Property2));

        item.IsDone = true;

        Console.WriteLine(item.IsDone);

        // Output:
        // False
        // False
        // True
    }

    /// <summary>Prints the status line a to-do item had before its done flag changed, with a value selector.</summary>
    public static void ProjectTodoStatusLine()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.IsDone, static (title, isDone) => $"{title} ({(isDone ? DoneText : OpenText)})")
            .Subscribe(Console.WriteLine);

        item.IsDone = true;

        // Output:
        // Renew car registration (open)
        // Renew car registration (open)
    }

    /// <summary>Observes the title, the done flag and the priority of a to-do item.</summary>
    public static void ObserveTodoTitleStatusAndPriority()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.IsDone, x => x.Priority)
            .Subscribe(static values => Console.WriteLine(values.Property3));

        item.Priority = TodoPriority.High;

        Console.WriteLine(item.Priority);

        // Output:
        // Normal
        // Normal
        // High
    }

    /// <summary>Prints how urgent a to-do item was before its priority changed; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone ? TodoPriority.Low : priority)
            .Subscribe(static priority => Console.WriteLine(priority));

        item.Priority = TodoPriority.High;

        // Output:
        // Normal
        // Normal
    }

    /// <summary>Observes the title, notes, done flag and deadline of a to-do item.</summary>
    public static void ObserveTodoDeadline()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate)
            .Subscribe(static values => Console.WriteLine(values.Property4));

        item.DueDate = _movedDue;

        Console.WriteLine(item.DueDate);

        // Output:
        // 03/20/2026
        // 03/20/2026
        // 03/27/2026
    }

    /// <summary>Prints the headline a to-do item had before its deadline moved.</summary>
    public static void ProjectTodoHeadline()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) => isDone || due is null ? title : $"{title} due {due.Value.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Subscribe(Console.WriteLine);

        item.DueDate = _movedDue;

        // Output:
        // Renew car registration due 2026-03-20
        // Renew car registration due 2026-03-20
    }

    /// <summary>Observes the title, notes, done flag, deadline and priority of a to-do item.</summary>
    public static void ObserveTodoWithPriority()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority)
            .Subscribe(static values => Console.WriteLine(values.Property5));

        item.Priority = TodoPriority.High;

        Console.WriteLine(item.Priority);

        // Output:
        // Normal
        // Normal
        // High
    }

    /// <summary>Prints whether a high-priority item needed attention before it was finished.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        TodoItemDraft item = CreateRegistrationTask();

        item.Priority = TodoPriority.High;

        using IDisposable subscription = item
            .WhenChanging(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone && due is not null && priority == TodoPriority.High)
            .Subscribe(Console.WriteLine);

        item.IsDone = true;

        // Output:
        // True
        // True
    }

    /// <summary>Observes every editable property of a to-do item; six properties is the whole form.</summary>
    public static void ObserveWholeTodo()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(x => x.Title, x => x.Notes, x => x.IsDone, x => x.DueDate, x => x.Priority, x => x.Tags)
            .Subscribe(static values => Console.WriteLine(string.Join(", ", values.Property6)));

        item.Tags = _urgentTags;

        Console.WriteLine(string.Join(", ", item.Tags));

        // Output:
        // errands, car
        // errands, car
        // errands, car, urgent
    }

    /// <summary>Prints the title with its labels as it was before the labels changed.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        TodoItemDraft item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenChanging(
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
        // Renew car registration #errands #car
    }

    /// <summary>Observes seven properties of an issue.</summary>
    public static void ObserveIssueSummary()
    {
        IssueDraft issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanging(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt)
            .Subscribe(static values => Console.WriteLine(values.Property7));

        issue.UpdatedAt = _repliedAt;

        Console.WriteLine(issue.UpdatedAt);

        // Output:
        // 03/02/2026 15:05:00 +00:00
        // 03/02/2026 15:05:00 +00:00
        // 03/03/2026 09:00:00 +00:00
    }

    /// <summary>Prints the assignment line an issue had before it was reassigned, from seven properties.</summary>
    public static void ProjectIssueAssignment()
    {
        IssueDraft issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanging(
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

        Console.WriteLine(issue.Assignee!.Login);

        // Output:
        // #101 Checkout button unresponsive on Safari -> priya-nair
        // #101 Checkout button unresponsive on Safari -> priya-nair
        // tomas-berg
    }

    /// <summary>Observes eight properties of an issue.</summary>
    public static void ObserveIssueWithComments()
    {
        IssueDraft issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanging(x => x.Number, x => x.Title, x => x.State, x => x.Author, x => x.Assignee!, x => x.Labels, x => x.UpdatedAt, x => x.Comments)
            .Subscribe(static values => Console.WriteLine(values.Property8.Count));

        issue.Comments = _firstReply;

        Console.WriteLine(issue.Comments.Count);

        // Output:
        // 0
        // 0
        // 1
    }

    /// <summary>Prints whether an open issue still waited for its first reply before the reply arrived.</summary>
    public static void ProjectIssueAwaitsFirstReply()
    {
        IssueDraft issue = CreateCheckoutIssue();

        using IDisposable subscription = issue
            .WhenChanging(
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

        issue.Comments = _firstReply;

        // Output:
        // True
        // True
    }

    /// <summary>Observes a transfer and the fields of its source account, nine properties in all.</summary>
    public static void ObserveTransferAndSourceAccount()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance)
            .Subscribe(static values => Console.WriteLine(values.Property9));

        form.AvailableBalance = EverydayBalance;

        Console.WriteLine(form.AvailableBalance);

        // Output:
        // 4700.50
        // 4700.50
        // 4200.50
    }

    /// <summary>Prints whether the account could cover the amount before the amount changed.</summary>
    public static void ProjectTransferIsAffordable()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                static (amount, _, _, _, _, _, _, _, available) => amount <= available)
            .Subscribe(Console.WriteLine);

        form.Amount = AboveAvailable;

        // Output:
        // True
        // True
    }

    /// <summary>Adds the payee to the transfer and its source account fields, ten properties in all.</summary>
    public static void ObserveTransferWithPayee()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!)
            .Subscribe(static values => Console.WriteLine(values.Property10?.Name));

        form.Payee = _strata;

        Console.WriteLine(form.Payee.Name);

        // Output:
        // Harbour Realty
        // Harbour Realty
        // Bayside Strata
    }

    /// <summary>Prints whether the amount fitted the account and the payee's limit before the amount changed.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                static (amount, _, _, _, _, _, _, _, available, payee) =>
                    payee is not null && amount <= available && (payee.TransferLimit is not { } limit || amount <= limit))
            .Subscribe(Console.WriteLine);

        form.Amount = AbovePayeeLimit;

        // Output:
        // True
        // True
    }

    /// <summary>Adds the day the transfer is sent, eleven properties in all.</summary>
    public static void ObserveTransferSchedule()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor)
            .Subscribe(static values => Console.WriteLine(values.Property11));

        form.ScheduledFor = _rescheduledFor;

        Console.WriteLine(form.ScheduledFor);

        // Output:
        // 04/01/2026
        // 04/01/2026
        // 04/08/2026
    }

    /// <summary>Prints the statement line the transfer had before it was rescheduled.</summary>
    public static void ProjectTransferStatementLine()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                static (_, reference, _, _, _, _, _, _, _, _, scheduledFor) => $"{reference} on {scheduledFor.ToString(IsoDay, CultureInfo.InvariantCulture)}")
            .Subscribe(Console.WriteLine);

        form.ScheduledFor = _rescheduledFor;

        // Output:
        // Rent March on 2026-04-01
        // Rent March on 2026-04-01
    }

    /// <summary>Adds whether the transfer repeats, twelve properties in all.</summary>
    public static void ObserveTransferRecurrence()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring)
            .Subscribe(static values => Console.WriteLine(values.Property12));

        form.IsRecurring = false;

        Console.WriteLine(form.IsRecurring);

        // Output:
        // True
        // True
        // False
    }

    /// <summary>Prints how the transfer was described before it stopped repeating.</summary>
    public static void ProjectTransferRecurrenceLabel()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                static (_, reference, _, _, _, _, _, _, _, _, _, isRecurring) => $"{reference} ({(isRecurring ? MonthlyText : OnceText)})")
            .Subscribe(Console.WriteLine);

        form.IsRecurring = false;

        // Output:
        // Rent March (monthly)
        // Rent March (monthly)
    }

    /// <summary>Adds whether the payee is told, thirteen properties in all.</summary>
    public static void ObserveTransferNotification()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee)
            .Subscribe(static values => Console.WriteLine(values.Property13));

        form.NotifyPayee = false;

        Console.WriteLine(form.NotifyPayee);

        // Output:
        // True
        // True
        // False
    }

    /// <summary>Prints whether the payee was going to be told before the customer switched the notice off.</summary>
    public static void ProjectTransferNotifiesPayee()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                static (_, _, _, _, _, _, _, _, _, payee, _, _, notifyPayee) => notifyPayee && payee is not null)
            .Subscribe(Console.WriteLine);

        form.NotifyPayee = false;

        // Output:
        // True
        // True
    }

    /// <summary>Adds the private note, fourteen properties in all.</summary>
    public static void ObserveTransferMemo()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo)
            .Subscribe(static values => Console.WriteLine(values.Property14));

        form.Memo = MemoRevised;

        Console.WriteLine(form.Memo);

        // Output:
        // First payment under the new lease
        // First payment under the new lease
        // Second payment under the new lease
    }

    /// <summary>Prints the note the customer was about to replace, or the reference when there was none.</summary>
    public static void ProjectTransferNote()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                static (_, reference, _, _, _, _, _, _, _, _, _, _, _, memo) => memo.Length > 0 ? memo : reference)
            .Subscribe(Console.WriteLine);

        form.Memo = MemoRevised;

        // Output:
        // First payment under the new lease
        // First payment under the new lease
    }

    /// <summary>Adds what the transfer pays for, fifteen properties in all.</summary>
    public static void ObserveTransferPurpose()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose)
            .Subscribe(static values => Console.WriteLine(values.Property15));

        form.Purpose = PurposeRevised;

        Console.WriteLine(form.Purpose);

        // Output:
        // Housing
        // Housing
        // Rent
    }

    /// <summary>Prints the purpose and reference the transfer had before the purpose changed.</summary>
    public static void ProjectTransferPurposeLine()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                static (_, reference, _, _, _, _, _, _, _, _, _, _, _, _, purpose) => $"{purpose}: {reference}")
            .Subscribe(Console.WriteLine);

        form.Purpose = PurposeRevised;

        // Output:
        // Housing: Rent March
        // Housing: Rent March
    }

    /// <summary>Observes sixteen properties of the transfer screen, the most one call accepts.</summary>
    public static void ObserveTransferFee()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee)
            .Subscribe(static values => Console.WriteLine(values.Property16));

        form.Fee = 0M;

        Console.WriteLine(form.Fee);

        // Output:
        // 2.50
        // 2.50
        // 0
    }

    /// <summary>Prints whether the amount and fee fitted the account before the fee changed.</summary>
    public static void ProjectTransferTotalIsAffordable()
    {
        TransferForm form = CreateTransferForm();

        using IDisposable subscription = form
            .WhenChanging(
                x => x.Amount,
                x => x.Reference,
                x => x.SourceId,
                x => x.SourceName,
                x => x.SourceKind,
                x => x.Currency,
                x => x.Balance,
                x => x.OverdraftLimit,
                x => x.AvailableBalance,
                x => x.Payee!,
                x => x.ScheduledFor,
                x => x.IsRecurring,
                x => x.NotifyPayee,
                x => x.Memo,
                x => x.Purpose,
                x => x.Fee,
                static (amount, _, _, _, _, _, _, _, available, _, _, _, _, _, _, fee) => amount + fee <= available)
            .Subscribe(Console.WriteLine);

        form.Fee = AboveAvailable;

        // Output:
        // True
        // True
    }

    /// <summary>Creates the to-do item form the examples observe.</summary>
    /// <returns>An open, unfinished item.</returns>
    private static TodoItemDraft CreateRegistrationTask() => new()
    {
        Title = RegistrationTitle,
        Notes = RegistrationNotes,
        DueDate = _registrationDue,
        Priority = TodoPriority.Normal,
        Tags = _registrationTags,
    };

    /// <summary>Creates the issue form the examples observe.</summary>
    /// <returns>An open issue reported by Maria and assigned to Priya.</returns>
    private static IssueDraft CreateCheckoutIssue() => new()
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

    /// <summary>Creates the transfer screen the examples observe.</summary>
    /// <returns>A transfer that sends the rent from the everyday account to the landlord.</returns>
    private static TransferForm CreateTransferForm() => new()
    {
        Amount = RentAmount,
        Reference = RentReference,
        SourceId = EverydayId,
        SourceName = EverydayName,
        SourceKind = AccountKind.Everyday,
        Currency = Currency,
        Balance = EverydayBalance,
        OverdraftLimit = EverydayOverdraft,
        AvailableBalance = EverydayAvailable,
        Payee = _landlord,
        ScheduledFor = _scheduledFor,
        IsRecurring = true,
        NotifyPayee = true,
        Memo = MemoText,
        Purpose = PurposeText,
        Fee = TransferFee,
    };
}
