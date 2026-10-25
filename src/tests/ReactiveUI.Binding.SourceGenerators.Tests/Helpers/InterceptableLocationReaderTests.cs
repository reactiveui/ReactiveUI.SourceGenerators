// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Tests for <see cref="InterceptableLocationReader"/>, which is where both builds answer about interception.</summary>
public class InterceptableLocationReaderTests
{
    /// <summary>The namespace the generator emits interceptors into.</summary>
    private const string GeneratedNamespace = Constants.InterceptorNamespace;

    /// <summary>The feature name a build lists interceptable namespaces under.</summary>
    private const string FeatureName = Constants.InterceptorsNamespacesFeature;

    /// <summary>A build that lists nothing has not opted in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_NoFeature_ReturnsFalse() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(new CSharpParseOptions())).IsFalse();

    /// <summary>An empty list is the same as no list.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_EmptyFeature_ReturnsFalse() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces(string.Empty))).IsFalse();

    /// <summary>The generated namespace named exactly is an opt-in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_ExactNamespace_ReturnsTrue() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces(GeneratedNamespace))).IsTrue();

    /// <summary>A listed namespace covers the ones nested under it, so a parent counts.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_EnclosingNamespace_ReturnsTrue() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces("ReactiveUI.Binding"))).IsTrue();

    /// <summary>A namespace that merely starts with the same characters is a different namespace.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_NamespaceSharingAPrefix_ReturnsFalse() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces("ReactiveUI.Bind"))).IsFalse();

    /// <summary>Another package's namespace is not this one's opt-in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_UnrelatedNamespace_ReturnsFalse() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces("Some.Other.Package"))).IsFalse();

    /// <summary>The listed namespaces are read out of a list, so the entry can be anywhere in it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_ListedAlongsideOthers_ReturnsTrue() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(
            WithNamespaces($"Some.Other.Package;{GeneratedNamespace};Third.Package"))).IsTrue();

    /// <summary>A separator with nothing between it and the next is skipped rather than matched.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_ListWithEmptyEntries_ReturnsTrue() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces($";;{GeneratedNamespace};"))).IsTrue();

    /// <summary>A list of nothing but separators names no namespace at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsOptedIn_ListOfSeparators_ReturnsFalse() =>
        await Assert.That(InterceptableLocationReader.IsOptedIn(WithNamespaces(";; ;"))).IsFalse();

    /// <summary>
    /// A call site is described where the compiler can describe one, and reported as undescribed where it
    /// cannot. Both answers are handled the same way by every caller, so both are asserted here.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CallSite_MatchesWhatThisBuildSupports()
    {
        const string Source = """
                              namespace Probe
                              {
                                  public static class Holder
                                  {
                                      public static string Read() => 1.ToString();
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(Source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var location = InterceptableLocationReader.Read(
            compilation.GetSemanticModel(tree),
            invocation,
            CancellationToken.None);

        await Assert.That(location.IsAvailable).IsEqualTo(InterceptableLocationReader.IsSupported);
    }

    /// <summary>
    /// A call the compiler declines to describe reports that nothing was described, on either build. The
    /// callee here is an element of an array of delegates rather than a name the compiler can attach an
    /// interceptor to, which is a shape no interceptor can claim however new the compiler is.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Read_CallSiteTheCompilerCannotDescribe_ReportsNothingDescribed()
    {
        const string Source = """
                              namespace Probe
                              {
                                  public static class Holder
                                  {
                                      public static System.Func<int, string>[] Table = null!;

                                      public static string Read() => Table[0](1);
                                  }
                              }
                              """;

        var compilation = TestHelper.CreateCompilation(Source, LanguageVersion.CSharp10);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var location = InterceptableLocationReader.Read(
            compilation.GetSemanticModel(tree),
            invocation,
            CancellationToken.None);

        await Assert.That(location.IsAvailable).IsFalse();
    }

    /// <summary>An undescribed call site carries no data, whichever build produced it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsAvailable_DefaultLocation_ReturnsFalse() =>
        await Assert.That(default(SourceGenerators.Models.InterceptorLocation).IsAvailable).IsFalse();

    /// <summary>Builds parse options listing the given namespaces for interception.</summary>
    /// <param name="namespaces">The value the build sets.</param>
    /// <returns>The parse options.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CSharpParseOptions WithNamespaces(string namespaces) =>
        new CSharpParseOptions().WithFeatures([new KeyValuePair<string, string>(FeatureName, namespaces)]);
}
