// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A view model for testing WhenAnyObservable with observable properties.</summary>
public class TestWhenAnyObsViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the first command observable.</summary>
    public IObservable<int>? Command1
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the second command observable.</summary>
    public IObservable<int>? Command2
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the third command observable.</summary>
    public IObservable<int>? Command3
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the changes observable.</summary>
    public IObservable<int>? Changes
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new(propertyName));
}
