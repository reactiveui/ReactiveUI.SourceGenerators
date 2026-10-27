// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// The live event streams of one repository, one stream per kind of activity. The server pushes an event onto the
/// stream that matches it, and every subscriber of that stream hears it.
/// </summary>
[System.Diagnostics.DebuggerDisplay("RepositoryChannels: {Repository}")]
public sealed class RepositoryChannels : IDisposable
{
    /// <summary>Every stream the repository owns.</summary>
    private readonly Signal<string>[] _streams;

    /// <summary>Initializes a new instance of the <see cref="RepositoryChannels"/> class.</summary>
    /// <param name="repository">The repository the streams belong to.</param>
    public RepositoryChannels(Repository repository)
    {
        Repository = repository;
        _streams =
        [
            IssueOpened,
            IssueClosed,
            IssueReopened,
            IssueAssigned,
            IssueLabelled,
            CommentAdded,
            CommentEdited,
            MilestoneChanged,
            PullRequestOpened,
            PullRequestMerged,
            ReviewRequested,
            ReleasePublished,
        ];
    }

    /// <summary>Gets the repository the streams belong to.</summary>
    public Repository Repository { get; }

    /// <summary>Gets the stream of newly opened issues.</summary>
    public Signal<string> IssueOpened { get; } = new();

    /// <summary>Gets the stream of closed issues.</summary>
    public Signal<string> IssueClosed { get; } = new();

    /// <summary>Gets the stream of reopened issues.</summary>
    public Signal<string> IssueReopened { get; } = new();

    /// <summary>Gets the stream of issue assignments.</summary>
    public Signal<string> IssueAssigned { get; } = new();

    /// <summary>Gets the stream of issue labels.</summary>
    public Signal<string> IssueLabelled { get; } = new();

    /// <summary>Gets the stream of new comments.</summary>
    public Signal<string> CommentAdded { get; } = new();

    /// <summary>Gets the stream of edited comments.</summary>
    public Signal<string> CommentEdited { get; } = new();

    /// <summary>Gets the stream of milestone changes.</summary>
    public Signal<string> MilestoneChanged { get; } = new();

    /// <summary>Gets the stream of newly opened pull requests.</summary>
    public Signal<string> PullRequestOpened { get; } = new();

    /// <summary>Gets the stream of merged pull requests.</summary>
    public Signal<string> PullRequestMerged { get; } = new();

    /// <summary>Gets the stream of review requests.</summary>
    public Signal<string> ReviewRequested { get; } = new();

    /// <summary>Gets the stream of published releases.</summary>
    public Signal<string> ReleasePublished { get; } = new();

    /// <summary>Gets how many streams currently have a subscriber.</summary>
    public int SubscribedStreamCount => _streams.Count(static stream => stream.HasObservers);

    /// <summary>Publishes one announcement, such as a maintenance window, on every stream.</summary>
    /// <param name="text">The announcement.</param>
    public void PublishNotice(string text)
    {
        for (var i = 0; i < _streams.Length; i++)
        {
            _streams[i].OnNext(text);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        for (var i = 0; i < _streams.Length; i++)
        {
            _streams[i].Dispose();
        }
    }
}
