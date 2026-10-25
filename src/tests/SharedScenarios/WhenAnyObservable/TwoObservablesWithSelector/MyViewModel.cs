// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace SharedScenarios.WhenAnyObservable.TwoObservablesWithSelector;

/// <summary>ViewModel with two observable properties of different types.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>The backing field for <see cref="Count"/>.</summary>
    private IObservable<int>? _count;

    /// <summary>The backing field for <see cref="Message"/>.</summary>
    private IObservable<string>? _message;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the count observable.</summary>
    public IObservable<int>? Count
    {
        get => _count;
        set
        {
            if (_count == value)
            {
                return;
            }

            _count = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    /// <summary>Gets or sets the message observable.</summary>
    public IObservable<string>? Message
    {
        get => _message;
        set
        {
            if (_message == value)
            {
                return;
            }

            _message = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }
    }
}
