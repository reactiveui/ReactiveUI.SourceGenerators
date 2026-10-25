// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>Reports that the student records service refused a request.</summary>
[System.Diagnostics.DebuggerDisplay("StudentRecordsException: Failure = {Failure}, Message = {Message}")]
public sealed class StudentRecordsException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="StudentRecordsException"/> class.</summary>
    public StudentRecordsException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StudentRecordsException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public StudentRecordsException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StudentRecordsException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public StudentRecordsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StudentRecordsException"/> class.</summary>
    /// <param name="failure">Why the service refused the request.</param>
    /// <param name="message">What went wrong.</param>
    public StudentRecordsException(RecordsFailure failure, string message)
        : base(message) => Failure = failure;

    /// <summary>Gets why the service refused the request.</summary>
    public RecordsFailure Failure { get; }
}
