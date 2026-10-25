// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

namespace SharedScenarios.BindCommand.DeepCommandPath;

/// <summary>Child ViewModel that owns the command.</summary>
public class ChildViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="SaveCommand"/>.</summary>
    private ICommand? _saveCommand;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the save command.</summary>
    public ICommand? SaveCommand
    {
        get => _saveCommand;
        set
        {
            if (_saveCommand == value)
            {
                return;
            }

            _saveCommand = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveCommand)));
        }
    }
}
