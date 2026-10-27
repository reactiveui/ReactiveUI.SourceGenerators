// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A sequencer that queues every item until the benchmark drains it.</summary>
public sealed class QueuingSequencer : ISequencer
{
    /// <summary>The most items one drain runs before it reports that a binding never settles.</summary>
    private const int DrainLimit = 100_000;

    /// <summary>The items scheduled and not yet run.</summary>
    private readonly Queue<IWorkItem> _pending = new();

    /// <inheritdoc/>
    public DateTimeOffset Now => ImmediateSequencer.Instance.Now;

    /// <inheritdoc/>
    public long Timestamp => ImmediateSequencer.Instance.Timestamp;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Schedule(IWorkItem item) => _pending.Enqueue(item);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Schedule(IWorkItem item, long dueTimestamp) => _pending.Enqueue(item);

    /// <summary>Runs queued items, and the items they queue, until none are left.</summary>
    /// <exception cref="InvalidOperationException">The queue still held items after the drain limit.</exception>
    public void Drain()
    {
        var ran = 0;
        while (_pending.Count > 0)
        {
            if (ran == DrainLimit)
            {
                throw new InvalidOperationException("The binding never settled.");
            }

            _pending.Dequeue().Execute();
            ran++;
        }
    }
}
