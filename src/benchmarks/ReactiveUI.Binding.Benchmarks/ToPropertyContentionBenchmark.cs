// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Measures a helper's delivery gate as one, two and four threads push distinct values into it at once.</summary>
/// <remarks>
/// The producers are started before the measurement and released together by a barrier, so thread start-up is
/// not part of the number. Every value is distinct, so each one passes the distinct gate and is delivered.
/// </remarks>
public class ToPropertyContentionBenchmark : IDisposable
{
    /// <summary>The values each producer pushes per invocation.</summary>
    private const int ValuesPerProducer = 10_000;

    /// <summary>The source the helper follows.</summary>
    private BenchmarkSource<int> _source = null!;

    /// <summary>The helper under contention.</summary>
    private ObservableAsPropertyHelper<int> _helper = null!;

    /// <summary>Releases the producers together and waits for them to finish.</summary>
    private Barrier _barrier = null!;

    /// <summary>The producer threads, which live for the whole run.</summary>
    private Thread[] _producers = [];

    /// <summary>The number of values delivered, read so the callback is not elided.</summary>
    private int _delivered;

    /// <summary>Whether the producers should exit.</summary>
    private volatile bool _stopping;

    /// <summary>Gets or sets the number of threads pushing at once.</summary>
    [Params(1, 2, 4)]
    public int Producers { get; set; } = 1;

    /// <summary>Starts the producers, parked on the barrier until a benchmark invocation releases them.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = new();
        _helper = ObservableAsPropertyHelper<int>.Create(
            _source,
            this,
            static (owner, _) => ((ToPropertyContentionBenchmark)owner!)._delivered++,
            null,
            -1,
            false,
            null);

        // One participant per producer plus the benchmark thread, which releases and then collects them.
        _barrier = new(Producers + 1);
        _producers = new Thread[Producers];
        for (var p = 0; p < Producers; p++)
        {
            var offset = p;
            _producers[p] = new(() => Produce(offset)) { IsBackground = true };
            _producers[p].Start();
        }
    }

    /// <summary>Releases every producer for one burst of distinct values and waits for all of them to finish.</summary>
    /// <returns>The number of values delivered so far.</returns>
    [Benchmark(OperationsPerInvoke = ValuesPerProducer)]
    public int PushConcurrently()
    {
        _barrier.SignalAndWait();
        _barrier.SignalAndWait();
        return _delivered;
    }

    /// <summary>Stops the producers and releases the helper.</summary>
    [GlobalCleanup]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Stops the producers and releases the helper.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _stopping = true;
        _barrier.SignalAndWait();
        for (var p = 0; p < _producers.Length; p++)
        {
            _producers[p].Join();
        }

        _barrier.Dispose();
        _helper.Dispose();
    }

    /// <summary>Pushes bursts of values that no other producer pushes, so none repeats the one before it.</summary>
    /// <param name="offset">The producer's index, which interleaves its values with the others'.</param>
    private void Produce(int offset)
    {
        var next = offset;
        while (true)
        {
            _barrier.SignalAndWait();
            if (_stopping)
            {
                return;
            }

            for (var i = 0; i < ValuesPerProducer; i++)
            {
                next += Producers;
                _source.Push(next);
            }

            _barrier.SignalAndWait();
        }
    }
}
