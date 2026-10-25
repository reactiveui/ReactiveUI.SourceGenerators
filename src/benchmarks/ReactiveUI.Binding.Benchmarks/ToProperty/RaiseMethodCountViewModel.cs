// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Benchmarks.ToProperty;

/// <summary>A view model that is not partial but exposes a public event-args raise method.</summary>
public sealed class RaiseMethodCountViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _count;

    /// <summary>Initializes a new instance of the <see cref="RaiseMethodCountViewModel"/> class.</summary>
    /// <param name="counts">The values <see cref="Count"/> takes.</param>
    public RaiseMethodCountViewModel(IObservable<int> counts) => _count = counts.ToProperty(this, static x => x.Count);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest count.</summary>
    public int Count => _count.Value;

    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    /// <param name="args">The event arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _count.Dispose();
}
