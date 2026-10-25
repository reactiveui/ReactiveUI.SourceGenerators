// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Button = System.Windows.Controls.Button;

namespace PlatformBindingsVerification.Wpf;

/// <summary>A view that is a plain object, not a WPF element, holding a WPF button that the dispatcher owns.</summary>
[System.Diagnostics.DebuggerDisplay("WpfPlainCommandView: ViewModel = {ViewModel}")]
public sealed class WpfPlainCommandView : IViewFor<WpfCommandViewModel>, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the view model the view shows.</summary>
    public WpfCommandViewModel? ViewModel
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(ViewModel)));
        }
    }

    /// <summary>Gets the button that saves.</summary>
    public Button SaveButton { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (WpfCommandViewModel?)value;
    }
}
