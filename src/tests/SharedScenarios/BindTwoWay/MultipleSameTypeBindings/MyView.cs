// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindTwoWay.MultipleSameTypeBindings;

/// <summary>Target View with two string properties.</summary>
public class MyView : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="FirstNameText"/>.</summary>
    private string _firstNameText = string.Empty;

    /// <summary>The backing field for <see cref="LastNameText"/>.</summary>
    private string _lastNameText = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the first name text.</summary>
    public string FirstNameText
    {
        get => _firstNameText;
        set
        {
            if (_firstNameText == value)
            {
                return;
            }

            _firstNameText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstNameText)));
        }
    }

    /// <summary>Gets or sets the last name text.</summary>
    public string LastNameText
    {
        get => _lastNameText;
        set
        {
            if (_lastNameText == value)
            {
                return;
            }

            _lastNameText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastNameText)));
        }
    }
}
