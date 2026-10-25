// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// A binding call that no generated code claims would throw when it runs, so the build fails at the call instead. A
/// call a build can claim with an interceptor is claimed, and reports nothing.
/// </summary>
public class UnclaimedCallSiteTests
{
    /// <summary>Why a test only applies to a build that can intercept.</summary>
    private const string NeedsInterception = "Only a build against Roslyn 4.13 or newer can intercept a call site.";

    /// <summary>The values a one-property observation delivers: the start, then one change.</summary>
    private static readonly int[] OnePropertyValues = [1, 3];

    /// <summary>A call no build can claim fails the build with the error that names its cause.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="expectedId">The error the call reports.</param>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(nameof(UnnameableTypeScenarios.UnhostablePrivateMember), "RXUIBIND003", false)]
    [Arguments(nameof(UnnameableTypeScenarios.UnhostablePrivateMember), "RXUIBIND003", true)]
    [Arguments(nameof(UnnameableTypeScenarios.ComputedPath), "RXUIBIND021", false)]
    [Arguments(nameof(UnnameableTypeScenarios.ComputedPath), "RXUIBIND021", true)]
    [Arguments(nameof(UnnameableTypeScenarios.StaticFormCall), "RXUIBIND021", false)]
    [Arguments(nameof(UnnameableTypeScenarios.StaticFormCall), "RXUIBIND021", true)]
    [Arguments(nameof(UnnameableTypeScenarios.StoredPath), "RXUIBIND001", false)]
    [Arguments(nameof(UnnameableTypeScenarios.StoredPath), "RXUIBIND001", true)]
    [Arguments(nameof(UnnameableTypeScenarios.StoredCommandParameter), "RXUIBIND001", false)]
    [Arguments(nameof(UnnameableTypeScenarios.StoredCommandParameter), "RXUIBIND001", true)]
    public async Task UnclaimableCall_FailsTheBuild(string scenario, string expectedId, bool intercept)
    {
        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), intercept);
        var errors = await CallSiteDiagnostics.ErrorsAsync(result);

        await Assert.That(UnnameableTypeScenarios.GeneratedCodeErrors(result)).IsEmpty();
        await Assert.That(errors).Contains(expectedId);
    }

    /// <summary>A private property read inside its partial declaring class is claimed, and reports nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HostedPrivateMember_ReportsNothing()
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.HostedPrivateMember, true);

        await Assert.That(await CallSiteDiagnostics.ErrorsAsync(result)).IsEmpty();
    }

    /// <summary>A private property read inside its partial declaring class delivers every value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HostedPrivateMember_DeliversEveryValue()
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.HostedPrivateMember, true);
        await result.CompilationSucceeds();
        await Assert.That(CallSiteDiagnostics.RunScenario(result)).IsEquivalentTo(OnePropertyValues);
    }

    /// <summary>A build that cannot intercept reports the private property as an error.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HostedPrivateMember_WithoutInterception_FailsTheBuild() =>
        await Assert.That(await CallSiteDiagnostics.ErrorsAsync(UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.HostedPrivateMember, false)))
            .Contains("RXUIBIND003");
}
