// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// An observer that records what it was given and how the calls overlapped, for tests that need to tell
/// a serialized producer from one whose emissions can run concurrently on the downstream observer.
/// </summary>
/// <typeparam name="T">The emitted element type.</typeparam>
internal sealed class EmissionRecorder<T> : IObserver<T>
{
    /// <summary>Serializes the recorded state against emissions arriving on several threads.</summary>
    private readonly Lock _gate = new();

    /// <summary>The values received so far, in arrival order.</summary>
    private readonly List<T> _values = [];

    /// <summary>The errors received so far.</summary>
    private readonly List<Exception> _errors = [];

    /// <summary>The number of emissions currently in flight.</summary>
    private int _inFlight;

    /// <summary>The high-water mark of <see cref="_inFlight"/>.</summary>
    private int _maxInFlight;

    /// <summary>
    /// Gets or sets a one-shot action run from inside the first emission, after the value has been
    /// recorded and while that call still counts as in flight. It runs outside this recorder's own lock
    /// so that what it measures is the producer's serialization rather than the recorder's.
    /// </summary>
    internal Action? OnFirstValue { get; set; }

    /// <summary>
    /// Gets the greatest number of emissions that were ever in flight at once. Above one means the
    /// producer let two emissions overlap on the downstream observer.
    /// </summary>
    internal int MaxConcurrentEmissions
    {
        get
        {
            lock (_gate)
            {
                return _maxInFlight;
            }
        }
    }

    /// <inheritdoc/>
    public void OnNext(T value)
    {
        Action? hook;

        lock (_gate)
        {
            _inFlight++;
            if (_inFlight > _maxInFlight)
            {
                _maxInFlight = _inFlight;
            }

            _values.Add(value);

            hook = _values.Count == 1 ? OnFirstValue : null;
            OnFirstValue = null;
        }

        try
        {
            hook?.Invoke();
        }
        finally
        {
            lock (_gate)
            {
                _inFlight--;
            }
        }
    }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
        lock (_gate)
        {
            _errors.Add(error);
        }
    }

    /// <inheritdoc/>
    public void OnCompleted()
    {
    }

    /// <summary>Takes a copy of the values recorded so far.</summary>
    /// <returns>The values received, in arrival order.</returns>
    internal IReadOnlyList<T> Snapshot()
    {
        lock (_gate)
        {
            return [.. _values];
        }
    }

    /// <summary>Takes a copy of the errors recorded so far.</summary>
    /// <returns>The errors received, in arrival order.</returns>
    internal IReadOnlyList<Exception> ErrorSnapshot()
    {
        lock (_gate)
        {
            return [.. _errors];
        }
    }
}
