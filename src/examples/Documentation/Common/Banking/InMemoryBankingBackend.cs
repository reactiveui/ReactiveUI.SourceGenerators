// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>
/// A core banking backend that lives in memory. It checks a transfer against the balance of the account, the limit
/// of the payee and the limit of the day. It holds a transfer to a flagged payee for a fraud review, and it asks for
/// a one-time approval code from <see cref="ApprovalThreshold"/> upwards. Its time is <see cref="Now"/>, which
/// starts at 09:00 UTC on Tuesday 3 March 2026 and stamps transactions and starts each day's limit. Each call
/// takes <see cref="Latency"/> to answer. It hands out copies, so changing an account you read does not change the
/// bank until you make a transfer.
/// </summary>
[System.Diagnostics.DebuggerDisplay("InMemoryBankingBackend: DailyLimit = {DailyLimit}, IsUnavailable = {IsUnavailable}")]
public sealed class InMemoryBankingBackend : IBankingBackend
{
    /// <summary>The currency of every account.</summary>
    private const string Aud = "AUD";

    /// <summary>The account number of the payee the bank holds transfers to.</summary>
    private const string FlaggedAccountNumber = "033-112 9004 5511";

    /// <summary>The accounts a new backend starts with.</summary>
    private static readonly Account[] _seedAccounts =
    [
        new() { Id = "ACC-1001", Name = "Everyday Account", Kind = AccountKind.Everyday, Currency = Aud, Balance = 2450.75M, OverdraftLimit = 500M },
        new() { Id = "ACC-2002", Name = "Savings Account", Kind = AccountKind.Savings, Currency = Aud, Balance = 15230.00M },
    ];

    /// <summary>The payees a new backend starts with. The last one is flagged for fraud review.</summary>
    private static readonly Payee[] _seedPayees =
    [
        new(1, "City Power and Water", "062-000 4455 1122", null),
        new(2, "Sam Whitfield (rent)", "083-004 7781 2290", 1500M),
        new(3, "Northwind Traders", FlaggedAccountNumber, null),
    ];

