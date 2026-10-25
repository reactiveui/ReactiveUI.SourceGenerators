// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharedScenarios.ToProperty.PartialProtectedBase;

/// <summary>A base class that exposes its raise methods to derived types only.</summary>
public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    /// <param name="args">The event arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

    /// <summary>Raises <see cref="PropertyChanged"/> for a property name.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    /// <summary>Raises <see cref="PropertyChanging"/>.</summary>
    /// <param name="args">The event arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanging(PropertyChangingEventArgs args) => PropertyChanging?.Invoke(this, args);
}
