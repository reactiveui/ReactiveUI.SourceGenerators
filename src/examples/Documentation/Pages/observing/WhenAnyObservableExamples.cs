// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// Shows every <c>WhenAnyObservable</c> overload. The properties you name hold observables, and the feed swaps them at
/// run time. <c>WhenAnyObservable</c> follows whichever observable each property holds now and drops the one it held
/// before. With one to twelve properties and no selector it merges the streams into one. With two to twelve
/// properties and a selector it delivers the selector's result whenever any stream emits, once every stream has
/// emitted at least once.
/// </summary>
public static class WhenAnyObservableExamples
{
    /// <summary>The name of the repository the examples follow first.</summary>
    private const string WebshopName = "webshop";

    /// <summary>The name of the repository the examples switch to.</summary>
    private const string MobileAppName = "mobile-app";

    /// <summary>The number of open issues the repository reports.</summary>
    private const int OpenIssueCount = 12;

    /// <summary>The announcement published on every stream before a selector has all its inputs.</summary>
    private const string MaintenanceNotice = "Maintenance starts at 22:00 UTC";

    /// <summary>An event that arrives on a stream the feed no longer follows.</summary>
    private const string StaleEvent = "#55 opened: Reported before the switch";

    /// <summary>An event that arrives on the repository the feed switched to.</summary>
    private const string MobileAppEvent = "#7 opened: App crashes on start";

    /// <summary>The event that arrives on the issue opened stream.</summary>
    private const string IssueOpenedEvent = "#101 opened: Checkout button unresponsive";

    /// <summary>The event that arrives on the issue closed stream.</summary>
    private const string IssueClosedEvent = "#98 closed: Cart total rounds down";

    /// <summary>The event that arrives on the issue reopened stream.</summary>
    private const string IssueReopenedEvent = "#97 reopened: Login loops on Safari";

    /// <summary>The event that arrives on the issue assigned stream.</summary>
    private const string IssueAssignedEvent = "#101 assigned to priya-nair";

    /// <summary>The event that arrives on the issue labelled stream.</summary>
    private const string IssueLabelledEvent = "#101 labelled bug";

    /// <summary>The event that arrives on the comment added stream.</summary>
    private const string CommentAddedEvent = "#101 comment: Fixed in the next release.";

    /// <summary>The event that arrives on the comment edited stream.</summary>
    private const string CommentEditedEvent = "#101 comment edited";

    /// <summary>The event that arrives on the milestone changed stream.</summary>
    private const string MilestoneChangedEvent = "#101 milestone: 2.4";

    /// <summary>The event that arrives on the pull request opened stream.</summary>
    private const string PullRequestOpenedEvent = "PR #205 opened";

    /// <summary>The event that arrives on the pull request merged stream.</summary>
    private const string PullRequestMergedEvent = "PR #205 merged";

    /// <summary>The event that arrives on the review requested stream.</summary>
    private const string ReviewRequestedEvent = "PR #205 review requested from tomas-berg";

    /// <summary>The event that arrives on the release published stream.</summary>
    private const string ReleasePublishedEvent = "Release 2.4 published";

    /// <summary>The repository the feed follows first.</summary>
    private static readonly Repository _webshop = new("acme", WebshopName, "The online shop", OpenIssueCount);

    /// <summary>The repository the feed switches to.</summary>
    private static readonly Repository _mobileApp = new("acme", MobileAppName, "The mobile app", 1);

    /// <summary>Follows one stream. A single stream delivers exactly what its property holds.</summary>
    public static void FollowIssueOpened()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed.WhenAnyObservable(x => x.IssueOpened).Subscribe(Console.WriteLine);

        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Selects another repository: the stream of the first one is dropped and the new one is followed.</summary>
    public static void SwitchStreamWhenRepositoryChanges()
    {
        using RepositoryChannels webshop = new(_webshop);
        using RepositoryChannels mobileApp = new(_mobileApp);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed.WhenAnyObservable(x => x.IssueOpened).Subscribe(Console.WriteLine);

        Console.WriteLine(webshop.IssueOpened.HasObservers);

        feed.Follow(mobileApp);

        Console.WriteLine(webshop.IssueOpened.HasObservers);
        Console.WriteLine(mobileApp.IssueOpened.HasObservers);

        webshop.IssueOpened.OnNext(StaleEvent);
        mobileApp.IssueOpened.OnNext(MobileAppEvent);

        // Output:
        // True
        // False
        // True
        // #7 opened: App crashes on start
    }

