// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Globalization;

namespace PlatformBindingsVerification.Wpf;

/// <summary>Backs <see cref="Text"/> with an <see cref="ObservableAsPropertyHelper{T}"/> built from a source that is pushed from a background thread.</summary>
[System.Diagnostics.DebuggerDisplay("WpfCounterViewModel: Text = {Text}")]
public sealed partial class WpfCounterViewModel : INotifyPropertyChanged
{
    /// <summary>Backs <see cref="Text"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _textHelper;

    /// <summary>Initializes a new instance of the <see cref="WpfCounterViewModel"/> class.</summary>
    /// <param name="counts">The counts to format and expose as <see cref="Text"/>. Pushed from a background thread in this example.</param>
    public WpfCounterViewModel(IObservable<int> counts) =>
        _textHelper = counts.Select(static c => c.ToString(CultureInfo.InvariantCulture)).ToProperty(this, static x => x.Text, initialValue: "0");

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest count, formatted for display.</summary>
    public string Text => _textHelper.Value;
}
