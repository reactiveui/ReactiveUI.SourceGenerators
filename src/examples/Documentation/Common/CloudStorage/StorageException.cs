// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.CloudStorage;

/// <summary>Reports that the storage service refused a request.</summary>
[System.Diagnostics.DebuggerDisplay("StorageException: Failure = {Failure}, Message = {Message}")]
public sealed class StorageException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="StorageException"/> class.</summary>
    public StorageException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StorageException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public StorageException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StorageException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StorageException"/> class.</summary>
    /// <param name="failure">Why the service refused the request.</param>
    /// <param name="message">What went wrong.</param>
    public StorageException(StorageFailure failure, string message)
        : base(message) => Failure = failure;

    /// <summary>Gets why the service refused the request.</summary>
    public StorageFailure Failure { get; }
}
