// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias analyzer;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Call sites that name a type generated code cannot name: an anonymous type, a private or protected nested type, or a
/// type built from one. A build that intercepts claims them with generic interceptors. Every other build fails at the
/// call with RXUIBIND015 as an error, and no generated file names the type.
/// </summary>
public class UnnameableTypeCallSiteTests
{
    /// <summary>The interception attribute a claimed call site carries.</summary>
    private const string InterceptsAttribute = "InterceptsLocation(";

    /// <summary>The dispatch file WhenAnyValue generates into.</summary>
    private const string WhenAnyValueDispatch = "WhenAnyValueDispatch.g.cs";

    /// <summary>Why a test only applies to a build that can intercept.</summary>
    private const string NeedsInterception = "Only a build against Roslyn 4.13 or newer can intercept a call site.";

    /// <summary>Gets every scenario, with the dispatch file its API generates into.</summary>
    /// <returns>The scenario name and the dispatch file name.</returns>
    public static IEnumerable<Func<(string Scenario, string DispatchFile)>> Scenarios()
    {
        yield return static () => (nameof(UnnameableTypeScenarios.AnonymousWhenAnyValueResult), WhenAnyValueDispatch);
        yield return static () => (nameof(UnnameableTypeScenarios.PrivateWhenAnyValueResult), WhenAnyValueDispatch);
        yield return static () => (nameof(UnnameableTypeScenarios.PrivateTypeArgumentResult), WhenAnyValueDispatch);
        yield return static () => (nameof(UnnameableTypeScenarios.PrivateTupleResult), WhenAnyValueDispatch);
        yield return static () => (nameof(UnnameableTypeScenarios.AnonymousWhenAnyResult), "WhenAnyDispatch.g.cs");
        yield return static () => (nameof(UnnameableTypeScenarios.AnonymousWhenChangedResult), "WhenChangedDispatch.g.cs");
        yield return static () => (nameof(UnnameableTypeScenarios.AnonymousWhenChangingResult), "WhenChangingDispatch.g.cs");
        yield return static () => (nameof(UnnameableTypeScenarios.AnonymousWhenAnyObservableResult), "WhenAnyObservableDispatch.g.cs");
        yield return static () => (nameof(UnnameableTypeScenarios.HostedPrivateSource), WhenAnyValueDispatch);
        yield return static () => (nameof(UnnameableTypeScenarios.HostedPrivateBindCommand), "BindCommandDispatch.g.cs");
        yield return static () => (nameof(UnnameableTypeScenarios.HostedEveryApi), "ToPropertyDispatch.g.cs");
    }

    /// <summary>Gets the dispatch file of every API the hosted scenario calls.</summary>
    /// <returns>The dispatch file names.</returns>
    public static IEnumerable<string> HostedApiDispatchFiles()
    {
        yield return "WhenChangedDispatch.g.cs";
        yield return "WhenChangingDispatch.g.cs";
        yield return WhenAnyValueDispatch;
        yield return "WhenAnyDispatch.g.cs";
        yield return "WhenAnyObservableDispatch.g.cs";
        yield return "BindOneWayDispatch.g.cs";
        yield return "BindTwoWayDispatch.g.cs";
        yield return "OneWayBindDispatch.g.cs";
        yield return "BindDispatch.g.cs";
        yield return "BindToDispatch.g.cs";
        yield return "InvokeCommandDispatch.g.cs";
        yield return "BindInteractionDispatch.g.cs";
        yield return "ToPropertyDispatch.g.cs";
    }

