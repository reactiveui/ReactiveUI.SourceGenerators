// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.MultiPropertyTwoProperties;

/// <summary>ViewModel with two observable properties.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Name"/>.</summary>
    private string _name = string.Empty;

    /// <summary>The backing field for <see cref="Age"/>.</summary>
    private int _age;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    /// <summary>Gets or sets the age.</summary>
    public int Age
    {
        get => _age;
        set
        {
            if (_age == value)
            {
                return;
            }

            _age = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
        }
    }
}
