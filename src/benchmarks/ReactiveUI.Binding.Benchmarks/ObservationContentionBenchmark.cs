// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Separates competing setter latency from the time a slow subscriber occupies the delivery thread.</summary>
public class ObservationContentionBenchmark : IDisposable
{
    /// <summary>The competing changes supplied during one slow subscriber call.</summary>
    private const int CompetingChanges = 16;

    /// <summary>The subscriber workload, longer than the delivery gate's contention budget.</summary>
    private const int SubscriberDelayMilliseconds = 40;

    /// <summary>The first value produced by the competing setter.</summary>
    private const int CompetingValue = 2;

    /// <summary>The observed integer source.</summary>
    private DeliveryViewModel _source = null!;

    /// <summary>The subscriber that establishes deterministic contention.</summary>
    private ContendedDeliveryObserver _observer = null!;

    /// <summary>The live subscription owning the delivery gate.</summary>
    private IDisposable _subscription = null!;

    /// <summary>The competing producer, created outside the measurement.</summary>
    private Thread _worker = null!;

    /// <summary>A worker failure propagated by cleanup.</summary>
    private Exception? _workerError;

    /// <summary>Whether the current iteration's resources have been released.</summary>
    private bool _disposed = true;

    /// <summary>Gets or sets the delivery contract.</summary>
    [Params(nameof(ObservationKind.Changed), nameof(ObservationKind.Changing), nameof(ObservationKind.Plugin))]
    public string Kind { get; set; } = nameof(ObservationKind.Changed);

    /// <summary>Blocks a subscriber on a worker before measuring the competing setter.</summary>
    /// <exception cref="TimeoutException">The worker fails to enter its subscriber.</exception>
    [IterationSetup(Target = nameof(ProducerLatency))]
    public void SetupProducer()
    {
        Setup();
        _observer.Arm(true, 0);
        _worker = new(OccupyDelivery) { IsBackground = true };
        _worker.Start();
        if (!_observer.Entered.Wait(ContendedDeliveryObserver.Timeout))
        {
            throw new TimeoutException("The worker did not enter the subscriber.");
        }
    }

    /// <summary>Prepares a waiting producer and a 40 ms subscriber workload on the measured thread.</summary>
    [IterationSetup(Target = nameof(DeliveryThread))]
    public void SetupDelivery()
    {
        Setup();
        _observer.Arm(false, SubscriberDelayMilliseconds);
        _worker = new(CompeteForDelivery) { IsBackground = true };
        _worker.Start();
    }

    /// <summary>Releases the subscriber and joins all producer work outside the timed region.</summary>
    [IterationCleanup]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Cleanup() => Dispose();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Measures one setter's return while another thread holds the subscriber indefinitely.</summary>
    [Benchmark]
    public void ProducerLatency() => _source.Value = CompetingValue;

    /// <summary>Measures a delivery-thread setter, including the 40 ms subscriber and any handed-off work it drains.</summary>
    [Benchmark]
    public void DeliveryThread() => _source.Value = 1;

    /// <summary>Releases the current iteration after every producer has finished.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    /// <exception cref="TimeoutException">The competing producer does not finish.</exception>
    /// <exception cref="InvalidOperationException">The competing producer fails.</exception>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        _disposed = true;
        _observer.Release.Set();
        if (!_worker.Join(ContendedDeliveryObserver.Timeout))
        {
            throw new TimeoutException("The producer did not finish.");
        }

        _subscription.Dispose();
        _observer.Dispose();
        if (_workerError is not null)
        {
            throw new InvalidOperationException("The producer failed.", _workerError);
        }
    }

    /// <summary>Creates the fixture and delivers the initial value before arming the slow subscriber.</summary>
    private void Setup()
    {
        _source = new();
        _observer = new();
        _workerError = null;
        _subscription = DeliveryObservation.Create(_source, Kind).Subscribe(_observer);
        _disposed = false;
    }

    /// <summary>Owns the subscriber until cleanup releases it.</summary>
    private void OccupyDelivery()
    {
        try
        {
            _source.Value = 1;
        }
        catch (Exception error)
        {
            _workerError = error;
        }
    }

    /// <summary>Competes only after the measured thread has entered its slow subscriber.</summary>
    /// <exception cref="TimeoutException">The measured thread fails to enter the subscriber.</exception>
    private void CompeteForDelivery()
    {
        try
        {
            if (!_observer.Entered.Wait(ContendedDeliveryObserver.Timeout))
            {
                throw new TimeoutException("The delivery thread did not enter the subscriber.");
            }

            for (var i = 0; i < CompetingChanges; i++)
            {
                _source.Value = i + CompetingValue;
            }
        }
        catch (Exception error)
        {
            _workerError = error;
        }
    }
}
