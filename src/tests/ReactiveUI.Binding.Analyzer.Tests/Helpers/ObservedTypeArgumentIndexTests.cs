// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.Analyzer.Analyzers;

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>
/// Tests how <see cref="AnalyzerHelpers.ObservedTypeArgumentIndex"/> matches an API name: the API itself and its
/// <c>Unsafe</c> twin answer as the API does, and a longer name that only starts with the API's name does not.
/// </summary>
public class ObservedTypeArgumentIndexTests
{
    /// <summary>Methods named like the binding APIs, each with a <c>viewModel</c> parameter behind its second type parameter.</summary>
    private const string Source = """
                                  namespace TestApp
                                  {
                                      public static class Apis
                                      {
                                          public static void BindUnsafe<TView, TViewModel>(TView view, TViewModel viewModel) { }

                                          public static void BindTwoWay<TView, TViewModel>(TView view, TViewModel viewModel) { }

                                          public static void BindCommand<TView, TViewModel>(TView view, TViewModel viewModel) { }
                                      }
                                  }
                                  """;

    /// <summary>The index of the <c>TViewModel</c> type parameter in <see cref="Source"/>.</summary>
    private const int ViewModelIndex = 1;

    /// <summary>The <c>Unsafe</c> twin of <c>Bind</c> names the view model by its <c>viewModel</c> argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnsafeTwinOfBind_UsesTheViewModelArgument() =>
        await Assert.That(AnalyzerHelpers.ObservedTypeArgumentIndex(Method("BindUnsafe"))).IsEqualTo(ViewModelIndex);

    /// <summary>A name as long as the <c>Unsafe</c> twin that does not end in <c>Unsafe</c> is not <c>Bind</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameLengthAsTheUnsafeTwin_IsNotTheApi() =>
        await Assert.That(AnalyzerHelpers.ObservedTypeArgumentIndex(Method("BindTwoWay"))).IsEqualTo(0);

    /// <summary>A name that only starts with <c>Bind</c> is not <c>Bind</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongerNameStartingWithTheApi_IsNotTheApi() =>
        await Assert.That(AnalyzerHelpers.ObservedTypeArgumentIndex(Method("BindCommand"))).IsEqualTo(0);

    /// <summary>Reads a method declared in <see cref="Source"/>.</summary>
    /// <param name="name">The method name.</param>
    /// <returns>The method symbol.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IMethodSymbol Method(string name) =>
        AnalyzerTestHelper.CreateCompilation(Source)
            .GetTypeByMetadataName("TestApp.Apis")!
            .GetMembers(name)
            .OfType<IMethodSymbol>()
            .Single();
}
