// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Raises cached notifications around an integer write without per-change fixture allocations.</summary>
public sealed class DeliveryViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>The shared after-change notification.</summary>
    private static readonly PropertyChangedEventArgs Changed = new(nameof(Value));

    /// <summary>The shared before-change notification.</summary>
    private static readonly PropertyChangingEventArgs Changing = new(nameof(Value));

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the value, raising both events even when the value repeats.</summary>
    public int Value
    {
        get => Volatile.Read(ref field);
        set
        {
            PropertyChanging?.Invoke(this, Changing);
            Volatile.Write(ref field, value);
            PropertyChanged?.Invoke(this, Changed);
        }
    }
}
