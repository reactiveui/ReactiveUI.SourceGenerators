// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;
using ReactiveUI.Binding.SourceGenerators.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Covers RXUIBIND009, which reports a binding the generated dispatch cannot reach. Without it the call
/// silently takes the runtime path, which is a performance cliff nothing in the build points at.
/// </summary>
public class DispatchReachAnalyzerTests
{
    /// <summary>The diagnostic this analyzer reports.</summary>
    private const string DiagnosticId = "RXUIBIND009";

    /// <summary>The root namespace the dispatch would be emitted into.</summary>
    private const string RootNamespace = "Contoso.App";

    /// <summary>A namespace with no relationship to the root namespace.</summary>
    private const string UnrelatedNamespace = "Fabrikam.Ui";

    /// <summary>A namespace nested under the root namespace.</summary>
    private const string NestedNamespace = $"{RootNamespace}.Views";

    /// <summary>An assembly-level grant of internals to another assembly.</summary>
    private const string GrantsInternals =
        "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"Contoso.App.Tests\")]\n";

    /// <summary>A consumer whose call site sits in the global namespace, with no enclosing namespace at all.</summary>
    private const string GlobalNamespaceSource = """
                                                 using System;
                                                 using System.ComponentModel;
                                                 using System.Linq.Expressions;
                                                 using ReactiveUI.Binding;

                                                 [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Contoso.App.Tests")]

                                                 namespace ReactiveUI.Binding
                                                 {
                                                     public static class ReactiveUIBindingExtensions
                                                     {
                                                         public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
                                                             this TObj objectToMonitor,
                                                             Expression<Func<TObj, TReturn>> property1)
                                                             where TObj : class
                                                             => throw new NotImplementedException();
                                                     }
                                                 }

                                                 public class GlobalViewModel : INotifyPropertyChanged
                                                 {
                                                     public event PropertyChangedEventHandler PropertyChanged;

                                                     public string Name { get; set; }
                                                 }

                                                 public static class GlobalUsage
                                                 {
                                                     public static IObservable<string> Observe(GlobalViewModel viewModel)
                                                     {
                                                         return viewModel.WhenChanged(x => x.Name);
                                                     }
                                                 }
                                                 """;

    /// <summary>All three conditions hold, so the call is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OldLanguageVersion_ExposedInternals_FileOutsideTheRootNamespace_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A file under the root namespace reaches the dispatch, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FileUnderTheRootNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(NestedNamespace, GrantsInternals),
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>
    /// Without exposed internals the dispatch stays in the shared namespace, which every file reaches, so
    /// there is nothing to warn about wherever the file sits.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithoutExposedInternals_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, string.Empty),
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>From C# 10 the generated import carries every file, so there is nothing to warn about.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FromCSharp10_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            LanguageVersion.CSharp10,
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>
    /// The reach of a dispatch overload decides nothing where the call site is claimed outright, so a build
    /// that lists the generated namespace is not warned - on the compiler that can honour the listing. On one
    /// that cannot, the overloads are still what serves the call and the warning stands.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ListingTheGeneratedNamespace_IsReportedOnlyWhereInterceptionCannotBeHonoured()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            AnalyzerTestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp7_3),
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId))
            .IsEqualTo(!InterceptableLocationReader.IsSupported);
    }

    /// <summary>
    /// Listing another package's namespace is not this package's opt-in, so the call is reported whichever
    /// compiler is building it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ListingAnotherPackagesNamespace_IsReported()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp7_3)
            .WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", "Some.Other.Package")]);

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            parseOptions,
            RootNamespace);

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>With no root namespace there is nowhere else to emit, so the shared namespace is kept.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithoutARootNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            LanguageVersion.CSharp7_3,
            null);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>
    /// A file declared in the global namespace has no enclosing namespace at all, so it never reaches the root
    /// namespace either.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallSiteInTheGlobalNamespace_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            GlobalNamespaceSource,
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A file declared as the root namespace itself reaches the dispatch without being nested in it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallSiteExactlyInTheRootNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(RootNamespace, GrantsInternals),
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>
    /// Sharing the root namespace's opening characters is not the same as being nested under it, so
    /// <c>Contoso.AppExtra</c> is out of reach even though it starts with <c>Contoso.App</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CallSiteInANamespaceMerelySharingThePrefix_IsReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn($"{RootNamespace}Extra", GrantsInternals),
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Count(static d => d.Id == DiagnosticId)).IsEqualTo(1);
    }

    /// <summary>An invocation that is not a binding call is passed over, wherever it sits.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NonBindingInvocation_IsNotReported()
    {
        var source = SourceIn(UnrelatedNamespace, GrantsInternals)
            .Replace(
                "return viewModel.WhenChanged(x => x.Name);",
                "viewModel.ToString();\n            return null;",
                StringComparison.Ordinal);

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            source,
            LanguageVersion.CSharp7_3,
            RootNamespace);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A blank root namespace names nowhere, so it is treated as none at all.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BlankRootNamespace_IsNotReported()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<DispatchReachAnalyzer>(
            SourceIn(UnrelatedNamespace, GrantsInternals),
            LanguageVersion.CSharp7_3,
            "   ");

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>Renders a consumer with one binding call in the given namespace.</summary>
    /// <param name="consumerNamespace">The namespace the call site is declared in.</param>
    /// <param name="assemblyAttributes">Assembly-level attributes to include.</param>
    /// <returns>The source code.</returns>
    /// <remarks>
    /// Declares the runtime stub in source, the way the other analyzer tests do: the test compilation carries
    /// framework references only, so a call has to have something in the compilation to bind to.
    /// </remarks>
    private static string SourceIn(string consumerNamespace, string assemblyAttributes) =>
        $$"""
          using System;
          using System.ComponentModel;
          using System.Linq.Expressions;
          using ReactiveUI.Binding;

          {{assemblyAttributes}}
          namespace ReactiveUI.Binding
          {
              public static class ReactiveUIBindingExtensions
              {
                  public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
                      this TObj objectToMonitor,
                      Expression<Func<TObj, TReturn>> property1)
                      where TObj : class
                      => throw new NotImplementedException();
              }
          }

          namespace {{consumerNamespace}}
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
}
