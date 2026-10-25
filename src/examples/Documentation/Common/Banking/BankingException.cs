// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>Reports that the core banking backend refused a request.</summary>
[System.Diagnostics.DebuggerDisplay("BankingException: Failure = {Failure}, Message = {Message}")]
public sealed class BankingException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="BankingException"/> class.</summary>
    public BankingException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BankingException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public BankingException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BankingException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public BankingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BankingException"/> class.</summary>
    /// <param name="failure">Why the backend refused the request.</param>
    /// <param name="message">What went wrong.</param>
    public BankingException(BankingFailure failure, string message)
        : base(message) => Failure = failure;

    /// <summary>Gets why the backend refused the request.</summary>
    public BankingFailure Failure { get; }
}
