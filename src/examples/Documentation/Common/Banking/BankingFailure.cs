// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>Why the core banking backend refused a request.</summary>
public enum BankingFailure
{
    /// <summary>The source account cannot cover the amount.</summary>
    InsufficientFunds = 0,

    /// <summary>The transfers of the day would pass the daily limit.</summary>
    DailyLimitExceeded = 1,

    /// <summary>The amount is above the limit the customer set for the payee.</summary>
    PayeeLimitExceeded = 2,

    /// <summary>The bank holds the transfer for a fraud review.</summary>
    FraudHold = 3,

    /// <summary>The transfer needs a one-time approval code.</summary>
    ApprovalRequired = 4,

    /// <summary>The approval code is wrong.</summary>
    InvalidApprovalCode = 5,

    /// <summary>The source account does not exist.</summary>
    UnknownAccount = 6,

    /// <summary>The payee does not exist.</summary>
    UnknownPayee = 7,

    /// <summary>The amount is zero or negative.</summary>
    InvalidAmount = 8,

    /// <summary>The backend is down.</summary>
    ServiceUnavailable = 9,
}
