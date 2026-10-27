// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.NullForgivingDeepChain;

/// <summary>Parent ViewModel with a nullable child property.</summary>
public class ParentViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Child"/>.</summary>
    private ChildModel? _child;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the nullable child model.</summary>
    public ChildModel? Child
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
