// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.SinglePropertyIntToInt;

/// <summary>Target View implementing IViewFor with an integer property.</summary>
public class MyView : IViewFor, INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="CountValue"/>.</summary>
    private int _countValue;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

    /// <summary>Gets or sets the count value.</summary>
    public int CountValue
    {
        get => _countValue;
        set
        {
            if (_countValue == value)
            {
                return;
            }

            _countValue = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountValue)));
        }
    }
}
