// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// A base class for edit forms. Each setter raises <see cref="INotifyPropertyChanging"/> before it stores a new value
/// and <see cref="INotifyPropertyChanged"/> after it, which is what <c>WhenChanging</c> and <c>WhenChanged</c> observe.
/// </summary>
public class ChangingObject : ObservableObject, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event System.ComponentModel.PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Reports that a property is about to change, when the new value differs from the current one.</summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="current">The value the property holds now.</param>
    /// <param name="value">The value being assigned.</param>
    /// <param name="propertyName">The name of the property; filled in by the compiler.</param>
    /// <returns><see langword="true"/> when the value differs and the notification was raised.</returns>
    protected bool RaisePropertyChanging<T>(T current, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(current, value))
        {
            return false;
        }

        PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
        return true;
    }
}
