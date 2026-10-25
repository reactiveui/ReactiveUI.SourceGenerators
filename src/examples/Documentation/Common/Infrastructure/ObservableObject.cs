// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>A base class for objects that report each property change through <see cref="INotifyPropertyChanged"/>.</summary>
public class ObservableObject : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Stores a new value and reports the change when the value differs from the stored one.</summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The field that holds the property value.</param>
    /// <param name="value">The value being assigned.</param>
    /// <param name="propertyName">The name of the property; filled in by the compiler.</param>
    /// <returns><see langword="true"/> when the value changed and the event was raised.</returns>
    internal bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    /// <summary>Reports that a property changed.</summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
