// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;

namespace ReactiveUI.SourceGenerators.Runtime.Benchmarks;

/// <summary>Measures the members the generators write, as a consumer's app runs them.</summary>
/// <remarks>
/// Each benchmark has one <c>PropertyChanged</c> subscriber, as a bound view would, so the notifications the generated
/// code raises are delivered. Allocations per operation are the figure to watch: the generated code should add none of
/// its own beyond what ReactiveUI's notification needs.
/// </remarks>
public class GeneratedCodeBenchmarks
{
    /// <summary>The view model under measurement.</summary>
    private readonly BenchmarkViewModel _viewModel = new();

    /// <summary>The plain object under measurement.</summary>
    private readonly BenchmarkPlainObject _plainObject = new();

    /// <summary>The first of two collections the collection property alternates between.</summary>
    private readonly ObservableCollection<int> _first = [];

    /// <summary>The second of two collections the collection property alternates between.</summary>
    private readonly ObservableCollection<int> _second = [];

    /// <summary>The number of notifications delivered, so the subscriber cannot be optimized away.</summary>
    private int _notifications;

    /// <summary>Which value the next set writes.</summary>
    private bool _toggle;

    /// <summary>Subscribes to both objects' notifications and gives the collection property its first value.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _viewModel.PropertyChanged += OnPropertyChanged;
        _plainObject.PropertyChanged += OnPropertyChanged;
        _viewModel.Items = _first;
    }

    /// <summary>Replaces the collection a <c>[ReactiveCollection]</c> property holds.</summary>
    /// <returns>The number of notifications delivered so far.</returns>
    [Benchmark]
    public int ReplaceCollection()
    {
        _toggle = !_toggle;
        _viewModel.Items = _toggle ? _second : _first;
        return _notifications;
    }

    /// <summary>Adds to and clears the collection a <c>[ReactiveCollection]</c> property holds.</summary>
    /// <returns>The number of notifications delivered so far.</returns>
    [Benchmark]
    public int ChangeCollection()
    {
        var items = _viewModel.Items!;
        items.Add(1);
        items.Clear();
        return _notifications;
    }

    /// <summary>Sets a <c>[Reactive]</c> property that also notifies another property.</summary>
    /// <returns>The number of notifications delivered so far.</returns>
    [Benchmark]
    public int SetReactivePropertyWithAlsoNotify()
    {
        _toggle = !_toggle;
        _viewModel.Name = _toggle ? "first" : "second";
        return _notifications;
    }

    /// <summary>Sets a <c>[Reactive]</c> property on a type that implements <c>IReactiveObject</c> through <c>[IReactiveObject]</c>.</summary>
    /// <returns>The number of notifications delivered so far.</returns>
    [Benchmark]
    public int SetGeneratedReactiveObjectProperty()
    {
        _plainObject.Value++;
        return _notifications;
    }

    /// <summary>Reads a generated command, which is created on first use.</summary>
    /// <returns>The command's hash code, so the read cannot be optimized away.</returns>
    [Benchmark]
    public int ReadCommand() => _viewModel.IncrementCommand.GetHashCode();

    /// <summary>Counts a delivered notification.</summary>
    /// <param name="sender">The object that raised it.</param>
    /// <param name="e">The notification.</param>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => _notifications++;
}
