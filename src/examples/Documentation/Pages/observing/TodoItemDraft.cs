// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>The edit form of a to-do item. It reports each change before and after it is applied.</summary>
[System.Diagnostics.DebuggerDisplay("TodoItemDraft: Title = {Title}, IsDone = {IsDone}")]
public sealed class TodoItemDraft : ChangingObject
{
    /// <summary>Gets or sets the short summary of the task.</summary>
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

    /// <summary>Gets or sets the longer description of the task.</summary>
    public string Notes
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

    /// <summary>Gets or sets a value indicating whether the task is finished.</summary>
    public bool IsDone
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

    /// <summary>Gets or sets the day the task is due, or <see langword="null"/> when it has no deadline.</summary>
    public DateOnly? DueDate
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

    /// <summary>Gets or sets how urgent the task is.</summary>
    public TodoPriority Priority
    {
        get;
        set
        {
            if (RaisePropertyChanging(field, value))
            {
                _ = SetProperty(ref field, value);
            }
        }
    } = TodoPriority.Normal;

    /// <summary>Gets or sets the labels attached to the task.</summary>
    public IReadOnlyList<string> Tags
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
}
