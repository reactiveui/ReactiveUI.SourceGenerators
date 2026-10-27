// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Pauses one subscriber call while a separate producer competes for delivery.</summary>
internal sealed class ContendedDeliveryObserver : IObserver<int>, IDisposable
{
    /// <summary>The longest permitted fixture synchronization wait.</summary>
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    /// <summary>Whether the next value should pause.</summary>
    private int _armed;

    /// <summary>Whether the pause ends only when cleanup releases it.</summary>
    private bool _hold;

    /// <summary>The fixed subscriber workload in milliseconds.</summary>
    private int _delay;

    /// <summary>Gets the signal raised when the paused subscriber owns delivery.</summary>
    internal ManualResetEventSlim Entered { get; } = new();

    /// <summary>Gets the signal that ends a held subscriber call.</summary>
    internal ManualResetEventSlim Release { get; } = new();

    /// <inheritdoc/>
    public void OnNext(int value)
    {
        if (Interlocked.Exchange(ref _armed, 0) == 0)
        {
            return;
        }

        Entered.Set();
        if (_hold)
        {
            if (!Release.Wait(Timeout))
            {
                throw new TimeoutException("The benchmark did not release the subscriber.");
            }
        }
        else
        {
            Thread.Sleep(_delay);
        }
    }

    /// <inheritdoc/>
    public void OnError(Exception error) => throw new InvalidOperationException("Observation failed.", error);

    /// <inheritdoc/>
    public void OnCompleted()
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Entered.Dispose();
        Release.Dispose();
    }

    /// <summary>Arms one pause after the subscription's initial value has been delivered.</summary>
    /// <param name="hold">Whether cleanup controls when the subscriber resumes.</param>
    /// <param name="delay">The subscriber workload in milliseconds when not held.</param>
    internal void Arm(bool hold, int delay)
    {
        _hold = hold;
        _delay = delay;
        Volatile.Write(ref _armed, 1);
    }
}
