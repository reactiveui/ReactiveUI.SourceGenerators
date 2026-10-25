// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>Reports that the to-do database could not complete a request.</summary>
public sealed class TodoStoreException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="TodoStoreException"/> class.</summary>
    public TodoStoreException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TodoStoreException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public TodoStoreException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TodoStoreException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public TodoStoreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
