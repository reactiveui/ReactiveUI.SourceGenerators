// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace SharedScenarios.WhenChanged.MultipleInvocationsSameViewModel;

/// <summary>ViewModel with multiple properties observed separately.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Name"/>.</summary>
    private string _name = string.Empty;

    /// <summary>The backing field for <see cref="Age"/>.</summary>
    private int _age;

    /// <summary>The backing field for <see cref="Score"/>.</summary>
    private double _score;

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

    /// <summary>Gets or sets the score.</summary>
    public double Score
    {
        get => _score;
        set
        {
            if (Math.Abs(_score - value) < double.Epsilon)
            {
                return;
            }

            _score = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
        }
    }
}
