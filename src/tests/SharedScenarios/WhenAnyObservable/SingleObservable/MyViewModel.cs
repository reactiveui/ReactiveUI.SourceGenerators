// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace SharedScenarios.WhenAnyObservable.SingleObservable;

/// <summary>ViewModel implementing INotifyPropertyChanged with a property that is itself an observable.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="MyCommand"/>.</summary>
    private IObservable<string>? _myCommand;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the command observable.</summary>
    public IObservable<string>? MyCommand
    {
        get => _myCommand;
        set
        {
            if (_myCommand == value)
            {
                return;
            }

            _myCommand = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyCommand)));
        }
    }
}
