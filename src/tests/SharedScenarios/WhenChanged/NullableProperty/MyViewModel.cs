// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace SharedScenarios.WhenChanged.NullableProperty;

/// <summary>ViewModel with a nullable string property.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="NullableName"/>.</summary>
    private string? _nullableName;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the nullable name.</summary>
    public string? NullableName
    {
        get => _nullableName;
        set
        {
            if (_nullableName == value)
            {
                return;
            }

            _nullableName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NullableName)));
        }
    }
}
