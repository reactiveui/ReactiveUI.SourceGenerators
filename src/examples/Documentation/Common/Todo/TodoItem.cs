// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>One task on the to-do list. Each property raises <c>PropertyChanged</c>.</summary>
[System.Diagnostics.DebuggerDisplay("TodoItem: {Id}: {Title}, IsDone = {IsDone}")]
public sealed class TodoItem : ObservableObject
{
    /// <summary>Gets or sets the identifier the store assigned; zero until the item is stored.</summary>
    public int Id
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the short summary of the task.</summary>
    public string Title
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the longer description of the task.</summary>
    public string Notes
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the task is finished.</summary>
    public bool IsDone
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the day the task is due, or <see langword="null"/> when it has no deadline.</summary>
    public DateOnly? DueDate
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets how urgent the task is.</summary>
    public TodoPriority Priority
    {
        get;
        set => SetProperty(ref field, value);
    } = TodoPriority.Normal;

    /// <summary>Gets or sets the labels attached to the task.</summary>
    public IReadOnlyList<string> Tags
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>Creates an independent copy, as a database returns a new row each time it is read.</summary>
    /// <returns>A copy with the same values.</returns>
    public TodoItem Clone() => new() { Id = Id, Title = Title, Notes = Notes, IsDone = IsDone, DueDate = DueDate, Priority = Priority, Tags = Tags.ToList() };

    /// <summary>Checks whether the title or notes contain a text, ignoring case.</summary>
    /// <param name="text">The text to look for; blank matches every item.</param>
    /// <returns><see langword="true"/> when the item matches.</returns>
    public bool Matches(string text) =>
        string.IsNullOrWhiteSpace(text)
        || Title.Contains(text, StringComparison.OrdinalIgnoreCase)
        || Notes.Contains(text, StringComparison.OrdinalIgnoreCase);
}
