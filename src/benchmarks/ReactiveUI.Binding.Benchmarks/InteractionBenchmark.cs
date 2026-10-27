// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Interaction benchmarks: asking a question, and registering and removing handlers.</summary>
public class InteractionBenchmark
{
    /// <summary>How many questions each Handle benchmark asks.</summary>
    private const int QuestionCount = 1_000;

    /// <summary>How many handlers each registration benchmark adds and removes.</summary>
    private const int RegistrationCount = 10;

    /// <summary>The interaction under measurement, with three handlers registered.</summary>
    private Interaction<string, int> _interaction = null!;

    /// <summary>The handler registrations, released after each iteration.</summary>
    private IDisposable[] _registrations = null!;

    /// <summary>Registers one handler that answers and two later ones that pass, so every question walks all three.</summary>
    [IterationSetup]
    public void Setup()
    {
        _interaction = new();
        _registrations =
        [
            _interaction.RegisterHandler(static context => context.SetOutput(context.Input.Length)),
            _interaction.RegisterHandler(static _ => { }),
            _interaction.RegisterHandler(static _ => { }),
        ];
    }

    /// <summary>Releases the handler registrations.</summary>
    [IterationCleanup]
    public void Cleanup()
    {
        for (var i = 0; i < _registrations.Length; i++)
        {
            _registrations[i].Dispose();
        }
    }

    /// <summary>Asks N questions that fall through two handlers to the one that answers.</summary>
    /// <returns>The sum of the answers.</returns>
    [Benchmark(Description = "Handle")]
    public async Task<int> Handle()
    {
        var total = 0;
        for (var i = 0; i < QuestionCount; i++)
        {
            total += await _interaction.Handle("question").ConfigureAwait(false);
        }

        return total;
    }

    /// <summary>Registers ten more handlers beside the three already registered, then removes them.</summary>
    [Benchmark(Description = "10x Register/Dispose")]
    public void RegisterAndDispose()
    {
        var registrations = new IDisposable[RegistrationCount];

        for (var i = 0; i < RegistrationCount; i++)
        {
            registrations[i] = _interaction.RegisterHandler(static context => context.SetOutput(0));
        }

        for (var i = 0; i < RegistrationCount; i++)
        {
            registrations[i].Dispose();
        }
    }
}
