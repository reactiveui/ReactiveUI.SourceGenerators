// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding;

namespace SharedScenarios.ToProperty.PublicRaiseMethod;

/// <summary>A view model that is not partial but exposes a public raise method.</summary>
public class MyViewModel : INotifyPropertyChanged
{
    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _count;

    /// <summary>Initializes a new instance of the <see cref="MyViewModel"/> class.</summary>
    /// <param name="counts">The values <see cref="Count"/> takes.</param>
    public MyViewModel(IObservable<int> counts) => _count = counts.ToProperty(this, x => x.Count);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest count.</summary>
    public int Count => _count.Value;

    /// <summary>Raises <see cref="PropertyChanged"/> for a property name.</summary>
    /// <param name="propertyName">The property name.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
