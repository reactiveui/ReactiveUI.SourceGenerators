// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace SharedScenarios.BindCommand.ObservableParam;

/// <summary>ViewModel exposing an ICommand property.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Save"/>.</summary>
    private ICommand? _save;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the save command.</summary>
    public ICommand? Save
    {
        get => _save;
        set
        {
            if (_save == value)
            {
                return;
            }

            _save = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Save)));
        }
    }
}
