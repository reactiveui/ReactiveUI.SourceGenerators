// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindOneWay.SinglePropertyIntToInt;

/// <summary>Target View with an integer property.</summary>
public class MyView : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="DisplayCount"/>.</summary>
    private int _displayCount;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the display count.</summary>
    public int DisplayCount
    {
        get => _displayCount;
        set
        {
            if (_displayCount == value)
            {
                return;
            }

            _displayCount = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayCount)));
        }
    }
}
