// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows every <c>WhenAny</c> overload, from one observed property to twelve. <c>WhenAny</c> hands the selector one
/// <see cref="IObservedChange{TSender, TValue}"/> per property instead of the bare value. The change names the object
/// you observed and the current value. The selector runs when you subscribe and again after each change.
/// </summary>
public static class WhenAnyExamples
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

    /// <summary>The name of the payee.</summary>
    private const string PayeeName = "Harbour Realty";

    /// <summary>The account that receives the money.</summary>
    private const string PayeeAccountNumber = "062-000 1234 5678";

    /// <summary>The most the customer allows in one transfer to the payee.</summary>
    private const decimal PayeeLimit = 1000M;

    /// <summary>The amount the customer starts with.</summary>
    private const decimal RentAmount = 250M;

    /// <summary>An amount above the payee's limit and above the available balance.</summary>
    private const decimal AboveAvailable = 9000M;

    /// <summary>An amount above the payee's limit and below the available balance.</summary>
    private const decimal AbovePayeeLimit = 1500M;

    /// <summary>The reference the customer starts with.</summary>
    private const string RentReference = "Rent March";

    /// <summary>The reference the customer changes to.</summary>
    private const string RentReferenceRevised = "Rent March, paid early";

    /// <summary>The number the user types first.</summary>
    private const string FirstIssueNumber = "100";

    /// <summary>The number the user types second.</summary>
    private const string SecondIssueNumber = "101";

    /// <summary>The number the user types last.</summary>
    private const string ThirdIssueNumber = "102";

    /// <summary>The value that stands for text that is not a number.</summary>
    private const int NotANumber = -1;

    /// <summary>The repository the issue numbers belong to.</summary>
    private const string Webshop = "acme/webshop";

    /// <summary>The deadline of the registration task.</summary>
    private static readonly DateOnly _registrationDue = new(2026, 3, 20);

    /// <summary>The deadline the registration task moves to.</summary>
    private static readonly DateOnly _movedDue = new(2026, 3, 27);

    /// <summary>The labels of the registration task.</summary>
    private static readonly IReadOnlyList<string> _registrationTags = [ErrandsTag, CarTag];

    /// <summary>The labels of the registration task once it is urgent.</summary>
    private static readonly IReadOnlyList<string> _urgentTags = [ErrandsTag, CarTag, UrgentTag];

    /// <summary>The payee of the transfer.</summary>
    private static readonly Payee _landlord = new(PayeeId, PayeeName, PayeeAccountNumber, PayeeLimit);

    /// <summary>Observes the title of a to-do item and prints each change as it arrives.</summary>
    public static void ObserveTodoTitleChange()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenAny(x => x.Title, static change => change).Subscribe(static change => Console.WriteLine(change.Value));

        item.Title = RenamedTitle;

        // Output:
        // Renew car registration
        // Renew car registration online
    }

    /// <summary>Turns the title of a to-do item into its length, as a character counter does.</summary>
    public static void ProjectTodoTitleLength()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item.WhenAny(x => x.Title, static title => title.Value.Length).Subscribe(Console.WriteLine);

        item.Title = RenamedTitle;

        // Output:
        // 22
        // 29
    }

    /// <summary>Turns the title and the done flag into one checklist line.</summary>
    public static void ProjectTodoChecklistLine()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(x => x.Title, x => x.IsDone, static (title, isDone) => (isDone.Value ? DoneMark : OpenMark) + title.Value)
            .Subscribe(Console.WriteLine);

        item.IsDone = true;

        // Output:
        // [ ] Renew car registration
        // [x] Renew car registration
    }

    /// <summary>Works out how urgent a to-do item is from three properties; a finished item is never urgent.</summary>
    public static void ProjectTodoEffectivePriority()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(x => x.Title, x => x.IsDone, x => x.Priority, static (_, isDone, priority) => isDone.Value ? TodoPriority.Low : priority.Value)
            .Subscribe(static priority => Console.WriteLine(priority));

        item.Priority = TodoPriority.High;
        item.IsDone = true;

        // Output:
        // Normal
        // High
        // Low
    }

    /// <summary>Builds a headline with the deadline from four properties.</summary>
    public static void ProjectTodoHeadline()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                static (title, _, isDone, due) =>
                    due.Value is { } day && !isDone.Value ? $"{title.Value} due {day.ToString(IsoDay, CultureInfo.InvariantCulture)}" : title.Value)
            .Subscribe(Console.WriteLine);

        item.DueDate = _movedDue;

        // Output:
        // Renew car registration due 2026-03-20
        // Renew car registration due 2026-03-27
    }

    /// <summary>Decides from five properties whether a to-do item needs attention now.</summary>
    public static void ProjectTodoNeedsAttention()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                static (_, _, isDone, due, priority) => !isDone.Value && due.Value is not null && priority.Value == TodoPriority.High)
            .Subscribe(Console.WriteLine);

        item.Priority = TodoPriority.High;

        // Output:
        // False
        // True
    }

    /// <summary>Lists the labels after the title from six properties.</summary>
    public static void ProjectTodoTitleWithLabels()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (title, _, _, _, _, tags) => $"{title.Value} #{string.Join(" #", tags.Value)}")
            .Subscribe(Console.WriteLine);

        item.Tags = _urgentTags;

        // Output:
        // Renew car registration #errands #car
        // Renew car registration #errands #car #urgent
    }

    /// <summary>Numbers a to-do item from seven properties, every editable property of the item.</summary>
    public static void ProjectTodoNumberedHeadline()
    {
        TodoItem item = CreateRegistrationTask();

        using IDisposable subscription = item
            .WhenAny(
                x => x.Id,
                x => x.Title,
                x => x.Notes,
                x => x.IsDone,
                x => x.DueDate,
                x => x.Priority,
                x => x.Tags,
                static (id, title, _, isDone, _, _, _) => $"{(isDone.Value ? DoneMark : OpenMark)}{id.Value}: {title.Value}")
            .Subscribe(Console.WriteLine);

        item.Title = RenamedTitle;
        item.IsDone = true;

        // Output:
        // [ ] 7: Renew car registration
        // [ ] 7: Renew car registration online
        // [x] 7: Renew car registration online
    }

    /// <summary>Works out from eight properties of a transfer how much money the account keeps after it.</summary>
    public static void ProjectTransferRemainingBalance()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                static (amount, _, _, _, _, _, available, _) => available.Value - amount.Value)
            .Subscribe(Console.WriteLine);

        draft.Amount = AboveAvailable;

        // Output:
        // 4450.50
        // -4299.50
    }

    /// <summary>Checks from nine properties whether the account can cover the amount and the payee can receive it.</summary>
    public static void ProjectTransferIsPayable()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                static (amount, _, _, _, _, _, available, payee, _) => amount.Value <= available.Value && payee.Value.AccountNumber.Length > 0)
            .Subscribe(Console.WriteLine);

        draft.Amount = AboveAvailable;

        // Output:
        // True
        // False
    }

    /// <summary>Checks from ten properties whether the amount fits the account and the payee's limit.</summary>
    public static void ProjectTransferWithinPayeeLimit()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                x => x.Source!.Kind,
                static (amount, _, _, _, _, _, available, payee, _, _) =>
                    amount.Value <= available.Value && (payee.Value.TransferLimit is not { } max || amount.Value <= max))
            .Subscribe(Console.WriteLine);

        draft.Amount = AbovePayeeLimit;

        // Output:
        // True
        // False
    }

    /// <summary>Writes the statement line from eleven properties: the account, the payee and the reference. The payee is replaced as a whole.</summary>
    public static void ProjectTransferStatementLine()
    {
        TransferDraft draft = CreateTransferDraft();

        using IDisposable subscription = draft
            .WhenAny(
                x => x.Amount,
                x => x.Reference,
                x => x.Source!.Name,
                x => x.Source!.Currency,
                x => x.Source!.Balance,
                x => x.Source!.OverdraftLimit,
                x => x.Source!.AvailableBalance,
                x => x.Payee!,
                x => x.Source!.Id,
                x => x.Source!.Kind,
                x => x.Source!,
                static (_, reference, _, _, _, _, _, payee, sourceId, _, _) => $"{sourceId.Value} -> {payee.Value.Name}: {reference.Value}")
            .Subscribe(Console.WriteLine);

        draft.Reference = RentReferenceRevised;

        // Output:
        // ACC-1001 -> Harbour Realty: Rent March
        // ACC-1001 -> Harbour Realty: Rent March, paid early
    }

    /// <summary>Flags from twelve properties of the transfer screen, the most one call accepts, a transfer that needs a manual review.</summary>
    public static void ProjectTransferNeedsReview()
    {
        TransferViewModel screen = new(new InMemoryBankingBackend());
        TransferDraft draft = screen.Draft;
        draft.Source = CreateEverydayAccount();
        draft.Payee = _landlord;
        draft.Amount = RentAmount;
        draft.Reference = RentReference;

        using IDisposable subscription = screen
            .WhenAny(
                x => x.Draft.Amount,
                x => x.Draft.Reference,
                x => x.Draft.Source!.Name,
                x => x.Draft.Source!.Currency,
                x => x.Draft.Source!.Balance,
                x => x.Draft.Source!.OverdraftLimit,
                x => x.Draft.Source!.AvailableBalance,
                x => x.Draft.Payee!,
                x => x.Draft.Source!.Id,
                x => x.Draft.Source!.Kind,
                x => x.IsValid,
                x => x.ErrorMessage,
                static (amount, _, _, _, _, _, available, _, _, kind, _, _) => kind.Value == AccountKind.Savings || amount.Value > available.Value)
            .Subscribe(Console.WriteLine);

        draft.Source!.Kind = AccountKind.Savings;

        // Output:
        // False
        // True
    }

    /// <summary>Jumps to the issue number the user types. A number that does not parse throws in the selector, and Catch replaces the failure with a value and ends the pipeline.</summary>
    public static void CatchEndsThePipelineAfterOneFailure()
    {
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueSearchViewModel viewModel = new(server, Webshop) { SearchTerm = FirstIssueNumber };

        using IDisposable subscription = viewModel
            .WhenAny(x => x.SearchTerm, static term => int.Parse(term.Value, CultureInfo.InvariantCulture))
            .Catch<int, FormatException>(static _ => Signal.Return(NotANumber))
            .Subscribe(Console.WriteLine);

        viewModel.SearchTerm = SecondIssueNumber;
        viewModel.SearchTerm = "abc";

        // The pipeline has ended, so this number is never delivered.
        viewModel.SearchTerm = ThirdIssueNumber;

        // Output:
        // 100
        // 101
        // -1
    }

    /// <summary>Jumps to the issue number the user types. Handling a number that does not parse inside the selector keeps the pipeline alive.</summary>
    public static void HandleFailuresInsideTheSelector()
    {
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueSearchViewModel viewModel = new(server, Webshop) { SearchTerm = FirstIssueNumber };

        using IDisposable subscription = viewModel
            .WhenAny(x => x.SearchTerm, static term => int.TryParse(term.Value, CultureInfo.InvariantCulture, out var number) ? number : NotANumber)
            .Subscribe(Console.WriteLine);

        viewModel.SearchTerm = SecondIssueNumber;
        viewModel.SearchTerm = "abc";
        viewModel.SearchTerm = ThirdIssueNumber;

        // Output:
        // 100
        // 101
        // -1
        // 102
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
}
