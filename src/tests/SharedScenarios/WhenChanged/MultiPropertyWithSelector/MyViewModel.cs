// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.MultiPropertyWithSelector;

/// <summary>ViewModel with first and last name properties.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="FirstName"/>.</summary>
    private string _firstName = string.Empty;

    /// <summary>The backing field for <see cref="LastName"/>.</summary>
    private string _lastName = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the first name.</summary>
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName == value)
            {
                return;
            }

            _firstName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstName)));
        }
    }

    /// <summary>Gets or sets the last name.</summary>
    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName == value)
            {
                return;
            }

            _lastName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastName)));
        }
    }
}
