// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ToPropertyVerification.Common;

/// <summary>
/// A view model that declares its own field-like <see cref="PropertyChanged"/> event. Only code inside the type
/// can invoke that event, so the generator adds a member of its own; that only works while the type stays
/// <see langword="partial"/>. Backs <see cref="DoubledCount"/> with the selector overload (no initial value) and
/// <see cref="LabelText"/> with the selector overload that returns its helper through an <see langword="out"/>
/// parameter.
/// </summary>
[System.Diagnostics.DebuggerDisplay("FieldEventViewModel: DoubledCount = {DoubledCount}, LabelText = {LabelText}")]
public sealed partial class FieldEventViewModel : INotifyPropertyChanged
{
    /// <summary>The factor <see cref="DoubledCount"/> multiplies the followed item's count by.</summary>
    private const int DoublingFactor = 2;

    /// <summary>Backs <see cref="DoubledCount"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _doubledCountHelper;

    /// <summary>Backs <see cref="LabelText"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _labelTextHelper;

    /// <summary>Initializes a new instance of the <see cref="FieldEventViewModel"/> class.</summary>
    /// <param name="item">The item the view model follows.</param>
    public FieldEventViewModel(SourceItem item)
    {
        _doubledCountHelper = item.WhenChanged(static x => x.Count).Select(static c => c * DoublingFactor).ToProperty(this, static x => x.DoubledCount);
        _ = item.WhenChanged(static x => x.Label).ToProperty(this, static x => x.LabelText, out _labelTextHelper);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets twice the followed item's count.</summary>
    public int DoubledCount => _doubledCountHelper.Value;

    /// <summary>Gets the followed item's label.</summary>
    public string LabelText => _labelTextHelper.Value;
}
