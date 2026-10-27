// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.BindOneWay.SinglePropertyWithConverterAndScheduler;

/// <summary>Target View with a string property.</summary>
public class MyView : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="CountText"/>.</summary>
    private string _countText = string.Empty;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the count text.</summary>
    public string CountText
    {
        get => _countText;
        set
        {
            if (_countText == value)
            {
                return;
            }

            _countText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountText)));
        }
    }
}
