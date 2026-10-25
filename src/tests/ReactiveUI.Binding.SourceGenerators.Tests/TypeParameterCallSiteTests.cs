// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Calls built from the calling code's type parameters. A build that intercepts claims a call when each of those type
/// parameters is one of the called method's own type arguments, with generated code generic over them. Every other
/// such call fails the build with RXUIBIND016.
/// </summary>
public class TypeParameterCallSiteTests
{
    /// <summary>Why a test only applies to a build that can intercept.</summary>
    private const string NeedsInterception = "Only a build against Roslyn 4.13 or newer can intercept a call site.";

    /// <summary>The error reported for a call built from a type parameter that no generated code claims.</summary>
    private const string TypeParameterCallId = "RXUIBIND016";

    /// <summary>The values a one-property observation delivers: the start, then one change.</summary>
    private static readonly int[] OnePropertyValues = [1, 3];

    /// <summary>The values a doubling projection delivers: the start, then one change.</summary>
    private static readonly int[] DoubledValues = [2, 6];

    /// <summary>The value a one-way binding wrote after the change.</summary>
    private static readonly int[] BoundValues = [3];

    /// <summary>The values the every-API scenario returns, in the order it lists them.</summary>
    private static readonly int[] EveryApiValues = [2, 2, 5, 1, 2, 2, 7, 2, 1, 1];

    /// <summary>Gets the scenarios a build that intercepts claims, with the values each delivers.</summary>
    /// <returns>The scenario name and its values.</returns>
    public static IEnumerable<Func<(string Scenario, int[] Values)>> Claimable()
    {
        yield return static () => (nameof(UnnameableTypeScenarios.TypeParameterReceiver), OnePropertyValues);
        yield return static () => (nameof(UnnameableTypeScenarios.TypeParameterValue), OnePropertyValues);
        yield return static () => (nameof(UnnameableTypeScenarios.TypeParameterResult), DoubledValues);
        yield return static () => (nameof(UnnameableTypeScenarios.TypeParameterViewBase), BoundValues);
        yield return static () => (nameof(UnnameableTypeScenarios.TypeParameterEveryApi), EveryApiValues);
    }

    /// <summary>A build that intercepts claims the call, compiles, reports nothing, and delivers every value.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="values">The values the scenario delivers.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Claimable))]
    public async Task InterceptingBuild_ClaimsAndRunsTheCall(string scenario, int[] values)
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), true);

        await result.CompilationSucceeds();
        await Assert.That(await CallSiteDiagnostics.ErrorsAsync(result)).IsEmpty();
        await Assert.That(CallSiteDiagnostics.RunScenario(result)).IsEquivalentTo(values);
    }

    /// <summary>A build that does not intercept fails at the call with RXUIBIND016, and generates nothing broken.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="values">The values the scenario would deliver.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Claimable))]
    public async Task BuildWithoutInterception_FailsTheBuild(string scenario, int[] values)
    {
        _ = values;
        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), false);

        await Assert.That(UnnameableTypeScenarios.GeneratedCodeErrors(result)).IsEmpty();
        await Assert.That(await CallSiteDiagnostics.ErrorsAsync(result)).Contains(TypeParameterCallId);
    }

    /// <summary>
    /// A type parameter the called method's type arguments do not carry, directly or through a constraint, cannot be
    /// handed on, so every build fails.
    /// </summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task UnmappableTypeParameter_FailsTheBuild(
        [Matrix(
            nameof(UnnameableTypeScenarios.UnmappableTypeParameter),
            nameof(UnnameableTypeScenarios.TypeParameterInsideArray),
            nameof(UnnameableTypeScenarios.ConstraintNamesAnotherTypeParameter))] string scenario,
        [Matrix(false, true)] bool intercept)
    {
        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), intercept);

        await Assert.That(UnnameableTypeScenarios.GeneratedCodeErrors(result)).IsEmpty();
        await Assert.That(await CallSiteDiagnostics.ErrorsAsync(result)).Contains(TypeParameterCallId);
    }
}
