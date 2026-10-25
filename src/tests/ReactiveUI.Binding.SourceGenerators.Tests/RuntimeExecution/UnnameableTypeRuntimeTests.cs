// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.RuntimeExecution;

/// <summary>
/// Runs call sites whose types generated code cannot name, through the generic interceptors a build against Roslyn 4.13
/// or newer writes for them, and checks the values that arrive.
/// </summary>
public class UnnameableTypeRuntimeTests
{
    /// <summary>The values a two-property projection delivers: the start, then a change to each property.</summary>
    private static readonly int[] TwoPropertyValues = [12, 32, 34];

    /// <summary>The values a one-property projection delivers: the start, then one change.</summary>
    private static readonly int[] OnePropertyValues = [1, 3];

    /// <summary>The values a tuple projection delivers: each value added to its double.</summary>
    private static readonly int[] TupleValues = [3, 9];

    /// <summary>The sums a two-property conversion delivers: the start, then one change.</summary>
    private static readonly int[] SumValues = [3, 5];

    /// <summary>The values the hosted scenario returns when every API delivered, in the order it lists them.</summary>
    private static readonly int[] EveryApiValues = [2, 2, 5, 1, 2, 2, 7, 1, 1, 1, 4, 4];

    /// <summary>A single count of one.</summary>
    private static readonly int[] OnceValues = [1];

    /// <summary>The values a combined observable delivers once both have produced a value.</summary>
    private static readonly int[] CombinedValues = [12, 32];

    /// <summary>An anonymous WhenAnyValue result arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnonymousWhenAnyValueResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.AnonymousWhenAnyValueResult)).IsEquivalentTo(TwoPropertyValues);

    /// <summary>A private nested WhenAnyValue result arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PrivateWhenAnyValueResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.PrivateWhenAnyValueResult)).IsEquivalentTo(OnePropertyValues);

    /// <summary>A list closed over a protected nested type arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PrivateTypeArgumentResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.PrivateTypeArgumentResult)).IsEquivalentTo(OnePropertyValues);

    /// <summary>A tuple holding a private nested type arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PrivateTupleResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.PrivateTupleResult)).IsEquivalentTo(TupleValues);

    /// <summary>An anonymous WhenAny result arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnonymousWhenAnyResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.AnonymousWhenAnyResult)).IsEquivalentTo(OnePropertyValues);

    /// <summary>An anonymous WhenChanged conversion arrives for the start and each change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnonymousWhenChangedResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.AnonymousWhenChangedResult)).IsEquivalentTo(SumValues);

    /// <summary>An anonymous WhenChanging conversion delivers the same values as the call with a named result.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnonymousWhenChangingResult_MatchesTheNamedResult()
    {
        var named = await RunAsync(UnnameableTypeScenarios.NamedWhenChangingResult);

        await Assert.That(named).IsNotEmpty();
        await Assert.That(await RunAsync(UnnameableTypeScenarios.AnonymousWhenChangingResult)).IsEquivalentTo(named);
    }

    /// <summary>An anonymous WhenAnyObservable result arrives once both observables have produced a value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnonymousWhenAnyObservableResult_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.AnonymousWhenAnyObservableResult)).IsEquivalentTo(CombinedValues);

    /// <summary>A private nested view model observed inside its partial declaring class delivers every value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostedPrivateSource_DeliversEveryValue() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.HostedPrivateSource)).IsEquivalentTo(OnePropertyValues);

    /// <summary>A command bound between private nested types runs once while bound and not after the binding ends.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostedPrivateBindCommand_ExecutesWhileBound() =>
        await Assert.That(await RunAsync(UnnameableTypeScenarios.HostedPrivateBindCommand)).IsEquivalentTo(OnceValues);

    /// <summary>Every API, called between private nested types inside their partial declaring class, delivers its values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostedEveryApi_DeliversEveryValue() =>
        await Assert.That(string.Join(",", await RunAsync(UnnameableTypeScenarios.HostedEveryApi))).IsEqualTo(string.Join(",", EveryApiValues));

    /// <summary>Generates a scenario with interception, runs it, and returns the values it recorded.</summary>
    /// <param name="scenario">The scenario's <c>Scenario</c> type.</param>
    /// <returns>The values the call delivered.</returns>
    private static async Task<int[]> RunAsync(string scenario)
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test("Only a build against Roslyn 4.13 or newer can intercept a call site.");
        }

        var result = UnnameableTypeScenarios.Generate(scenario, true);
        await result.CompilationSucceeds();

        var (assembly, context) = TestHelper.EmitAndLoad(result);
        try
        {
            return (int[])assembly.GetType(UnnameableTypeScenarios.ScenarioTypeName)!.GetMethod("Run")!.Invoke(null, null)!;
        }
        finally
        {
            context.Unload();
        }
    }
}
