// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Helpers;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// The same repeated call sites, generated for a build that claims each one by name. Collapsing a group to one
/// call site per distinct pair of selectors is right where dispatch keys on that text, because the later
/// branches are unreachable - but an interceptor names the call site it replaces, so a dropped one carries no
/// attribute and takes the runtime engine while its twin is generated.
/// </summary>
public partial class RepeatedCallSiteDispatchTests
{
    /// <summary>The attribute text that claims one call site.</summary>
    private const string InterceptsAttribute = "InterceptsLocation(";

    /// <summary>The number of call sites each repeated scenario writes.</summary>
    private const int CallSitesPerScenario = 2;

    /// <summary>Both BindOneWay call sites are claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RepeatedCallSites_UnderInterception_ClaimEveryCallSite() =>
        AssertEveryCallSiteIsClaimed(RepeatedBindingSource, DispatchFileName);

    /// <summary>Both BindTo call sites are claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RepeatedBindTo_UnderInterception_ClaimsEveryCallSite() =>
        AssertEveryCallSiteIsClaimed(RepeatedBindToSource, "BindToDispatch.g.cs");

    /// <summary>Both BindCommand call sites are claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RepeatedBindCommand_UnderInterception_ClaimsEveryCallSite() =>
        AssertEveryCallSiteIsClaimed(RepeatedBindCommandSource, "BindCommandDispatch.g.cs");

    /// <summary>Both BindInteraction call sites are claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RepeatedBindInteraction_UnderInterception_ClaimsEveryCallSite() =>
        AssertEveryCallSiteIsClaimed(RepeatedBindInteractionSource, "BindInteractionDispatch.g.cs");

    /// <summary>Two ToProperty call sites naming the same property by string are each claimed, not only the first.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RepeatedToProperty_UnderInterception_ClaimsEveryCallSite() =>
        AssertEveryCallSiteIsClaimed(RepeatedToPropertySource, "ToPropertyDispatch.g.cs");

    /// <summary>Generates a scenario for an opted-in build and counts the call sites it claimed.</summary>
    /// <param name="source">The consumer source, which writes the same call site twice.</param>
    /// <param name="dispatchFileName">The dispatch file the API generates into.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    /// <remarks>
    /// A compiler that describes no call site emits the overloads instead, where collapsing is what should
    /// happen, so the expected count is stated for both builds rather than skipped on one. The assembly is
    /// emitted because that is where the compiler checks an interceptor against the call it replaces - two
    /// attributes that both claim the same call site pass every other check.
    /// </remarks>
    private static async Task AssertEveryCallSiteIsClaimed(string source, string dispatchFileName)
    {
        var parseOptions = TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGenerator(compilation, parseOptions, ProbeRootNamespace, true);

        await result.CompilationSucceeds();

        var expected = InterceptableLocationReader.IsSupported ? CallSitesPerScenario : 0;

        await result.GeneratedSourceContainsCount(dispatchFileName, InterceptsAttribute, expected);

        var (_, context) = TestHelper.EmitAndLoad(result);
        context.Unload();
    }
}
