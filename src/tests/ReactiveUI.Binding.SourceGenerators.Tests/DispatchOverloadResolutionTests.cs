// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers the generated overload winning the call against the generic runtime stub. Overload resolution only
/// prefers the concrete candidate once the two parameter lists match; where they differ in length neither is
/// better and the call is ambiguous, so the generated overload has to carry the stub's optional expression
/// parameters even on language versions that cannot populate them.
/// </summary>
public class DispatchOverloadResolutionTests
{
    /// <summary>The hint name of the generated WhenChanged dispatch file.</summary>
    private const string DispatchFileName = "WhenChangedDispatch.g.cs";

    /// <summary>A minimal consumer: one observable type and one call site.</summary>
    private const string Source = """
                                  using System;
                                  using System.ComponentModel;
                                  using ReactiveUI.Binding;

                                  namespace Contoso.App
                                  {
                                      public class MyViewModel : INotifyPropertyChanged
                                      {
                                          public event PropertyChangedEventHandler PropertyChanged;

                                          public string Name { get; set; }
                                      }

                                      public static class Usage
                                      {
                                          public static IObservable<string> Observe(MyViewModel viewModel)
                                          {
                                              return viewModel.WhenChanged(x => x.Name);
                                          }
                                      }
                                  }
                                  """;

    /// <summary>
    /// A project can target a runtime that has <c>CallerArgumentExpression</c> while compiling below C# 10. The
    /// stub then declares the expression parameters and the generated overload cannot dispatch on them, which is
    /// the case that used to leave both candidates merely applicable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BelowCSharp10_TheCallStillBindsToTheGeneratedOverload()
    {
        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp7_3);

        await result.CompilationSucceeds();

        var symbol = await CallSiteResolution.ResolveAsync(result, Constants.WhenChangedMethodName);

        await Assert.That(symbol).IsNotNull();
        await Assert.That(symbol!.ContainingType.Name).IsEqualTo($"{Constants.GeneratedExtensionClassName}_TestAssembly");
    }

    /// <summary>
    /// The expression parameters are present so the parameter lists line up, and unattributed because below
    /// C# 10 the compiler would not fill them in - dispatch runs off the file and line instead.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BelowCSharp10_TheOverloadTakesTheExpressionParametersWithoutTheAttribute()
    {
        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp7_3);

        await result.GeneratedSourceContains(DispatchFileName, "            string property1Expression = \"\",");
        await Assert.That(result.GeneratedSources[DispatchFileName]).DoesNotContain("CallerArgumentExpression");
        await result.GeneratedSourceContains(DispatchFileName, "callerLineNumber ==");
    }

    /// <summary>From C# 10 the same parameters carry the attribute and dispatch runs off the expression text.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FromCSharp10_TheOverloadTakesTheAttributedExpressionParameters()
    {
        var result = TestHelper.RunGenerator(Source, LanguageVersion.CSharp10);

        await result.CompilationSucceeds();
        await result.GeneratedSourceContains(
            DispatchFileName,
            "[global::System.Runtime.CompilerServices.CallerArgumentExpression(\"property1\")] string property1Expression = \"\",");

        var symbol = await CallSiteResolution.ResolveAsync(result, Constants.WhenChangedMethodName);

        await Assert.That(symbol).IsNotNull();
        await Assert.That(symbol!.ContainingType.Name).IsEqualTo(Constants.GeneratedExtensionClassName);
    }
}
