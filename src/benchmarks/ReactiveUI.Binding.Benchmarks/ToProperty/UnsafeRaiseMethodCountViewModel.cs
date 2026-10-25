// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Benchmarks.ToProperty;

/// <summary>A view model that is not partial and raises through a protected method, backed by <c>ToPropertyUnsafe</c>.</summary>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Finds the members that raise a property's change notifications by reflection; they may be trimmed.")]
[RequiresDynamicCode("Closes ReactiveUI's generic raise extension over IReactiveObject at run time.")]
#endif
public class UnsafeRaiseMethodCountViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _count;

    /// <summary>Initializes a new instance of the <see cref="UnsafeRaiseMethodCountViewModel"/> class.</summary>
    /// <param name="counts">The values <see cref="Count"/> takes.</param>
    public UnsafeRaiseMethodCountViewModel(IObservable<int> counts) => _count = counts.ToPropertyUnsafe(this, x => x.Count);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest count.</summary>
    public int Count => _count.Value;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the helper.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _count.Dispose();
        }
    }

    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    /// <param name="args">The event arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);
}
