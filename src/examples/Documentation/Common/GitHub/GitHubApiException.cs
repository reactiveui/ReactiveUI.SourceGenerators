// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>Reports that the server answered a request with an error status.</summary>
[System.Diagnostics.DebuggerDisplay("GitHubApiException: StatusCode = {StatusCode}, Message = {Message}")]
public sealed class GitHubApiException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="GitHubApiException"/> class.</summary>
    public GitHubApiException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GitHubApiException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public GitHubApiException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GitHubApiException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public GitHubApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GitHubApiException"/> class.</summary>
    /// <param name="statusCode">The status the server answered with.</param>
    /// <param name="message">What went wrong.</param>
    /// <param name="rateLimitResetsAt">When the request quota renews; <see langword="null"/> unless the quota ran out.</param>
    public GitHubApiException(HttpStatusCode statusCode, string message, DateTimeOffset? rateLimitResetsAt)
        : base(message)
    {
        StatusCode = statusCode;
        RateLimitResetsAt = rateLimitResetsAt;
    }

    /// <summary>Gets the status the server answered with.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Gets when the request quota renews, or <see langword="null"/> when the quota did not run out.</summary>
    public DateTimeOffset? RateLimitResetsAt { get; }
}
