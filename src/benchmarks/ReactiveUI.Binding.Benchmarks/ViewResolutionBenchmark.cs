// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>View resolution through the generated view dispatch, beside a generated binding call in the same assembly.</summary>
/// <remarks>
/// <see cref="BenchmarkView"/> implements <c>IViewFor&lt;BenchmarkViewModel&gt;</c>, so this assembly carries a
/// generated view dispatch next to every generated binding. The dispatch lives in the same class as the binding
/// methods, so <see cref="GeneratedBindingCall"/> reads the cost of a binding call in an assembly that also
/// registers a view lookup. Setup finds the generated class by reflection, so no NativeAOT job is declared.
/// </remarks>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Finds the generated class by reflection; members may be trimmed.")]
#endif
public class ViewResolutionBenchmark
{
    /// <summary>The name of the class the generator declares its dispatch and bindings in.</summary>
    private const string GeneratedClassName = "__ReactiveUIGeneratedBindings";

    /// <summary>The locator under measurement.</summary>
    private DefaultViewLocator _locator = null!;

    /// <summary>A view model the generated dispatch maps to a view.</summary>
    private BenchmarkViewModel _mapped = null!;

    /// <summary>A view model the generated dispatch has no view for.</summary>
    private BenchmarkChildViewModel _unmapped = null!;

    /// <summary>Puts the generated view dispatch in place and checks it resolves a view before anything is timed.</summary>
    /// <exception cref="InvalidOperationException">The generated dispatch did not resolve the mapped view model.</exception>
    [GlobalSetup]
    public void Setup()
    {
        RunGeneratedClassInitialization();

        _locator = new();
        _mapped = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };
        _unmapped = new();

        if (_locator.ResolveView(_mapped, null) is not BenchmarkView)
        {
            throw new InvalidOperationException("The generated view dispatch did not resolve a view for the mapped view model.");
        }
    }

    /// <summary>Resolves a view the generated dispatch maps: type switch, service locator probe, then construction.</summary>
    /// <returns>The resolved view.</returns>
    [Benchmark(Description = "Resolve Generated View")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? ResolveGeneratedView() => _locator.ResolveView(_mapped, null);

    /// <summary>Resolves a view model with no view: the generated switch misses, then the mappings and service locator are tried.</summary>
    /// <returns>Always <see langword="null"/>.</returns>
    [Benchmark(Description = "Resolve Unmapped View Model")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? ResolveUnmappedViewModel() => _locator.ResolveView(_unmapped, null);

    /// <summary>Creates and disposes one generated observation in an assembly that also carries a view dispatch.</summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "Generated Binding Call")]
    public string GeneratedBindingCall()
    {
        var result = string.Empty;
        using var sub = _mapped.WhenChanged(x => x.Name)
            .Subscribe(v => result = v);
        return result;
    }

    /// <summary>Runs the static initialization of the generated class, so a lookup that registers when the class loads is in place.</summary>
    private static void RunGeneratedClassInitialization()
    {
        foreach (var type in typeof(BenchmarkView).Assembly.GetTypes())
        {
            if (string.Equals(type.Name, GeneratedClassName, StringComparison.Ordinal))
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
        }
    }
}