    /// <summary>Stops following: a property that holds no observable delivers nothing and keeps no subscription.</summary>
    public static void StopWhenNoRepositoryIsFollowed()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed.WhenAnyObservable(x => x.IssueOpened).Subscribe(Console.WriteLine);

        feed.Unfollow();

        Console.WriteLine(webshop.IssueOpened.HasObservers);

        webshop.IssueOpened.OnNext(StaleEvent);

        feed.Follow(webshop);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // False
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Follows only the streams the call names: the other streams of the repository keep no subscriber.</summary>
    public static void SubscribeOnlyToNamedStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed.WhenAnyObservable(x => x.IssueOpened, x => x.ReleasePublished).Subscribe(Console.WriteLine);

        Console.WriteLine(webshop.SubscribedStreamCount);

        // Output:
        // 2
    }

    /// <summary>Merges the opened and closed issue streams of the followed repository into one feed.</summary>
    public static void MergeTwoStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(x => x.IssueOpened, x => x.IssueClosed)
            .Subscribe(Console.WriteLine);

        webshop.IssueClosed.OnNext(IssueClosedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #98 closed: Cart total rounds down
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges the issue lifecycle streams: opened, closed and reopened.</summary>
    public static void MergeThreeStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(x => x.IssueOpened, x => x.IssueClosed, x => x.IssueReopened)
            .Subscribe(Console.WriteLine);

        webshop.IssueReopened.OnNext(IssueReopenedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #97 reopened: Login loops on Safari
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges four streams and adds the assignments a triage lead watches.</summary>
    public static void MergeFourStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned)
            .Subscribe(Console.WriteLine);

        webshop.IssueAssigned.OnNext(IssueAssignedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 assigned to priya-nair
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges five streams and adds the labels.</summary>
    public static void MergeFiveStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled)
            .Subscribe(Console.WriteLine);

        webshop.IssueLabelled.OnNext(IssueLabelledEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 labelled bug
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges six streams and adds new comments.</summary>
    public static void MergeSixStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded)
            .Subscribe(Console.WriteLine);

        webshop.CommentAdded.OnNext(CommentAddedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 comment: Fixed in the next release.
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges seven streams and adds edited comments.</summary>
    public static void MergeSevenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited)
            .Subscribe(Console.WriteLine);

        webshop.CommentEdited.OnNext(CommentEditedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 comment edited
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges eight streams and adds milestone changes.</summary>
    public static void MergeEightStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged)
            .Subscribe(Console.WriteLine);

        webshop.MilestoneChanged.OnNext(MilestoneChangedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // #101 milestone: 2.4
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges nine streams and adds pull requests as they open.</summary>
    public static void MergeNineStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened)
            .Subscribe(Console.WriteLine);

        webshop.PullRequestOpened.OnNext(PullRequestOpenedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // PR #205 opened
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges ten streams and adds merged pull requests.</summary>
    public static void MergeTenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged)
            .Subscribe(Console.WriteLine);

        webshop.PullRequestMerged.OnNext(PullRequestMergedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // PR #205 merged
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges eleven streams and adds review requests.</summary>
    public static void MergeElevenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged,
                x => x.ReviewRequested)
            .Subscribe(Console.WriteLine);

        webshop.ReviewRequested.OnNext(ReviewRequestedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // PR #205 review requested from tomas-berg
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Merges twelve streams, the most one call accepts, and adds releases.</summary>
    public static void MergeTwelveStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged,
                x => x.ReviewRequested,
                x => x.ReleasePublished)
            .Subscribe(Console.WriteLine);

        webshop.ReleasePublished.OnNext(ReleasePublishedEvent);
        webshop.IssueOpened.OnNext(IssueOpenedEvent);

        // Output:
        // Release 2.4 published
        // #101 opened: Checkout button unresponsive
    }

    /// <summary>Combines the first and last of two streams with a selector, once each stream has emitted.</summary>
    public static void CombineTwoStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(x => x.IssueOpened, x => x.IssueClosed, static (first, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.IssueClosed.OnNext(IssueClosedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #98 closed: Cart total rounds down
    }

    /// <summary>Combines the first and last of three streams with a selector, once each stream has emitted.</summary>
    public static void CombineThreeStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(x => x.IssueOpened, x => x.IssueClosed, x => x.IssueReopened, static (first, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.IssueReopened.OnNext(IssueReopenedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #97 reopened: Login loops on Safari
    }

    /// <summary>Combines the first and last of four streams with a selector, once each stream has emitted.</summary>
    public static void CombineFourStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                static (first, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.IssueAssigned.OnNext(IssueAssignedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #101 assigned to priya-nair
    }

    /// <summary>Combines the first and last of five streams with a selector, once each stream has emitted.</summary>
    public static void CombineFiveStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                static (first, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.IssueLabelled.OnNext(IssueLabelledEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #101 labelled bug
    }

    /// <summary>Combines the first and last of six streams with a selector, once each stream has emitted.</summary>
    public static void CombineSixStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                static (first, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.CommentAdded.OnNext(CommentAddedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #101 comment: Fixed in the next release.
    }

    /// <summary>Combines the first and last of seven streams with a selector, once each stream has emitted.</summary>
    public static void CombineSevenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                static (first, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.CommentEdited.OnNext(CommentEditedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #101 comment edited
    }

    /// <summary>Combines the first and last of eight streams with a selector, once each stream has emitted.</summary>
    public static void CombineEightStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                static (first, _, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.MilestoneChanged.OnNext(MilestoneChangedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / #101 milestone: 2.4
    }

    /// <summary>Combines the first and last of nine streams with a selector, once each stream has emitted.</summary>
    public static void CombineNineStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                static (first, _, _, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.PullRequestOpened.OnNext(PullRequestOpenedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / PR #205 opened
    }

    /// <summary>Combines the first and last of ten streams with a selector, once each stream has emitted.</summary>
    public static void CombineTenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged,
                static (first, _, _, _, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.PullRequestMerged.OnNext(PullRequestMergedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / PR #205 merged
    }

    /// <summary>Combines the first and last of eleven streams with a selector, once each stream has emitted.</summary>
    public static void CombineElevenStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged,
                x => x.ReviewRequested,
                static (first, _, _, _, _, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.ReviewRequested.OnNext(ReviewRequestedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / PR #205 review requested from tomas-berg
    }

    /// <summary>Combines the first and last of twelve streams with a selector, once each stream has emitted.</summary>
    public static void CombineTwelveStreams()
    {
        using RepositoryChannels webshop = new(_webshop);
        RepositoryFeedViewModel feed = new();
        feed.Follow(webshop);

        using IDisposable subscription = feed
            .WhenAnyObservable(
                x => x.IssueOpened,
                x => x.IssueClosed,
                x => x.IssueReopened,
                x => x.IssueAssigned,
                x => x.IssueLabelled,
                x => x.CommentAdded,
                x => x.CommentEdited,
                x => x.MilestoneChanged,
                x => x.PullRequestOpened,
                x => x.PullRequestMerged,
                x => x.ReviewRequested,
                x => x.ReleasePublished,
                static (first, _, _, _, _, _, _, _, _, _, _, last) => $"{first} / {last}")
            .Subscribe(Console.WriteLine);

        webshop.PublishNotice(MaintenanceNotice);
        webshop.ReleasePublished.OnNext(ReleasePublishedEvent);

        // Output:
        // Maintenance starts at 22:00 UTC / Maintenance starts at 22:00 UTC
        // Maintenance starts at 22:00 UTC / Release 2.4 published
    }
}
