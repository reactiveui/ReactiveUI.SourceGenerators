// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Consumes integer notifications without allocating for each value.</summary>
public sealed class DeliveryObserver : IObserver<int>
{
    /// <summary>Gets the most recent observed value.</summary>
    public int Value { get; private set; }

    /// <inheritdoc/>
    public void OnNext(int value) => Value = value;

    /// <inheritdoc/>
    public void OnError(Exception error) => throw new InvalidOperationException("Observation failed.", error);

    /// <inheritdoc/>
    public void OnCompleted()
    {
    }
}
