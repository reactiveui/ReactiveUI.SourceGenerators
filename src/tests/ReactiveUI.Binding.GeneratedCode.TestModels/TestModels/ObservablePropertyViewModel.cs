// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>
/// A view model with properties of type IObservable for testing WhenAnyObservable.
/// Implements INotifyPropertyChanged for property change observation.
/// </summary>
public class ObservablePropertyViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the command observable.</summary>
    public IObservable<string>? MyCommand
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(MyCommand)));
        }
    }

    /// <summary>Gets or sets another command observable.</summary>
    public IObservable<string>? OtherCommand
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new(nameof(OtherCommand)));
        }
    }
}
