// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindInteraction.DeepPropertyPath;

/// <summary>ViewModel with a nested child that holds the interaction.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Child"/>.</summary>
    private ChildViewModel? _child;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the child view model.</summary>
    public ChildViewModel? Child
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
