// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Wpf.Tests.XamlPage;

/// <summary>A view model for <see cref="NamedControlsView"/>.</summary>
[System.Diagnostics.DebuggerDisplay("NamedControlsViewModel: Name = {Name}")]
public sealed class NamedControlsViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the name.</summary>
    public string? Name
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
