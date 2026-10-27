// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers generating against the System.Reactive flavour of the runtime library. It shares no type names with
/// the lean one, so output written for the lean package does not compile against it at all - and a single
/// runtime type the retargeting misses is enough to break every consumer on that package.
/// </summary>
/// <remarks>
/// The sweep runs every shared scenario rather than a sample, because the failure mode is one unshifted type
/// name in one emitter, which no representative subset can be trusted to reach.
/// </remarks>
public class ReactiveRuntimeFlavourTests
{
    /// <summary>The import a scenario carries for the lean runtime package.</summary>
    private const string LeanImport = "using ReactiveUI.Binding;";

    /// <summary>The import that reaches the same API on the System.Reactive package.</summary>
    private const string ReactiveImport = "using ReactiveUI.Binding.Reactive;";

    /// <summary>The import a scenario carries for the lean scheduler abstraction.</summary>
    private const string LeanSchedulerImport = "using ReactiveUI.Primitives.Concurrency;";

    /// <summary>
    /// The alias that maps the scheduler name onto the System.Reactive one, mirroring what the build does for
    /// the runtime library's own source.
    /// </summary>
    private const string ReactiveSchedulerImport = "using ISequencer = System.Reactive.Concurrency.IScheduler;";

    /// <summary>A qualified runtime namespace as a scenario body writes it, without the global alias.</summary>
    private const string LeanQualifiedObservables = "ReactiveUI.Binding.Observables.";

    /// <summary>The same namespace on the System.Reactive package.</summary>
    private const string ReactiveQualifiedObservables = "ReactiveUI.Binding.Reactive.Observables.";

    /// <summary>The import a scenario carries for ReactiveUI's <c>ReactiveObject</c> and its raise extensions.</summary>
    private const string LeanReactiveUIImport = "using ReactiveUI;";

    /// <summary>The same import plus the namespace ReactiveUI.Reactive declares them in.</summary>
    private const string ReactiveReactiveUIImport = "using ReactiveUI;\nusing ReactiveUI.Reactive;";

    /// <summary>ReactiveUI's <c>ReactiveObject</c> as a scenario body names it outright.</summary>
    private const string LeanQualifiedReactiveObject = "ReactiveUI.ReactiveObject";

    /// <summary>The same type on ReactiveUI.Reactive.</summary>
    private const string ReactiveQualifiedReactiveObject = "ReactiveUI.Reactive.ReactiveObject";

    /// <summary>Lists the shared scenarios for the sweep.</summary>
    /// <returns>Every shared scenario path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> Scenarios() => SharedSourceReader.EnumerateScenarioPaths();

    /// <summary>Generated output for a System.Reactive consumer compiles against that package.</summary>
    /// <param name="scenarioPath">The shared scenario to generate from.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodDataSource(nameof(Scenarios))]
    public async Task Scenario_GeneratesCodeThatCompilesAgainstTheSystemReactiveRuntime(string scenarioPath)
    {
        var source = ShiftToReactiveRuntime(SharedSourceReader.ReadScenario(scenarioPath));
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);

        await result.CompilationSucceeds();
    }

    /// <summary>The retargeting names the System.Reactive types and leaves nothing pointing at the lean ones.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveRuntime_NamesTheShiftedTypesAndNoLeanOnes()
    {
        var source = ShiftToReactiveRuntime(SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC"));
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10, null, true);

        await result.CompilationSucceeds();

        var dispatch = result.GeneratedSources["WhenChangedDispatch.g.cs"];

        await Assert.That(dispatch).Contains("global::ReactiveUI.Binding.Reactive.Observables.");
        await Assert.That(dispatch).DoesNotContain("global::ReactiveUI.Binding.Observables.");
    }

    /// <summary>A lean consumer's output is untouched by the retargeting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LeanRuntime_NamesTheLeanTypes()
    {
        var source = SharedSourceReader.ReadScenario("WhenChanged/SinglePropertyINPC");
        var result = TestHelper.RunGenerator(source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();

        var dispatch = result.GeneratedSources["WhenChangedDispatch.g.cs"];

        await Assert.That(dispatch).Contains("global::ReactiveUI.Binding.Observables.");
        await Assert.That(dispatch).DoesNotContain("global::ReactiveUI.Binding.Reactive.");
    }

    /// <summary>
    /// Points a scenario at the other runtime package. The imports move - the scheduler one becomes an alias so
    /// the scenario bodies can keep naming <c>ISequencer</c>, which is exactly the seam the runtime library's own
    /// build uses to compile one source tree against both packages - and so does the one scenario that names a
    /// runtime type outright rather than relying on its import. ReactiveUI's <c>ReactiveObject</c> moves to
    /// ReactiveUI.Reactive, the package a System.Reactive consumer references.
    /// </summary>
    /// <param name="source">The scenario source, written against the lean package.</param>
    /// <returns>The same source, importing the System.Reactive package.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ShiftToReactiveRuntime(string source) =>
        source
            .Replace(LeanSchedulerImport, ReactiveSchedulerImport, StringComparison.Ordinal)
            .Replace(LeanImport, ReactiveImport, StringComparison.Ordinal)
            .Replace(LeanQualifiedObservables, ReactiveQualifiedObservables, StringComparison.Ordinal)
            .Replace(LeanReactiveUIImport, ReactiveReactiveUIImport, StringComparison.Ordinal)
            .Replace(LeanQualifiedReactiveObject, ReactiveQualifiedReactiveObject, StringComparison.Ordinal);
}
