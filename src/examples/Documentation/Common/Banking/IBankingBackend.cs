// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>The part of a core banking backend that the example app calls. Every call is asynchronous, as a call to a real backend is.</summary>
public interface IBankingBackend
{
    /// <summary>Lists the accounts of the customer.</summary>
    /// <returns>Copies of the accounts, by identifier.</returns>
    /// <exception cref="BankingException">The backend is down.</exception>
    Task<IReadOnlyList<Account>> GetAccountsAsync();

    /// <summary>Lists the transactions of an account.</summary>
    /// <param name="accountId">The identifier of the account.</param>
    /// <returns>The transactions, newest first.</returns>
    /// <exception cref="BankingException">The account does not exist or the backend is down.</exception>
    Task<IReadOnlyList<Transaction>> GetTransactionsAsync(string accountId);

    /// <summary>Lists the payees the customer can send money to.</summary>
    /// <returns>The payees, by name.</returns>
    /// <exception cref="BankingException">The backend is down.</exception>
    Task<IReadOnlyList<Payee>> GetPayeesAsync();

    /// <summary>Checks a transfer against the rules of the bank without making it.</summary>
    /// <param name="request">The transfer to check.</param>
    /// <returns>The rules the transfer breaks.</returns>
    /// <exception cref="BankingException">The account or payee does not exist or the backend is down.</exception>
    Task<TransferValidation> ValidateTransferAsync(TransferRequest request);

    /// <summary>Makes a transfer.</summary>
    /// <param name="request">The transfer to make.</param>
    /// <returns>The receipt for the transfer.</returns>
    /// <exception cref="BankingException">The transfer breaks a rule, the bank holds it for a fraud review, it needs an approval code, or the backend is down.</exception>
    Task<TransferReceipt> SubmitTransferAsync(TransferRequest request);

    /// <summary>Makes a transfer that needs a one-time approval code.</summary>
    /// <param name="request">The transfer to make.</param>
    /// <param name="approvalCode">The code the customer received.</param>
    /// <returns>The receipt for the transfer.</returns>
    /// <exception cref="BankingException">The code is wrong, the transfer breaks a rule, the bank holds it for a fraud review, or the backend is down.</exception>
    Task<TransferReceipt> SubmitTransferAsync(TransferRequest request, string approvalCode);
}
