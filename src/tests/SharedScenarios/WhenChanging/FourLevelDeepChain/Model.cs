// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanging.FourLevelDeepChain;

/// <summary>Leaf model with a string value and before-change notifications.</summary>
public class Model : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>The backing field for <see cref="Value"/>.</summary>
    private string _value = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the value.</summary>
    public string Value
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;
            }

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Value)));
            _value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
