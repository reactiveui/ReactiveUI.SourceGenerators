// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ToPropertyVerification.Common;

/// <summary>
/// Exercises three combined overload families in one view model: deferred subscription together with a scheduler,
/// the <see langword="out"/> parameter together with a plain initial value, and the <see langword="out"/> parameter
/// together with an initial-value factory.
/// </summary>
[System.Diagnostics.DebuggerDisplay("CombinedOverloadsViewModel: DeferredScheduled = {DeferredScheduled}, OutWithInitial = {OutWithInitial}, OutWithFactory = {OutWithFactory}")]
public sealed partial class CombinedOverloadsViewModel : INotifyPropertyChanged
{
    /// <summary>The plain initial value <see cref="OutWithInitial"/> starts at.</summary>
    private const string InitialSeed = "seed";

    /// <summary>The initial value <see cref="OutWithFactory"/>'s factory returns.</summary>
    private const string FactorySeed = "factory-seed";

    /// <summary>The suffix <see cref="OutWithFactory"/> appends to each label.</summary>
    private const string FactorySuffix = "!";

    /// <summary>Backs <see cref="DeferredScheduled"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _deferredScheduledHelper;

    /// <summary>Backs <see cref="OutWithInitial"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _outWithInitialHelper;

    /// <summary>Backs <see cref="OutWithFactory"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _outWithFactoryHelper;

    /// <summary>Initializes a new instance of the <see cref="CombinedOverloadsViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    /// <param name="labels">A source that has produced nothing yet, so the two labels start at their initial values.</param>
    /// <param name="scheduler">The sequencer <see cref="DeferredScheduled"/>'s change notifications are raised on.</param>
    public CombinedOverloadsViewModel(SourceItem item, IObservable<string> labels, ISequencer scheduler)
    {
        _deferredScheduledHelper = item.WhenChanged(static x => x.Count)
            .ToProperty(this, static x => x.DeferredScheduled, deferSubscription: true, scheduler: scheduler);

        _ = labels.ToProperty(this, static x => x.OutWithInitial, out _outWithInitialHelper, InitialSeed);
        _ = labels
            .Select(static l => l + FactorySuffix)
            .ToProperty(this, static x => x.OutWithFactory, out _outWithFactoryHelper, static () => FactorySeed);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the followed item's count. Subscription is deferred and delivery runs on a scheduler.</summary>
    public int DeferredScheduled => _deferredScheduledHelper.Value;

    /// <summary>Gets the latest label. Backed with an <see langword="out"/> helper and a plain initial value.</summary>
    public string OutWithInitial => _outWithInitialHelper.Value;

    /// <summary>Gets the latest label with a suffix. Backed with an <see langword="out"/> helper and an initial-value factory.</summary>
    public string OutWithFactory => _outWithFactoryHelper.Value;
}
