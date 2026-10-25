// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.InvokeCommand.DeepCommandPath;

/// <summary>A view model whose child holds the command to execute.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Child"/>.</summary>
    private ChildViewModel _child = new ChildViewModel();

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the child holding the command.</summary>
    public ChildViewModel Child
    {
        get => _child;
        set
        {
            if (_child == value)
            {
                return;
            }

            _child = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
        }
    }
}
