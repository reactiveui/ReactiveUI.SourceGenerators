// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>
/// The view model behind the issue board. It signs in, lists repositories and their open issues, and closes or
/// comments on the selected issue. Each command starts the matching <c>Async</c> method and returns; await the
/// method to wait for the work. A failed request never throws from those methods. It sets
/// <see cref="ErrorMessage"/>, and a 401 signs the user out.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IssueBoardViewModel: IsSignedIn = {IsSignedIn}, Issues = {Issues.Count}")]
public sealed class IssueBoardViewModel : ObservableObject
{
    /// <summary>The server the view model calls.</summary>
    private readonly IGitHubApi _api;

    /// <summary>Initializes a new instance of the <see cref="IssueBoardViewModel"/> class.</summary>
    /// <param name="api">The server to call.</param>
    public IssueBoardViewModel(IGitHubApi api)
    {
        _api = api;
        RateLimitRemaining = api.RateLimitRemaining;
        SignInCommand = new(() => _ = SignInAsync(), () => !IsSignedIn && !string.IsNullOrWhiteSpace(Token));
        SignOutCommand = new(SignOut, () => IsSignedIn);
        LoadIssuesCommand = new(() => _ = LoadIssuesAsync(), () => IsSignedIn && SelectedRepository is not null);
        CloseIssueCommand = new(() => _ = CloseIssueAsync(), () => SelectedIssue is { State: IssueState.Open });
        AddCommentCommand = new(() => _ = AddCommentAsync(), () => SelectedIssue is not null && !string.IsNullOrWhiteSpace(NewCommentText));
    }

    /// <summary>Gets the question the view answers before an issue closes. The input is the issue; the answer is <see langword="true"/> to close it.</summary>
    public Interaction<Issue, bool> ConfirmClose { get; } = new();

    /// <summary>Gets the command that runs <see cref="SignInAsync"/>.</summary>
    public Command SignInCommand { get; }

    /// <summary>Gets the command that runs <see cref="SignOut"/>.</summary>
    public Command SignOutCommand { get; }

    /// <summary>Gets the command that runs <see cref="LoadIssuesAsync"/>.</summary>
    public Command LoadIssuesCommand { get; }

    /// <summary>Gets the command that runs <see cref="CloseIssueAsync"/>.</summary>
    public Command CloseIssueCommand { get; }

    /// <summary>Gets the command that runs <see cref="AddCommentAsync"/>.</summary>
    public Command AddCommentCommand { get; }

    /// <summary>Gets or sets the access token the user typed.</summary>
    public string Token
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SignInCommand.ChangeCanExecute();
            }
        }
    } = string.Empty;

    /// <summary>Gets a value indicating whether the server accepted the token.</summary>
    public bool IsSignedIn
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            SignInCommand.ChangeCanExecute();
            SignOutCommand.ChangeCanExecute();
            LoadIssuesCommand.ChangeCanExecute();
        }
    }

    /// <summary>Gets the signed-in account, or <see langword="null"/> when nobody is signed in.</summary>
    public User? CurrentUser
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the repositories the account can see.</summary>
    public IReadOnlyList<Repository> Repositories
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the repository whose issues the board shows.</summary>
    public Repository? SelectedRepository
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                LoadIssuesCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>Gets the open issues of <see cref="SelectedRepository"/>.</summary>
    public IReadOnlyList<Issue> Issues
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the issue the user picked. A path such as <c>SelectedIssue.Assignee.Login</c> is empty while nothing is picked or the issue has no assignee.</summary>
    public Issue? SelectedIssue
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            CloseIssueCommand.ChangeCanExecute();
            AddCommentCommand.ChangeCanExecute();
        }
    }

    /// <summary>Gets or sets the text of the comment the user is writing.</summary>
    public string NewCommentText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                AddCommentCommand.ChangeCanExecute();
            }
        }
    } = string.Empty;

    /// <summary>Gets how many requests the account may still make before the quota renews.</summary>
    public int RateLimitRemaining
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last failed request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Signs in with <see cref="Token"/> and loads the repositories.</summary>
    /// <returns>A task that completes when the repositories are loaded or the request failed.</returns>
    public async Task SignInAsync()
    {
        try
        {
            User user = await _api.SignInAsync(Token).ConfigureAwait(false);
            CurrentUser = user;
            IsSignedIn = true;
            ErrorMessage = string.Empty;
            Repositories = await _api.ListRepositoriesAsync().ConfigureAwait(false);
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Signs out and clears everything the account could see.</summary>
    public void SignOut()
    {
        _api.SignOut();
        Reset();
    }

    /// <summary>Loads the open issues of <see cref="SelectedRepository"/>.</summary>
    /// <returns>A task that completes when the issues are loaded or the request failed.</returns>
    public async Task LoadIssuesAsync()
    {
        if (SelectedRepository is not { } repository)
        {
            return;
        }

        try
        {
            IReadOnlyList<Issue> issues = await _api.ListIssuesAsync(repository.FullName, IssueState.Open).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            Issues = issues;
            SelectedIssue = null;
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Asks <see cref="ConfirmClose"/> to confirm, then closes <see cref="SelectedIssue"/>.</summary>
    /// <returns>A task that completes when the issue is closed, the user declined or the request failed.</returns>
    public async Task CloseIssueAsync()
    {
        if (SelectedIssue is not { } issue || SelectedRepository is not { } repository)
        {
            return;
        }

        if (!await ConfirmClose.Handle(issue).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            Issue closed = await _api.CloseIssueAsync(repository.FullName, issue.Number).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            issue.UpdateFrom(closed);
            CloseIssueCommand.ChangeCanExecute();
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Posts <see cref="NewCommentText"/> on <see cref="SelectedIssue"/>.</summary>
    /// <returns>A task that completes when the comment is posted or the request failed.</returns>
    public async Task AddCommentAsync()
    {
        if (SelectedIssue is not { } issue || SelectedRepository is not { } repository)
        {
            return;
        }

        try
        {
            Issue updated = await _api.AddCommentAsync(repository.FullName, issue.Number, NewCommentText).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            issue.UpdateFrom(updated);
            NewCommentText = string.Empty;
        }
        catch (GitHubApiException ex)
        {
            Fail(ex);
        }
        finally
        {
            RateLimitRemaining = _api.RateLimitRemaining;
        }
    }

    /// <summary>Reports a failed request, and signs out when the server no longer accepts the token.</summary>
    /// <param name="ex">The failure.</param>
    private void Fail(GitHubApiException ex)
    {
        ErrorMessage = ex.RateLimitResetsAt is { } resetsAt
            ? $"{ex.Message} Try again after {resetsAt.ToString("HH:mm", CultureInfo.InvariantCulture)} UTC."
            : ex.Message;

        if (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            Reset();
        }
    }

    /// <summary>Clears the account and everything loaded for it.</summary>
    private void Reset()
    {
        IsSignedIn = false;
        CurrentUser = null;
        Repositories = [];
        SelectedRepository = null;
        Issues = [];
        SelectedIssue = null;
    }
}
