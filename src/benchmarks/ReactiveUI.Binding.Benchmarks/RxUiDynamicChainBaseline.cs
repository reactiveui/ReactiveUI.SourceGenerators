// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Builder;
using BenchmarkVm = ReactiveUI.Binding.Benchmarks.Mocks.BenchmarkViewModel;

// Outside ReactiveUI.Binding so extension lookup reaches ReactiveUI's overloads; the view model comes in by alias,
// since importing its namespace would make every call ambiguous.
namespace RxUiDynamicChain;

/// <summary>The dynamic-chain scenarios of <c>WhenAnyDynamicBenchmark</c>, run against ReactiveUI's own engine.</summary>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
#endif
public class RxUiDynamicChainBaseline
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>Names the chain reaching one property.</summary>
    private static readonly Expression NameChain = Chain(x => x.Name);

    /// <summary>Names the chain reaching a second property, so an arity above one has something to combine.</summary>
    private static readonly Expression AgeChain = Chain(x => x.Age);

    /// <summary>Names the chain reaching through an intermediate.</summary>
    private static readonly Expression ChildValueChain = Chain(x => x.Child.Value);

    /// <summary>Reads one observed change as ReactiveUI's type, so a call resolving to this library's overload does not compile.</summary>
    private static readonly Func<IObservedChange<BenchmarkVm?, object?>, object?> ReadOne =
        static c1 => c1.Value;

    /// <summary>Reads whichever of two observed changes carries a value.</summary>
    private static readonly Func<IObservedChange<BenchmarkVm?, object?>, IObservedChange<BenchmarkVm?, object?>, object?> ReadEither =
        static (c1, c2) => c1.Value ?? c2.Value;

    /// <summary>The view model instance used for observation benchmarks.</summary>
    private BenchmarkVm _vm = null!;

    /// <summary>Registers the observation plugins ReactiveUI resolves each link through.</summary>
    [GlobalSetup]
    public static void Register()
    {
        var builder = RxAppBuilder.CreateReactiveUIBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Sets up a fresh view model before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>One chain: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Single Chain")]
    public void SingleChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, ReadOne)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Two chains combined: subscribe, fire N changes on each, dispose.</summary>
    [Benchmark(Description = "Two Chains")]
    public void TwoChains()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, AgeChain, ReadEither)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>A chain through an intermediate: subscribe, fire N changes on the leaf, dispose.</summary>
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(ChildValueChain, ReadOne)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>Cold start: subscribe, read the initial value, dispose. No property changes.</summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "First Observation")]
    public object? FirstObservation()
    {
        object? result = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, ReadOne)
            .Subscribe(v => result = v);
        return result;
    }

    /// <summary>Names a property chain the way a caller building one at run time hands it over.</summary>
    /// <typeparam name="TValue">The type the chain ends at.</typeparam>
    /// <param name="property">The chain to name.</param>
    /// <returns>The expression body, which is what these overloads take.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression Chain<TValue>(Expression<Func<BenchmarkVm, TValue>> property) =>
        property.Body;
}
