// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ToPropertyVerification.Common;

/// <summary>
/// Declares its read-only properties with <see cref="ObservableAsPropertyAttribute"/> instead of writing the body
/// and the backing field by hand. Each property has to be <see langword="partial"/> and get-only; the source
/// generator writes the body and a field named <c>_{name}Helper</c>, which the constructor assigns with
/// <c>ToProperty</c>. <see cref="AttrCount"/>/<see cref="AttrLabel"/> (rather than plain <c>Count</c>/<c>Label</c>)
/// avoid reusing the exact selector text SourceItem's own <c>WhenChanged</c> call sites use elsewhere in this
/// project; see the remark on <c>ReactiveObjectViewModel</c> for why.
/// </summary>
[System.Diagnostics.DebuggerDisplay("AttributeViewModel: AttrCount = {AttrCount}, AttrLabel = {AttrLabel}")]
public sealed partial class AttributeViewModel : INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="AttributeViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    public AttributeViewModel(SourceItem item)
    {
        _attrCountHelper = item.WhenChanged(static x => x.Count).ToProperty(this, static x => x.AttrCount);
        _attrLabelHelper = item.WhenChanged(static x => x.Label).ToProperty(this, static x => x.AttrLabel);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the followed item's count.</summary>
    [ObservableAsProperty]
    public partial int AttrCount { get; }

    /// <summary>Gets the followed item's label.</summary>
    [ObservableAsProperty]
    public partial string AttrLabel { get; }
}
