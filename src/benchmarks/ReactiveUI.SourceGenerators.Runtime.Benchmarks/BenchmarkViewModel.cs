// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ReactiveUI.SourceGenerators;

namespace ReactiveUI.SourceGenerators.Runtime.Benchmarks;

/// <summary>A view model whose members the generators write, measured by <see cref="GeneratedCodeBenchmarks"/>.</summary>
public partial class BenchmarkViewModel : ReactiveObject
{
    /// <summary>Backs the generated <c>Items</c> property.</summary>
    [ReactiveCollection]
    private ObservableCollection<int>? _items;

    /// <summary>Backs the generated <c>Name</c> property, which also notifies <see cref="Greeting"/>.</summary>
    [Reactive(nameof(Greeting))]
    private string _name = string.Empty;

    /// <summary>Backs the generated <c>Count</c> property.</summary>
    [Reactive]
    private int _count;

    /// <summary>Gets a value computed from <c>Name</c>.</summary>
    public string Greeting => Name;

    /// <summary>Increments <c>Count</c>; the generated command runs it.</summary>
    [ReactiveCommand]
    private void Increment() => Count++;
}
