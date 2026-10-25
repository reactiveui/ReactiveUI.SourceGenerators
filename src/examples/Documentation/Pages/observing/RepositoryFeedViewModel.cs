// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// The activity feed of the selected repository. Each property holds the stream of one kind of activity, and every
/// property is replaced when the user selects another repository.
/// </summary>
[System.Diagnostics.DebuggerDisplay("RepositoryFeedViewModel")]
public sealed class RepositoryFeedViewModel : ObservableObject
{
    /// <summary>Gets the stream of newly opened issues, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? IssueOpened
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of closed issues, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? IssueClosed
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of reopened issues, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? IssueReopened
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of issue assignments, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? IssueAssigned
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of issue labels, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? IssueLabelled
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of new comments, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? CommentAdded
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of edited comments, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? CommentEdited
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of milestone changes, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? MilestoneChanged
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of newly opened pull requests, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? PullRequestOpened
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of merged pull requests, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? PullRequestMerged
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of review requests, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? ReviewRequested
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the stream of published releases, or <see langword="null"/> when no repository is followed.</summary>
    public IObservable<string>? ReleasePublished
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Follows a repository by replacing every stream with the repository's own.</summary>
    /// <param name="channels">The streams of the repository the user selected.</param>
    public void Follow(RepositoryChannels channels)
    {
        IssueOpened = channels.IssueOpened;
        IssueClosed = channels.IssueClosed;
        IssueReopened = channels.IssueReopened;
        IssueAssigned = channels.IssueAssigned;
        IssueLabelled = channels.IssueLabelled;
        CommentAdded = channels.CommentAdded;
        CommentEdited = channels.CommentEdited;
        MilestoneChanged = channels.MilestoneChanged;
        PullRequestOpened = channels.PullRequestOpened;
        PullRequestMerged = channels.PullRequestMerged;
        ReviewRequested = channels.ReviewRequested;
        ReleasePublished = channels.ReleasePublished;
    }

    /// <summary>Stops following the repository, so every stream is <see langword="null"/>.</summary>
    public void Unfollow()
    {
        IssueOpened = null;
        IssueClosed = null;
        IssueReopened = null;
        IssueAssigned = null;
        IssueLabelled = null;
        CommentAdded = null;
        CommentEdited = null;
        MilestoneChanged = null;
        PullRequestOpened = null;
        PullRequestMerged = null;
        ReviewRequested = null;
        ReleasePublished = null;
    }
}
