// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>
/// The issue board screen: a sign-in strip, a repository list, an issue list and a detail area for the selected
/// issue. The view creates its MAUI controls in code; a real page builds them from markup.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IssueBoardView: ViewModel = {ViewModel}")]
public sealed class IssueBoardView : ObservableObject, IViewFor<IssueBoardViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IssueBoardViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the entry where the user types the access token.</summary>
    public Entry TokenTextBox { get; } = new() { Placeholder = "Access token" };

    /// <summary>Gets the button that signs in.</summary>
    public Button SignInButton { get; } = new() { Text = "Sign in" };

    /// <summary>Gets the button that signs out.</summary>
    public Button SignOutButton { get; } = new() { Text = "Sign out" };

    /// <summary>Gets the label that shows who is signed in.</summary>
    public Label UserLabel { get; } = new();

    /// <summary>Gets the label that shows how many requests are left in the quota.</summary>
    public Label RateLimitLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public Label ErrorLabel { get; } = new();

    /// <summary>Gets the list of repositories. Its selected item is a <see cref="Repository"/>.</summary>
    public CollectionView RepositoryList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the button that loads the issues of the selected repository.</summary>
    public Button LoadIssuesButton { get; } = new() { Text = "Load issues" };

    /// <summary>Gets the list of open issues. Its selected item is an <see cref="Issue"/>.</summary>
    public CollectionView IssueList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the label that shows the title of the selected issue.</summary>
    public Label IssueTitleLabel { get; } = new();

    /// <summary>Gets the label that shows whether the selected issue is open or closed.</summary>
    public Label StateLabel { get; } = new();

    /// <summary>Gets the label that shows the account name of the selected issue's assignee.</summary>
    public Label AssigneeLabel { get; } = new();

    /// <summary>Gets the label that shows the display name of the selected issue's author.</summary>
    public Label AuthorLabel { get; } = new();

    /// <summary>Gets the button that closes the selected issue.</summary>
    public Button CloseIssueButton { get; } = new() { Text = "Close issue" };

    /// <summary>Gets the entry where the user writes a comment.</summary>
    public Entry CommentTextBox { get; } = new() { Placeholder = "Leave a comment" };

    /// <summary>Gets the button that posts the comment.</summary>
    public Button AddCommentButton { get; } = new() { Text = "Comment" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IssueBoardViewModel?)value;
    }
}
