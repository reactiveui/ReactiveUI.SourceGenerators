// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Measures generated native wiring separately from platform rendering and interop costs.</summary>
public class NativeAdapterBenchmark : IObserver<int>
{
    /// <summary>The number of native updates in each invocation.</summary>
    private const int Updates = 1_000;

    /// <summary>The native value source.</summary>
    private readonly Android.Widget.NumberPicker _picker = new();

    /// <summary>The native binding target.</summary>
    private readonly NativeAdapterView _view = new();

    /// <summary>The command executed by native clicks.</summary>
    private readonly NativeAdapterCommand _command = new();

    /// <summary>The collection input stream.</summary>
    private readonly BenchmarkSource<System.Windows.Forms.Button[]> _source = new();

    /// <summary>Input controls allocated before measurement.</summary>
    private readonly System.Windows.Forms.Button[] _controls = [new(), new()];

    /// <summary>The active observation.</summary>
    private IDisposable _observation = null!;

    /// <summary>The active command binding.</summary>
    private IDisposable _commandBinding = null!;

    /// <summary>The active collection binding.</summary>
    private IDisposable _collectionBinding = null!;

    /// <summary>The number of completed observation batches.</summary>
    private long _observationBatches;

    /// <summary>The number of completed command batches.</summary>
    private long _commandBatches;

    /// <summary>The number of completed collection batches.</summary>
    private long _collectionBatches;

    /// <summary>The number of actual observation deliveries.</summary>
    private long _observed;

    /// <summary>The latest observed value.</summary>
    private int _last;

    /// <summary>Creates the native wiring before timing delivery.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _view.ViewModel = new() { Run = _command };
        _observation = _picker.WhenChanged(picker => picker.Value).Subscribe(this);
        _observed = 0;
        _commandBinding = _view.BindCommand(_view.ViewModel, model => model.Run, view => view.Button);
        _collectionBinding = _source.BindTo(_view, view => view.Controls);
    }

    /// <summary>Checks every native operation was delivered and releases the bindings.</summary>
    /// <exception cref="InvalidOperationException">The measured work was not delivered.</exception>
    [GlobalCleanup]
    public void Cleanup()
    {
        _observation.Dispose();
        _commandBinding.Dispose();
        _collectionBinding.Dispose();
        if (_observed != _observationBatches * Updates || _command.Executions != _commandBatches * Updates
            || _view.Controls.Owner.Layouts != _collectionBatches * Updates || _view.Controls.Owner.LayoutDepth != 0
            || (_observationBatches != 0 && _last != Updates - 1)
            || (_collectionBatches != 0 && _view.Controls.Count != _controls.Length))
        {
            throw new InvalidOperationException("Native adapter delivery counts do not match requested work.");
        }
    }

    /// <summary>Delivers value-type property notifications through the native event adapter.</summary>
    /// <returns>The final observed value.</returns>
    [Benchmark(OperationsPerInvoke = Updates)]
    public int ObserveValue()
    {
        _observationBatches++;
        for (var i = 0; i < Updates; i++)
        {
            _picker.Value = i;
        }

        return _last;
    }

    /// <summary>Executes commands through the native click adapter.</summary>
    /// <returns>The number of actual command executions.</returns>
    [Benchmark(OperationsPerInvoke = Updates)]
    public long ExecuteCommand()
    {
        _commandBatches++;
        for (var i = 0; i < Updates; i++)
        {
            _view.Button.RaiseClick();
        }

        return _command.Executions;
    }

    /// <summary>Populates the existing native collection from a concrete control array.</summary>
    /// <returns>The final collection size.</returns>
    [Benchmark(OperationsPerInvoke = Updates)]
    public int WriteCollection()
    {
        _collectionBatches++;
        for (var i = 0; i < Updates; i++)
        {
            _source.Push(_controls);
        }

        return _view.Controls.Count;
    }

    /// <inheritdoc/>
    public void OnNext(int value)
    {
        _observed++;
        _last = value;
    }

    /// <inheritdoc/>
    public void OnError(Exception error) => throw new InvalidOperationException("Native observation failed.", error);

    /// <inheritdoc/>
    public void OnCompleted() => throw new InvalidOperationException("Native observation completed unexpectedly.");
}
