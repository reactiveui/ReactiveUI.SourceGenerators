// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A stream the benchmark drives by hand, so the value count is the measurement.</summary>
/// <typeparam name="T">The value type.</typeparam>
/// <remarks>
/// Hand-rolled rather than taken from an Rx library, because a scheduler or a subject of its own would put
/// that library's allocations into a number meant to be this one's.
/// </remarks>
public sealed class BenchmarkSource<T> : IObservable<T>
{
    /// <summary>The observers currently subscribed.</summary>
    private readonly List<IObserver<T>> _observers = [];

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new Subscription(this, observer);
    }

    /// <summary>Hands one value to every observer.</summary>
    /// <param name="value">The value to deliver.</param>
    public void Push(T value)
    {
        for (var i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnNext(value);
        }
    }

    /// <summary>Removes one observer when its subscription is disposed.</summary>
    /// <param name="parent">The stream subscribed to.</param>
    /// <param name="observer">The observer to remove.</param>
    private sealed class Subscription(BenchmarkSource<T> parent, IObserver<T> observer) : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => parent._observers.Remove(observer);
    }
}
