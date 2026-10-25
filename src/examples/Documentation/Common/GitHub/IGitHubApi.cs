// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>The part of a GitHub-style issue tracker server that the example app calls.</summary>
public interface IGitHubApi
{
    /// <summary>Gets how many requests the signed-in account may still make before the quota renews.</summary>
    int RateLimitRemaining { get; }

    /// <summary>Signs in with an access token.</summary>
    /// <param name="token">The access token.</param>
    /// <returns>The account the token belongs to.</returns>
    /// <exception cref="GitHubApiException">The token is not valid (401) or the request quota is used up (403).</exception>
    Task<User> SignInAsync(string token);

    /// <summary>Forgets the access token. The next request fails with 401 until the caller signs in again.</summary>
    void SignOut();

    /// <summary>Lists the repositories the signed-in account can see.</summary>
    /// <returns>The repositories.</returns>
    /// <exception cref="GitHubApiException">The caller is not signed in (401) or the request quota is used up (403).</exception>
    Task<IReadOnlyList<Repository>> ListRepositoriesAsync();

    /// <summary>Lists the issues of a repository.</summary>
    /// <param name="repository">The repository name, such as <c>acme/webshop</c>.</param>
    /// <param name="state">Which issues to list.</param>
    /// <returns>The issues, newest first.</returns>
    /// <exception cref="GitHubApiException">The caller is not signed in (401), the quota is used up (403) or the repository does not exist (404).</exception>
    Task<IReadOnlyList<Issue>> ListIssuesAsync(string repository, IssueState state);

    /// <summary>Opens a new issue.</summary>
    /// <param name="repository">The repository name, such as <c>acme/webshop</c>.</param>
    /// <param name="title">The title of the issue.</param>
    /// <param name="assigneeLogin">The account to assign, or <see langword="null"/> to leave the issue unassigned.</param>
    /// <returns>The new issue.</returns>
    /// <exception cref="GitHubApiException">The caller is not signed in (401), the quota is used up (403), the repository or assignee does not exist (404) or the title is blank (422).</exception>
    Task<Issue> CreateIssueAsync(string repository, string title, string? assigneeLogin);

    /// <summary>Closes an issue.</summary>
    /// <param name="repository">The repository name, such as <c>acme/webshop</c>.</param>
    /// <param name="number">The number of the issue.</param>
    /// <returns>The issue after it closed.</returns>
    /// <exception cref="GitHubApiException">The caller is not signed in (401), the quota is used up (403), the issue does not exist (404) or it is already closed (422).</exception>
    Task<Issue> CloseIssueAsync(string repository, int number);

    /// <summary>Adds a comment to an issue.</summary>
    /// <param name="repository">The repository name, such as <c>acme/webshop</c>.</param>
    /// <param name="number">The number of the issue.</param>
    /// <param name="body">The text of the comment.</param>
    /// <returns>The issue after the comment was added.</returns>
    /// <exception cref="GitHubApiException">The caller is not signed in (401), the quota is used up (403), the issue does not exist (404) or the body is blank (422).</exception>
    Task<Issue> AddCommentAsync(string repository, int number, string body);
}
