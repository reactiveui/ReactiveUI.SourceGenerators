// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// A view model that inherits <see cref="ObservableObject"/>, whose <c>RaisePropertyChanged</c> method is
/// <see langword="protected"/>. A protected method can only be called from inside the type or a subclass, so the
/// generator adds a small member to a <see langword="partial"/> declaration that calls it on the type's behalf. This
/// backs <see cref="IsDone"/> with deferred subscription, and <see cref="DueDateLabel"/> with a scheduler.
/// </summary>
[System.Diagnostics.DebuggerDisplay("PartialProtectedBaseViewModel: DueDateLabel = {DueDateLabel}")]
public sealed partial class PartialProtectedBaseViewModel : ObservableObject
{
    /// <summary>The label shown for an item with no due date.</summary>
    public static readonly string NoDueDateLabel = "No due date";

    /// <summary>Backs <see cref="IsDone"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isDoneHelper;

    /// <summary>Backs <see cref="DueDateLabel"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _dueDateLabelHelper;

    /// <summary>Initializes a new instance of the <see cref="PartialProtectedBaseViewModel"/> class.</summary>
    /// <param name="item">The item whose completion and due date the view model follows.</param>
    /// <param name="scheduler">The sequencer the due-date label's change notifications are raised on.</param>
    public PartialProtectedBaseViewModel(TodoItem item, ISequencer scheduler)
    {
        _isDoneHelper = item.WhenChanged(static x => x.IsDone).ToProperty(this, static x => x.IsDone, deferSubscription: true);
        _dueDateLabelHelper = item.WhenChanged(static x => x.DueDate)
            .Select(static due => due is { } d ? d.ToString("yyyy-MM-dd") : NoDueDateLabel)
            .ToProperty(this, static x => x.DueDateLabel, scheduler);
    }

    /// <summary>Gets a value indicating whether the followed item is finished.</summary>
    /// <remarks>Subscription is deferred, so nothing follows the item until this property is first read.</remarks>
    public bool IsDone => _isDoneHelper.Value;

    /// <summary>Gets the due date of the followed item, formatted for display.</summary>
    public string DueDateLabel => _dueDateLabelHelper.Value;
}
