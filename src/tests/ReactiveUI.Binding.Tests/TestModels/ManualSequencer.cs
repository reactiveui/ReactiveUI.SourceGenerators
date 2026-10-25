// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A sequencer that holds every scheduled item until a test runs the pending ones.</summary>
internal sealed class ManualSequencer : ISequencer
{
    /// <summary>The most items <see cref="RunUntilIdle"/> runs before it gives up.</summary>
    private const int MaxItems = 1_000;

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

    /// <summary>Runs the items that were pending when the call began, leaving any they schedule for the next pass.</summary>
    /// <returns>How many items ran.</returns>
    internal int RunPending()
    {
        var count = _pending.Count;
        for (var i = 0; i < count; i++)
        {
            _pending.Dequeue().Execute();
        }

        return count;
    }

    /// <summary>Runs passes until one finds nothing pending, or a limit is reached.</summary>
    /// <param name="maxPasses">The most passes to run.</param>
    /// <returns>The number of passes that ran work, or -1 when work was still pending at a limit.</returns>
    /// <remarks>The item limit stops a binding whose every write queues more than one write from growing without bound.</remarks>
    internal int RunUntilIdle(int maxPasses)
    {
        var ran = 0;
        for (var pass = 0; pass < maxPasses; pass++)
        {
            var count = _pending.Count;
            if (count == 0)
            {
                return pass;
            }

            for (var i = 0; i < count; i++)
            {
                ran++;
                if (ran > MaxItems)
                {
                    return -1;
                }

                _pending.Dequeue().Execute();
            }
        }

        return -1;
    }
}
