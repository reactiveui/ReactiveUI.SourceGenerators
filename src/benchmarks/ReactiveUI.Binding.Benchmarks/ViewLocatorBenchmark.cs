// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Mocks;
using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Resolving a view through the generated view lookups registered with the default view locator.</summary>
/// <remarks>
/// Each benchmark registers its own lookups, and BenchmarkDotNet runs each benchmark in its own process, so
/// the registrations never meet. Lookups are consulted from the most recently registered to the first.
/// </remarks>
public class ViewLocatorBenchmark
{
    /// <summary>How many resolutions each benchmark performs per invocation.</summary>
    private const int ResolveCount = 1_000;

    /// <summary>The view a registered lookup returns.</summary>
    private readonly ViewLocatorBenchmarkView _view = new();

    /// <summary>The locator that resolves views.</summary>
    private readonly DefaultViewLocator _locator = new();

    /// <summary>The view model resolved when one lookup is registered.</summary>
    private readonly ViewLocatorBenchmarkViewModel _singleViewModel = new();

    /// <summary>The view model whose lookup is the first one consulted of three.</summary>
    private readonly ViewLocatorBenchmarkViewModel _firstOfThreeViewModel = new();

    /// <summary>The view model whose lookup is the last one consulted of three.</summary>
    private readonly ViewLocatorBenchmarkViewModel _lastOfThreeViewModel = new();

    /// <summary>The view model no registered lookup resolves.</summary>
    private readonly ViewLocatorBenchmarkViewModel _unresolvedViewModel = new();

    /// <summary>The view model this assembly's own generated lookup resolves.</summary>
    private readonly BenchmarkViewModel _generatedViewModel = new();

    /// <summary>Registers the runtime services a generated lookup and the service locator fallback resolve through.</summary>
    [GlobalSetup]
    public static void Register()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Registers one lookup that resolves <see cref="ViewLocatorBenchmarkViewModel"/>.</summary>
    [GlobalSetup(Target = nameof(SingleLookupHit))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RegisterSingleLookup() => RegisterResolvingLookup();

    /// <summary>Registers two lookups that resolve other view models, then one that resolves <see cref="ViewLocatorBenchmarkViewModel"/>.</summary>
    [GlobalSetup(Target = nameof(HitInFirstOfThreeLookups))]
    public void RegisterHitInFirstOfThree()
    {
        RegisterBoundViewModelLookup();
        RegisterNativeAdapterCommandLookup();
        RegisterResolvingLookup();
    }

    /// <summary>Registers a lookup that resolves <see cref="ViewLocatorBenchmarkViewModel"/>, then two that resolve other view models.</summary>
    [GlobalSetup(Target = nameof(HitInLastOfThreeLookups))]
    public void RegisterHitInLastOfThree()
    {
        RegisterResolvingLookup();
        RegisterBoundViewModelLookup();
        RegisterNativeAdapterCommandLookup();
    }

    /// <summary>Registers three lookups, none of which resolves <see cref="ViewLocatorBenchmarkViewModel"/>.</summary>
    [GlobalSetup(Target = nameof(MissInEveryLookup))]
    public void RegisterMissInEveryLookup()
    {
        RegisterBoundViewModelLookup();
        RegisterNativeAdapterCommandLookup();
        DefaultViewLocator.SetGeneratedViewDispatch(static (viewModel, _) => viewModel is BenchmarkChildViewModel ? new BenchmarkView() : null);
    }

    /// <summary>The single registered lookup answers.</summary>
    /// <returns>The last view resolved.</returns>
    [Benchmark(Description = "Single lookup hit", OperationsPerInvoke = ResolveCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? SingleLookupHit() => ResolveRepeatedly(_singleViewModel);

    /// <summary>The lookup registered last answers, so it is the first one asked.</summary>
    /// <returns>The last view resolved.</returns>
    [Benchmark(Description = "Hit in first of three lookups", OperationsPerInvoke = ResolveCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? HitInFirstOfThreeLookups() => ResolveRepeatedly(_firstOfThreeViewModel);

    /// <summary>The lookup registered first answers, so it is the last one asked.</summary>
    /// <returns>The last view resolved.</returns>
    [Benchmark(Description = "Hit in last of three lookups", OperationsPerInvoke = ResolveCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? HitInLastOfThreeLookups() => ResolveRepeatedly(_lastOfThreeViewModel);

    /// <summary>No lookup answers, so the locator asks all three and then tries its mappings and the service locator.</summary>
    /// <returns>The last view resolved.</returns>
    [Benchmark(Description = "Miss in every lookup", OperationsPerInvoke = ResolveCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? MissInEveryLookup() => ResolveRepeatedly(_unresolvedViewModel);

    /// <summary>The generated lookup of this assembly, registered when the module loaded, resolves the view.</summary>
    /// <returns>The last view resolved.</returns>
    [Benchmark(Description = "Assembly's generated lookup", OperationsPerInvoke = ResolveCount)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IViewFor? AssemblyGeneratedLookup() => ResolveRepeatedly(_generatedViewModel);

    /// <summary>Registers a lookup that resolves <see cref="BoundViewModel"/> only.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RegisterBoundViewModelLookup() =>
        DefaultViewLocator.SetGeneratedViewDispatch(static (viewModel, _) => viewModel is BoundViewModel ? new BoundView() : null);

    /// <summary>Registers a lookup that resolves <see cref="NativeAdapterCommand"/> only.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RegisterNativeAdapterCommandLookup() =>
        DefaultViewLocator.SetGeneratedViewDispatch(static (viewModel, _) => viewModel is NativeAdapterCommand ? new NativeAdapterView() : null);

    /// <summary>Registers a lookup that resolves <see cref="ViewLocatorBenchmarkViewModel"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RegisterResolvingLookup() =>
        DefaultViewLocator.SetGeneratedViewDispatch((viewModel, _) => viewModel is ViewLocatorBenchmarkViewModel ? _view : null);

    /// <summary>Resolves a view for the view model <see cref="ResolveCount"/> times.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="viewModel">The view model to resolve a view for.</param>
    /// <returns>The last view resolved.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IViewFor? ResolveRepeatedly<TViewModel>(TViewModel viewModel)
        where TViewModel : class
    {
        IViewFor? last = null;
        for (var i = 0; i < ResolveCount; i++)
        {
            last = _locator.ResolveView(viewModel);
        }

        return last;
    }
}
