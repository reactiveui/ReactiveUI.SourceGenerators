// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ToPropertyVerification.Common;

/// <summary>
/// A protected method can only be called from inside the type or a subclass, so the generator adds a member to a
/// <see langword="partial"/> declaration that calls it on the type's behalf. Backs <see cref="DeferredFlag"/> with
/// deferred subscription, and <see cref="ScheduledLabel"/> with an explicit scheduler.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ProtectedBaseViewModel: DeferredFlag = {DeferredFlag}, ScheduledLabel = {ScheduledLabel}")]
public sealed partial class ProtectedBaseViewModel : ProtectedRaiseBase
{
    /// <summary>Backs <see cref="DeferredFlag"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _deferredFlagHelper;

    /// <summary>Backs <see cref="ScheduledLabel"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _scheduledLabelHelper;

    /// <summary>Initializes a new instance of the <see cref="ProtectedBaseViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    /// <param name="scheduler">The sequencer <see cref="ScheduledLabel"/>'s change notifications are raised on.</param>
    public ProtectedBaseViewModel(SourceItem item, ISequencer scheduler)
    {
        _deferredFlagHelper = item.WhenChanged(static x => x.Count).Select(static c => c > 0).ToProperty(this, static x => x.DeferredFlag, deferSubscription: true);
        _scheduledLabelHelper = item.WhenChanged(static x => x.Label).ToProperty(this, static x => x.ScheduledLabel, scheduler);
    }

    /// <summary>Gets a value indicating whether the followed item's count is positive.</summary>
    /// <remarks>Subscription is deferred, so nothing follows the item until this property is first read.</remarks>
    public bool DeferredFlag => _deferredFlagHelper.Value;

    /// <summary>Gets the followed item's label. Change notifications are raised on the given scheduler.</summary>
    public string ScheduledLabel => _scheduledLabelHelper.Value;
}
