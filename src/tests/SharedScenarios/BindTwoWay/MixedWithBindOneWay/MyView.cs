// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindTwoWay.MixedWithBindOneWay;

/// <summary>Target View with properties for mixed binding.</summary>
public class MyView : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="NameText"/>.</summary>
    private string _nameText = string.Empty;

    /// <summary>The backing field for <see cref="CountDisplay"/>.</summary>
    private int _countDisplay;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the name text.</summary>
    public string NameText
    {
        get => _nameText;
        set
        {
            if (_nameText == value)
            {
                return;
            }

            _nameText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameText)));
        }
    }

    /// <summary>Gets or sets the count display.</summary>
    public int CountDisplay
    {
        get => _countDisplay;
        set
        {
            if (_countDisplay == value)
            {
                return;
            }

            _countDisplay = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountDisplay)));
        }
    }
}
