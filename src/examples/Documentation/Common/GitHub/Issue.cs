// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>A reported problem or request. Each property raises <c>PropertyChanged</c>.</summary>
[System.Diagnostics.DebuggerDisplay("Issue: #{Number} {Title} ({State})")]
public sealed class Issue : ObservableObject
{
    /// <summary>Gets or sets the number the repository gave the issue.</summary>
    public int Number
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the short summary of the issue.</summary>
    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets whether the issue is open or closed.</summary>
    public IssueState State
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the person who reported the issue.</summary>
    public User Author
    {
        get;
        set => SetProperty(ref field, value);
    } = new();

    /// <summary>Gets or sets the person working on the issue, or <see langword="null"/> when nobody is.</summary>
    public User? Assignee
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the labels attached to the issue.</summary>
    public IReadOnlyList<string> Labels
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the comments on the issue, oldest first.</summary>
    public IReadOnlyList<IssueComment> Comments
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets when the issue last changed.</summary>
    public DateTimeOffset UpdatedAt
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Creates an independent copy, as the server returns a new object for each response.</summary>
    /// <returns>A copy with the same values.</returns>
    public Issue Clone() => new()
    {
        Number = Number,
        Title = Title,
        State = State,
        Author = Author.Clone(),
        Assignee = Assignee?.Clone(),
        Labels = Labels.ToList(),
        Comments = Comments.ToList(),
        UpdatedAt = UpdatedAt,
    };

    /// <summary>Copies the values of a newer response into this issue, so anything bound to this issue sees the change.</summary>
    /// <param name="newer">The response to copy from.</param>
    public void UpdateFrom(Issue newer)
    {
        Title = newer.Title;
        State = newer.State;
        Assignee = newer.Assignee;
        Labels = newer.Labels;
        Comments = newer.Comments;
        UpdatedAt = newer.UpdatedAt;
    }
}
