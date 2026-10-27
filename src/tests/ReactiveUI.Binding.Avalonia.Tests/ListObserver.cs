// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>An observer that records the values it receives.</summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class ListObserver<T> : IObserver<T>
{
    /// <summary>Gets the values received, in order.</summary>
    public List<T> Values { get; } = [];

    /// <inheritdoc/>
    public void OnCompleted()
    {
    }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnNext(T value) => Values.Add(value);
}
