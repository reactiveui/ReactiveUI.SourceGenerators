// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>
/// The view model behind the transfer screen. It checks the draft as the customer edits it, and its
/// <see cref="TransferCommand"/> asks the view for a confirmation, sends the transfer and, when the bank asks for
/// one, asks the view for an approval code. Each command starts the matching method and returns; await the method
/// itself to wait for its work. A refused request never throws from a method. It sets <see cref="ErrorMessage"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TransferViewModel: Draft = {Draft}, IsValid = {IsValid}")]
public sealed class TransferViewModel : ObservableObject
{
    /// <summary>The separator between the messages of <see cref="ValidationSummary"/>.</summary>
    private const string MessageSeparator = " ";

    /// <summary>The backend the view model calls.</summary>
    private readonly IBankingBackend _backend;

    /// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
    /// <param name="backend">The backend to call.</param>
    public TransferViewModel(IBankingBackend backend)
    {
        _backend = backend;
        LoadCommand = new(() => _ = LoadAsync());
        TransferCommand = new(() => _ = TransferAsync(), () => IsValid);
        Draft.PropertyChanged += OnDraftChanged;
        Validate();
    }

    /// <summary>Gets the question the view answers before a transfer is sent. The input is the draft; the answer is <see langword="true"/> to send it.</summary>
    public Interaction<TransferDraft, bool> ConfirmTransfer { get; } = new();

    /// <summary>Gets the question the view answers when the bank wants a one-time code. The input is the draft; the answer is the code, or an empty string to cancel the transfer.</summary>
    public Interaction<TransferDraft, string> ApproveTransfer { get; } = new();

    /// <summary>Gets the command that runs <see cref="LoadAsync"/>.</summary>
    public Command LoadCommand { get; }

    /// <summary>Gets the command that runs <see cref="TransferAsync"/>; it runs only while <see cref="IsValid"/> is <see langword="true"/>.</summary>
    public Command TransferCommand { get; }

    /// <summary>Gets the transfer the customer is filling in.</summary>
    public TransferDraft Draft { get; } = new();

    /// <summary>Gets the accounts the money can leave.</summary>
    public IReadOnlyList<Account> Accounts
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets the payees the money can go to.</summary>
    public IReadOnlyList<Payee> Payees
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets a value indicating whether the draft breaks none of the rules the view model can check without the bank.</summary>
    public bool IsValid
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                TransferCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>Gets the rules the draft breaks, one message each.</summary>
    public IReadOnlyList<string> ValidationErrors
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets the messages of <see cref="ValidationErrors"/> as one line, or an empty string.</summary>
    public string ValidationSummary
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets the receipt of the last transfer, or <see langword="null"/> before the first transfer.</summary>
    public TransferReceipt? LastReceipt
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last refused request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Loads the accounts and the payees.</summary>
    /// <returns>A task that completes when both are loaded or the request was refused.</returns>
    public async Task LoadAsync()
    {
        try
        {
            Accounts = await _backend.GetAccountsAsync().ConfigureAwait(false);
            Payees = await _backend.GetPayeesAsync().ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (BankingException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Confirms and sends the draft, asking for an approval code when the bank wants one.</summary>
    /// <returns>A task that completes when the transfer is sent, the customer cancelled or the request was refused.</returns>
    public async Task TransferAsync()
    {
        if (Draft.Source is not { } source || Draft.Payee is not { } payee)
        {
            return;
        }

        if (!await ConfirmTransfer.Handle(Draft).ConfigureAwait(false))
        {
            return;
        }

        TransferRequest request = new(source.Id, payee.Id, Draft.Amount, Draft.Reference);

        try
        {
            TransferReceipt? receipt = await SendAsync(request).ConfigureAwait(false);
            if (receipt is null)
            {
                ErrorMessage = "The transfer was cancelled.";
                return;
            }

            LastReceipt = receipt;
            ErrorMessage = string.Empty;
            source.Balance = receipt.NewBalance;
            Draft.Amount = 0;
            Draft.Reference = string.Empty;
        }
        catch (BankingException ex)
        {
            ErrorMessage = $"{ex.Failure}: {ex.Message}";
        }
    }

    /// <summary>Sends a transfer, and sends it again with an approval code when the bank asks for one.</summary>
    /// <param name="request">The transfer to send.</param>
    /// <returns>The receipt, or <see langword="null"/> when the customer declined to enter a code.</returns>
    /// <exception cref="BankingException">The bank refused the transfer.</exception>
    private async Task<TransferReceipt?> SendAsync(TransferRequest request)
    {
        try
        {
            return await _backend.SubmitTransferAsync(request).ConfigureAwait(false);
        }
        catch (BankingException ex) when (ex.Failure == BankingFailure.ApprovalRequired)
        {
            string code = await ApproveTransfer.Handle(Draft).ConfigureAwait(false);
            return code.Length == 0 ? null : await _backend.SubmitTransferAsync(request, code).ConfigureAwait(false);
        }
    }

    /// <summary>Checks the draft again when one of its properties changes.</summary>
    /// <param name="sender">The draft.</param>
    /// <param name="e">The event data.</param>
    private void OnDraftChanged(object? sender, PropertyChangedEventArgs e) => Validate();

    /// <summary>Checks the draft against the rules the view model can check without the bank.</summary>
    private void Validate()
    {
        List<string> errors = [];

        if (Draft.Source is null)
        {
            errors.Add("Choose the account to pay from.");
        }

        if (Draft.Payee is null)
        {
            errors.Add("Choose who to pay.");
        }

        if (Draft.Amount <= 0)
        {
            errors.Add("Enter an amount above zero.");
        }
        else if (Draft.Source is { } source && Draft.Amount > source.AvailableBalance)
        {
            errors.Add($"{source.Name} has {source.AvailableBalance} available.");
        }

        if (Draft.Payee is { TransferLimit: { } limit } payee && Draft.Amount > limit)
        {
            errors.Add($"The limit for {payee.Name} is {limit}.");
        }

        ValidationErrors = errors;
        ValidationSummary = string.Join(MessageSeparator, errors);
        IsValid = errors.Count == 0;
    }
}
