// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace PlatformBindingsVerification.Wpf;

/// <summary>Holds the command a button runs, which the example replaces from a background thread.</summary>
[System.Diagnostics.DebuggerDisplay("WpfCommandViewModel: Save = {Save}")]
public sealed class WpfCommandViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the command the save button runs.</summary>
    public ICommand? Save
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new(nameof(Save)));
        }
    }
}
