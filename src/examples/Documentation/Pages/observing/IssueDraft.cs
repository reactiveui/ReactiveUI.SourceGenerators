// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>The edit form of an issue. It reports each change before and after it is applied.</summary>
[System.Diagnostics.DebuggerDisplay("IssueDraft: #{Number} {Title} ({State})")]
public sealed class IssueDraft : ChangingObject
{
    /// <summary>Gets or sets the number the repository gave the issue.</summary>
    public int Number
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the short summary of the issue.</summary>
    public string Title
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets whether the issue is open or closed.</summary>
    public IssueState State
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the person who reported the issue.</summary>
    public User Author
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = new();

    /// <summary>Gets or sets the person working on the issue, or <see langword="null"/> when nobody is.</summary>
    public User? Assignee
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }

    /// <summary>Gets or sets the labels attached to the issue.</summary>
    public IReadOnlyList<string> Labels
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = [];

    /// <summary>Gets or sets the comments on the issue, oldest first.</summary>
    public IReadOnlyList<IssueComment> Comments
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = [];

    /// <summary>Gets or sets when the issue last changed.</summary>
    public DateTimeOffset UpdatedAt
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    }
}
