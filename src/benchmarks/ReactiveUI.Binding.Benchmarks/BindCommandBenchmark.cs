// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Measures generated command binding setup separately from command replacement on the view.</summary>
public class BindCommandBenchmark
{
    /// <summary>The command replacements in a measured batch.</summary>
    private const int ChangeCount = 1_000;

    /// <summary>The source of command replacements.</summary>
    private BoundViewModel _model = null!;

    /// <summary>The view receiving commands.</summary>
    private BoundView _view = null!;

    /// <summary>Two commands alternating without per-change construction.</summary>
    private BenchmarkCommand[] _commands = null!;

    /// <summary>The established binding used for replacement measurements.</summary>
    private IDisposable _binding = null!;

    /// <summary>Creates the model and view outside the binding measurement.</summary>
    [GlobalSetup(Targets = [nameof(Subscribe), nameof(SubscribeWithExplicitEvent)])]
    public void Setup()
    {
        _commands = [new(), new()];
        _model = new() { Command = _commands[0] };
        _view = new() { ViewModel = _model };
    }

    /// <summary>Establishes the generated binding before command replacement measurement.</summary>
    [GlobalSetup(Target = nameof(ReplaceCommand))]
    public void SetupDelivery()
    {
        Setup();
        _binding = _view.BindCommand(_model, x => x.Command, x => x.Button);
    }

    /// <summary>Releases the established command binding.</summary>
    [GlobalCleanup(Target = nameof(ReplaceCommand))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CleanupDelivery() => _binding.Dispose();

    /// <summary>Creates a generated binding, applies the initial command, and restores the control on disposal.</summary>
    [Benchmark]
    public void Subscribe()
    {
        using var binding = _view.BindCommand(_model, x => x.Command, x => x.Button);
    }

    /// <summary>Creates a generated binding that names the event the control is bound through.</summary>
    [Benchmark]
    public void SubscribeWithExplicitEvent()
    {
        using var binding = _view.BindCommand(_model, x => x.Command, x => x.Button, toEvent: "Click");
    }

    /// <summary>Writes alternating commands through a live binding on the calling thread.</summary>
    /// <returns>The command installed on the view.</returns>
    [Benchmark(OperationsPerInvoke = ChangeCount)]
    public ICommand? ReplaceCommand()
    {
        for (var i = 0; i < ChangeCount; i++)
        {
            _model.Command = _commands[i & 1];
        }

        return _view.Button.Command;
    }
}
