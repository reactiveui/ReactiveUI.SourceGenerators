// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using ReactiveUI.Binding;

namespace SharedScenarios.ToProperty.FactoryOutResult;

/// <summary>A partial view model that takes its helper through an out parameter and its first value from a factory.</summary>
public partial class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _count;

    /// <summary>Initializes a new instance of the <see cref="MyViewModel"/> class.</summary>
    /// <param name="counts">The values <see cref="Count"/> takes.</param>
    public MyViewModel(IObservable<int> counts) => counts.ToProperty(this, x => x.Count, out _count, () => -1);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets the latest count.</summary>
    public int Count => _count.Value;
}
