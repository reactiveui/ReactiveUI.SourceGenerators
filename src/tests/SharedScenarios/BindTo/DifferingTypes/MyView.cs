// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindTo.DifferingTypes;

/// <summary>Target View with a string property bound from an int observable.</summary>
public class MyView : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Caption"/>.</summary>
    private string _caption = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the caption.</summary>
    public string Caption
    {
        get => _caption;
        set
        {
            if (_caption == value)
            {
                return;
            }

            _caption = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Caption)));
        }
    }
}
