// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Measures value delivery through generated nullable and numeric formatting conversions.</summary>
public class TypedAdapterBenchmark
{
    /// <summary>The number of values delivered in each measured batch.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The numeric values delivered to the nullable destination.</summary>
    private readonly BenchmarkSource<int> _nullableSource = new();

    /// <summary>The numeric values delivered to the string destination.</summary>
    private readonly BenchmarkSource<int> _formatSource = new();

    /// <summary>The typed destination properties.</summary>
    private readonly TypedAdapterTarget _target = new();

    /// <summary>The active nullable conversion binding.</summary>
    private IDisposable _nullableBinding = null!;

    /// <summary>The active formatting conversion binding.</summary>
    private IDisposable _formatBinding = null!;

    /// <summary>The number of numeric batches requested by the benchmark runner.</summary>
    private long _nullableBatches;

    /// <summary>The number of formatting batches requested by the benchmark runner.</summary>
    private long _formatBatches;

    /// <summary>Creates the bindings before measuring per-change delivery.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _nullableBinding = _nullableSource.BindTo(_target, target => target.Number);
        _formatBinding = _formatSource.BindTo(_target, target => target.Text);
    }

    /// <summary>Releases both subscriptions.</summary>
    /// <exception cref="InvalidOperationException">A binding dropped writes or delivered the wrong value.</exception>
    [GlobalCleanup]
    public void Cleanup()
    {
        _nullableBinding.Dispose();
        _formatBinding.Dispose();
        if (_target.NumberWrites != _nullableBatches * ValueCount || _target.TextWrites != _formatBatches * ValueCount
            || (_nullableBatches != 0 && _target.Number != ValueCount - 1)
            || (_formatBatches != 0 && _target.Text != (ValueCount - 1).ToString(System.Globalization.CultureInfo.CurrentCulture)))
        {
            throw new InvalidOperationException("The benchmark did not deliver every requested value.");
        }
    }

    /// <summary>Delivers numeric values through the generated nullable lift.</summary>
    /// <returns>The final delivered number.</returns>
    [Benchmark(OperationsPerInvoke = ValueCount)]
    public int NullableValues()
    {
        _nullableBatches++;
        for (var i = 0; i < ValueCount; i++)
        {
            _nullableSource.Push(i);
        }

        return _target.Number.GetValueOrDefault();
    }

    /// <summary>Formats numbers with the typed generated converter.</summary>
    /// <returns>The final formatted value.</returns>
    [Benchmark(OperationsPerInvoke = ValueCount)]
    public string FormattedValues()
    {
        _formatBatches++;
        for (var i = 0; i < ValueCount; i++)
        {
            _formatSource.Push(i);
        }

        return _target.Text;
    }
}
