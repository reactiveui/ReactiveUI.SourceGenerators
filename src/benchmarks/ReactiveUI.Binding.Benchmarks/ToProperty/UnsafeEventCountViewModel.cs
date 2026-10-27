// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Benchmarks.ToProperty;

/// <summary>A view model that is not partial and raises only its field-like event, backed by <c>ToPropertyUnsafe</c>.</summary>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Finds the members that raise a property's change notifications by reflection; they may be trimmed.")]
[RequiresDynamicCode("Closes ReactiveUI's generic raise extension over IReactiveObject at run time.")]
#endif
public sealed class UnsafeEventCountViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>Backs <see cref="Count"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _count;

    /// <summary>Initializes a new instance of the <see cref="UnsafeEventCountViewModel"/> class.</summary>
    /// <param name="counts">The values <see cref="Count"/> takes.</param>
    public UnsafeEventCountViewModel(IObservable<int> counts) => _count = counts.ToPropertyUnsafe(this, x => x.Count);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the latest count.</summary>
    public int Count => _count.Value;

    /// <summary>Gets a value indicating whether anything subscribes to <see cref="PropertyChanged"/>.</summary>
    public bool HasSubscribers => PropertyChanged is not null;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _count.Dispose();
}