    /// <summary>Every API claims its call between private nested types from the caller's partial class.</summary>
    /// <param name="dispatchFile">The dispatch file of one API.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(HostedApiDispatchFiles))]
    public async Task InterceptingBuild_ClaimsEveryHostedApi(string dispatchFile)
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.HostedEveryApi, true);

        await result.GeneratedSourceContains(dispatchFile, InterceptsAttribute);
        await result.GeneratedSourceContains(dispatchFile, "private static partial class __ReactiveUIHostedBindings");
    }

    /// <summary>
    /// Calls no build can claim: a private type whose declaring class is not partial, a file-local type, a private type
    /// in a file-local class, a call written through the stub's declaring class, and a scheduler overload declared in
    /// an extension block.
    /// </summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task UnclaimableCall_ReportsTheTypeAsAnError(
        [Matrix(
            nameof(UnnameableTypeScenarios.UnhostablePrivateSource),
            nameof(UnnameableTypeScenarios.FileLocalSource),
            nameof(UnnameableTypeScenarios.FileLocalHost),
            nameof(UnnameableTypeScenarios.HostedStaticFormCall),
            nameof(UnnameableTypeScenarios.HostedSchedulerOverload))] string scenario,
        [Matrix(false, true)] bool intercept) =>
        AssertReportedAsError(UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), intercept));

    /// <summary>
    /// A private type nested in a generic class is built from that class's type parameter, so no build hosts the call.
    /// RXUIBIND016 reports it, and no generated file names the type.
    /// </summary>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task PrivateTypeInAGenericClass_ReportsATypeParameterCall(bool intercept)
    {
        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.GenericHost, intercept);
        var diagnostics = await result.OutputCompilation
            .WithAnalyzers([new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.UnreachableTypeAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        await Assert.That(UnnameableTypeScenarios.GeneratedCodeErrors(result)).IsEmpty();
        await Assert.That(diagnostics.Select(static d => d.Id)).IsEquivalentTo(["RXUIBIND016"]);
    }

    /// <summary>A build that intercepts claims the call site, and the output compiles and emits.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="dispatchFile">The dispatch file the call site's API generates into.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task InterceptingBuild_ClaimsTheCallSite(string scenario, string dispatchFile)
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), true);
        var emitErrors = result.OutputCompilation.Emit(Stream.Null).Diagnostics
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .Select(static d => $"{d.Id}: {d.GetMessage()}")
            .ToArray();

        await result.CompilationSucceeds();
        await Assert.That(emitErrors).IsEmpty();
        await result.GeneratedSourceContains(dispatchFile, InterceptsAttribute);
    }

    /// <summary>A call site an interceptor claims reports neither RXUIBIND015 nor RXUIBIND021.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="dispatchFile">The dispatch file the call site's API generates into.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task InterceptingBuild_ReportsNothing(string scenario, string dispatchFile)
    {
        _ = dispatchFile;
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var diagnostics = await AnalyzeAsync(UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), true));

        await Assert.That(diagnostics.Select(static d => d.Id)).IsEmpty();
    }

    /// <summary>
    /// A build that does not intercept fails at the call with RXUIBIND015 as an error, and generates nothing that names
    /// the type.
    /// </summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="dispatchFile">The dispatch file the call site's API generates into.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task BuildWithoutInterception_ReportsTheTypeAsAnError(string scenario, string dispatchFile)
    {
        _ = dispatchFile;
        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), false);

        await AssertReportedAsError(result);
    }

    /// <summary>The baseline build cannot intercept even when the project opts in, so it reports the call as an error.</summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <param name="dispatchFile">The dispatch file the call site's API generates into.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task BaselineBuild_OptedIn_ReportsTheTypeAsAnError(string scenario, string dispatchFile)
    {
        _ = dispatchFile;
        if (InterceptableLocationReader.IsSupported)
        {
            Skip.Test("A build against Roslyn 4.13 or newer intercepts the call site instead.");
        }

        await AssertReportedAsError(UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), true));
    }

    /// <summary>
    /// The code written for a call site matches its snapshot: a generic interceptor for an anonymous or private result,
    /// code moved into the caller's partial class for a private view model, and code generic over the caller's type
    /// parameters for a generic view base.
    /// </summary>
    /// <param name="scenario">The name of the scenario.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(nameof(UnnameableTypeScenarios.AnonymousWhenAnyValueResult))]
    [Arguments(nameof(UnnameableTypeScenarios.PrivateWhenAnyValueResult))]
    [Arguments(nameof(UnnameableTypeScenarios.HostedPrivateSource))]
    [Arguments(nameof(UnnameableTypeScenarios.HostedPrivateBindCommand))]
    [Arguments(nameof(UnnameableTypeScenarios.TypeParameterViewBase))]
    public async Task InterceptingBuild_MatchesTheSnapshot(string scenario)
    {
        if (!InterceptableLocationReader.IsSupported)
        {
            Skip.Test(NeedsInterception);
        }

        var result = UnnameableTypeScenarios.Generate(UnnameableTypeScenarios.Named(scenario), true);

        await GeneratorSnapshot.VerifyAsync(result.Driver, "UTCS", scenario);
    }

    /// <summary>Asserts that every call is reported as an RXUIBIND015 error and that no generated file fails to compile.</summary>
    /// <param name="result">The generator result.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertReportedAsError(GeneratorTestResult result)
    {
        var diagnostics = await AnalyzeAsync(result);

        await Assert.That(UnnameableTypeScenarios.GeneratedCodeErrors(result)).IsEmpty();
        await Assert.That(diagnostics).IsNotEmpty();
        await Assert.That(diagnostics.Select(static d => $"{d.Id} {d.Severity}").Distinct())
            .IsEquivalentTo([$"{UnnameableTypeScenarios.UnreachableTypeId} {DiagnosticSeverity.Error}"]);
    }

    /// <summary>Runs the analyzers that report an ungenerated call over what the generator produced.</summary>
    /// <param name="result">The generator result.</param>
    /// <returns>The RXUIBIND015 and RXUIBIND021 diagnostics.</returns>
    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(GeneratorTestResult result)
    {
        var diagnostics = await result.OutputCompilation
            .WithAnalyzers(
            [
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.UnreachableTypeAnalyzer(),
                new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.NoGeneratedBindingAnalyzer(),
            ])
            .GetAnalyzerDiagnosticsAsync();

        return
        [
            .. diagnostics.Where(static d => d.Id is UnnameableTypeScenarios.UnreachableTypeId or UnnameableTypeScenarios.NoGeneratedBindingId),
        ];
    }
}