    /// <summary>The transactions a new backend starts with, newest first.</summary>
    private static readonly Transaction[] _seedTransactions =
    [
        new(4, "ACC-1001", new DateTimeOffset(2026, 3, 2, 23, 10, 0, TimeSpan.Zero), "Coles Supermarket", -86.40M, 2450.75M),
        new(3, "ACC-1001", new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero), "Rent to Sam Whitfield", -1200.00M, 2537.15M),
        new(2, "ACC-1001", new DateTimeOffset(2026, 2, 27, 0, 0, 0, TimeSpan.Zero), "Salary from Acme Pty Ltd", 3200.00M, 3737.15M),
        new(1, "ACC-1001", new DateTimeOffset(2026, 2, 24, 5, 30, 0, TimeSpan.Zero), "Energex electricity", -142.30M, 537.15M),
    ];

    /// <summary>The stored accounts, copies of the seed so each backend changes its own.</summary>
    private readonly List<Account> _accounts = CopySeedAccounts();

    /// <summary>The stored transactions, newest first.</summary>
    private readonly List<Transaction> _transactions = [.. _seedTransactions];

    /// <summary>The money sent on each day, by day.</summary>
    private readonly Dictionary<DateOnly, decimal> _sentByDay = [];

    /// <summary>The number of the last receipt.</summary>
    private int _receipts;

    /// <summary>Gets the one-time code that approves a large transfer.</summary>
    public static string ApprovalCode { get; } = "482913";

    /// <summary>Gets or sets how long each call takes to answer. Zero answers on the next turn of the scheduler.</summary>
    public TimeSpan Latency { get; set; }

    /// <summary>Gets or sets the time the backend reports. It stamps transactions and receipts and decides which day a transfer counts against.</summary>
    public DateTimeOffset Now { get; set; } = new(2026, 3, 3, 9, 0, 0, TimeSpan.Zero);

    /// <summary>Gets or sets a value indicating whether every call fails with <see cref="BankingFailure.ServiceUnavailable"/>.</summary>
    public bool IsUnavailable { get; set; }

    /// <summary>Gets or sets the most the customer can send in one day.</summary>
    public decimal DailyLimit { get; set; } = 5000M;

    /// <summary>Gets or sets the amount from which a transfer needs an approval code.</summary>
    public decimal ApprovalThreshold { get; set; } = 1000M;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Account>> GetAccountsAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        List<Account> accounts = [];
        foreach (Account account in _accounts)
        {
            accounts.Add(account.Clone());
        }

        return accounts;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(string accountId)
    {
        await EnterAsync().ConfigureAwait(false);

        _ = FindAccount(accountId);
        List<Transaction> transactions = [];
        foreach (Transaction transaction in _transactions)
        {
            if (transaction.AccountId == accountId)
            {
                transactions.Add(transaction);
            }
        }

        return transactions;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Payee>> GetPayeesAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        return _seedPayees.ToList();
    }

    /// <inheritdoc/>
    public async Task<TransferValidation> ValidateTransferAsync(TransferRequest request)
    {
        await EnterAsync().ConfigureAwait(false);

        List<string> errors = [];
        foreach (var (_, message) in Violations(request))
        {
            errors.Add(message);
        }

        return new(errors);
    }

    /// <inheritdoc/>
    public Task<TransferReceipt> SubmitTransferAsync(TransferRequest request) => SubmitAsync(request, null);

    /// <inheritdoc/>
    public Task<TransferReceipt> SubmitTransferAsync(TransferRequest request, string approvalCode) => SubmitAsync(request, approvalCode);

    /// <summary>Copies the accounts a new backend starts with.</summary>
    /// <returns>Independent copies of the seed accounts.</returns>
    private static List<Account> CopySeedAccounts()
    {
        List<Account> accounts = [];
        foreach (Account account in _seedAccounts)
        {
            accounts.Add(account.Clone());
        }

        return accounts;
    }

    /// <summary>Finds a payee.</summary>
    /// <param name="payeeId">The identifier of the payee.</param>
    /// <returns>The payee.</returns>
    /// <exception cref="BankingException">The payee does not exist.</exception>
    private static Payee FindPayee(int payeeId)
    {
        foreach (Payee payee in _seedPayees)
        {
            if (payee.Id == payeeId)
            {
                return payee;
            }
        }

        throw new BankingException(BankingFailure.UnknownPayee, $"There is no payee {payeeId}.");
    }

    /// <summary>Makes a transfer after the checks of the bank.</summary>
    /// <param name="request">The transfer to make.</param>
    /// <param name="approvalCode">The code the customer entered, or <see langword="null"/> when the customer has not been asked.</param>
    /// <returns>The receipt for the transfer.</returns>
    /// <exception cref="BankingException">The transfer breaks a rule, the bank holds it for a fraud review, it needs an approval code, or the code is wrong.</exception>
    private async Task<TransferReceipt> SubmitAsync(TransferRequest request, string? approvalCode)
    {
        await EnterAsync().ConfigureAwait(false);

        List<(BankingFailure Failure, string Message)> violations = Violations(request);
        if (violations.Count > 0)
        {
            throw new BankingException(violations[0].Failure, violations[0].Message);
        }

        Payee payee = FindPayee(request.PayeeId);
        if (payee.AccountNumber == FlaggedAccountNumber)
        {
            throw new BankingException(BankingFailure.FraudHold, $"The transfer to {payee.Name} is held for a fraud review.");
        }

        if (request.Amount >= ApprovalThreshold)
        {
            RequireApproval(approvalCode);
        }

        Account account = FindAccount(request.SourceAccountId);
        DateOnly day = DateOnly.FromDateTime(Now.UtcDateTime);
        account.Balance -= request.Amount;
        _sentByDay[day] = SentOn(day) + request.Amount;
        _transactions.Insert(0, new(_transactions.Count + 1, account.Id, Now, $"Transfer to {payee.Name}", -request.Amount, account.Balance));
        _receipts++;
        return new($"RCPT-{_receipts.ToString("D6", CultureInfo.InvariantCulture)}", request.Amount, account.Balance, Now);
    }

    /// <summary>Waits for <see cref="Latency"/>, then fails when the backend is unavailable.</summary>
    /// <returns>A task that completes when the call may proceed.</returns>
    /// <exception cref="BankingException">The backend is unavailable.</exception>
    private async Task EnterAsync()
    {
        if (Latency == TimeSpan.Zero)
        {
            await Task.Yield();
        }
        else
        {
            await Task.Delay(Latency).ConfigureAwait(false);
        }

        if (IsUnavailable)
        {
            throw new BankingException(BankingFailure.ServiceUnavailable, "The banking service is unavailable.");
        }
    }

    /// <summary>Checks the approval code of a large transfer.</summary>
    /// <param name="approvalCode">The code the customer entered, or <see langword="null"/> when the customer has not been asked.</param>
    /// <exception cref="BankingException">The code is missing or wrong.</exception>
    private void RequireApproval(string? approvalCode)
    {
        if (approvalCode is null)
        {
            throw new BankingException(BankingFailure.ApprovalRequired, $"A transfer of {ApprovalThreshold} or more needs an approval code.");
        }

        if (approvalCode != ApprovalCode)
        {
            throw new BankingException(BankingFailure.InvalidApprovalCode, "The approval code is not correct.");
        }
    }

    /// <summary>Finds every rule a transfer breaks.</summary>
    /// <param name="request">The transfer to check.</param>
    /// <returns>The broken rules, in the order the bank checks them.</returns>
    /// <exception cref="BankingException">The account or payee does not exist.</exception>
    private List<(BankingFailure Failure, string Message)> Violations(TransferRequest request)
    {
        Account account = FindAccount(request.SourceAccountId);
        Payee payee = FindPayee(request.PayeeId);
        List<(BankingFailure, string)> violations = [];

        if (request.Amount <= 0)
        {
            violations.Add((BankingFailure.InvalidAmount, "The amount must be more than zero."));
            return violations;
        }

        if (payee.TransferLimit is { } limit && request.Amount > limit)
        {
            violations.Add((BankingFailure.PayeeLimitExceeded, $"The limit for {payee.Name} is {limit}."));
        }

        if (request.Amount > account.AvailableBalance)
        {
            violations.Add((BankingFailure.InsufficientFunds, $"{account.Name} has {account.AvailableBalance} available."));
        }

        DateOnly day = DateOnly.FromDateTime(Now.UtcDateTime);
        if (SentOn(day) + request.Amount > DailyLimit)
        {
            violations.Add((BankingFailure.DailyLimitExceeded, $"The daily limit is {DailyLimit}."));
        }

        return violations;
    }

    /// <summary>Adds up the money sent on a day.</summary>
    /// <param name="day">The day.</param>
    /// <returns>The money sent, or 0 when nothing was sent.</returns>
    private decimal SentOn(DateOnly day) => _sentByDay.GetValueOrDefault(day);

    /// <summary>Finds an account.</summary>
    /// <param name="accountId">The identifier of the account.</param>
    /// <returns>The stored account.</returns>
    /// <exception cref="BankingException">The account does not exist.</exception>
    private Account FindAccount(string accountId) =>
        _accounts.Find(account => account.Id == accountId)
        ?? throw new BankingException(BankingFailure.UnknownAccount, $"There is no account {accountId}.");
}
