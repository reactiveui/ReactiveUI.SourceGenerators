// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Measures generated interaction binding registration and handler migration between interactions.</summary>
public class BindInteractionBenchmark
{
    /// <summary>The replacements in a measured batch.</summary>
    private const int ChangeCount = 1_000;

    /// <summary>The source of interaction replacements.</summary>
    private BoundViewModel _model = null!;

    /// <summary>The view owning the interaction handler.</summary>
    private BoundView _view = null!;

    /// <summary>The two interactions receiving the handler in turn.</summary>
    private Interaction<int, int>[] _interactions = null!;

    /// <summary>The established binding used for replacement measurements.</summary>
    private IDisposable _binding = null!;

    /// <summary>Creates the fixture outside the binding measurement.</summary>
    [GlobalSetup(Target = nameof(Subscribe))]
    public void Setup()
    {
        _interactions = [new(), new()];
        _model = new() { Question = _interactions[0] };
        _view = new() { ViewModel = _model };
    }

    /// <summary>Establishes a generated binding before replacing interactions.</summary>
    [GlobalSetup(Target = nameof(ReplaceInteraction))]
    public void SetupDelivery()
    {
        Setup();
        _binding = _view.BindInteraction(_model, x => x.Question, Answer);
    }

    /// <summary>Releases the binding and its final handler.</summary>
    [GlobalCleanup(Target = nameof(ReplaceInteraction))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CleanupDelivery() => _binding.Dispose();

    /// <summary>Creates a generated binding, registers its initial handler, and disposes both.</summary>
    [Benchmark]
    public void Subscribe()
    {
        using var binding = _view.BindInteraction(_model, x => x.Question, Answer);
    }

    /// <summary>Moves the registered handler between existing interactions through the live binding.</summary>
    /// <returns>The interaction that owns the handler.</returns>
    [Benchmark(OperationsPerInvoke = ChangeCount)]
    public Interaction<int, int> ReplaceInteraction()
    {
        for (var i = 0; i < ChangeCount; i++)
        {
            _model.Question = _interactions[i & 1];
        }

        return _model.Question;
    }

    /// <summary>Completes the interaction synchronously without adding asynchronous work.</summary>
    /// <param name="context">The question to answer.</param>
    /// <returns>A completed handler task.</returns>
    private static Task Answer(IInteractionContext<int, int> context)
    {
        context.SetOutput(context.Input);
        return Task.CompletedTask;
    }
}
