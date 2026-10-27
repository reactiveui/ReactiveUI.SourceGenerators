// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// A view model that raises its notifications through the public <see cref="RaiseMethodBase.RaisePropertyChanged"/>
/// method it inherits. This backs <see cref="RemainingLabel"/> with the selector overload and an initial-value
/// factory, and <see cref="Priority"/> with the named-property overload and an initial value.
/// </summary>
[System.Diagnostics.DebuggerDisplay("RaiseMethodViewModel: RemainingLabel = {RemainingLabel}")]
public sealed class RaiseMethodViewModel : RaiseMethodBase
{
    /// <summary>The label shown before the item is loaded.</summary>
    public static readonly string LoadingLabel = "Loading…";

    /// <summary>Backs <see cref="RemainingLabel"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _remainingLabelHelper;

    /// <summary>Backs <see cref="Priority"/>.</summary>
    private readonly ObservableAsPropertyHelper<TodoPriority> _priorityHelper;

    /// <summary>Initializes a new instance of the <see cref="RaiseMethodViewModel"/> class.</summary>
    /// <param name="item">The item whose remaining-work label and priority the view model follows.</param>
    public RaiseMethodViewModel(TodoItem item)
    {
        _remainingLabelHelper = item.WhenChanged(static x => x.IsDone)
            .Select(static done => done ? "Done" : "Open")
            .ToProperty(this, static x => x.RemainingLabel, static () => LoadingLabel);
        _priorityHelper = item.WhenChanged(static x => x.Priority).ToProperty(this, nameof(Priority), TodoPriority.Normal);
    }

    /// <summary>Gets a short label describing whether the item is still open.</summary>
    public string RemainingLabel => _remainingLabelHelper.Value;

    /// <summary>Gets the item's priority, or <see cref="TodoPriority.Normal"/> before the first value arrives.</summary>
    public TodoPriority Priority => _priorityHelper.Value;
}
