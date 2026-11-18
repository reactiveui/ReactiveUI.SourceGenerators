// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND022 (a selector that is not a property path).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported for a selector that is not a property path.</summary>
    private const string SelectorNotPropertyPathDiagnosticId = "RXUIBIND022";

    /// <summary>The view model the selectors are written against, with the call left open for one selector.</summary>
    private const string SelectorShapeSource = Preamble + """

                                                          namespace TestApp
                                                          {
                                                              public class Child
                                                              {
                                                                  public string Name { get; set; } = "";
                                                                  public int Count { get; set; }
                                                                  public int? Limit { get; set; }
                                                                  public Child? Next { get; set; }
                                                                  public string Describe() => Name;
                                                              }

                                                              public class MyViewModel : INotifyPropertyChanged
                                                              {
                                                                  public event PropertyChangedEventHandler? PropertyChanged;
                                                                  public string Name { get; set; } = "";
                                                                  public int Count { get; set; }
                                                                  public Child? Child { get; set; }
                                                                  public Child[] Children { get; set; } = new Child[0];

                                                                  public void Test(MyViewModel other)
                                                                  {
                                                                      ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, SELECTOR);
                                                                  }
                                                              }
                                                          }
                                                          """;

    /// <summary>Verifies a selector the generator reads as a path is not reported.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Name")]
    [Arguments("static x => x.Name")]
    [Arguments("(MyViewModel x) => x.Count")]
    [Arguments("x => x.Child!.Name")]
    [Arguments("x => (x.Child)!.Name")]
    [Arguments("x => x.Child?.Name")]
    [Arguments("x => x.Child?.Next?.Name")]
    [Arguments("x => x.Child?.Next!.Name")]
    [Arguments("x => (x.Child?.Next)!.Name")]
    [Arguments("x => x.Child?.Limit")]
    [Arguments("x => x.Child?.Next")]
    public async Task RXUIBIND022_PropertyPath_NoDiagnostic(string selector)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(WithSelector(selector));

        await Assert.That(diagnostics.Count(static d => d.Id == SelectorNotPropertyPathDiagnosticId)).IsEqualTo(0);
    }

    /// <summary>Verifies a selector that is not a chain of members read from its parameter is reported once, with the reason.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <param name="reason">Words the message gives as the reason.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => { return x.Name; }", "statement body")]
    [Arguments("x => { var name = x.Name; return name; }", "statement body")]
    [Arguments("x => x", "its own parameter")]
    [Arguments("x => other.Name", "its own parameter")]
    [Arguments("x => x.Name + \"!\"", "its own parameter")]
    [Arguments("x => x.Count + 1", "its own parameter")]
    [Arguments("x => x.Child ?? other.Child", "its own parameter")]
    [Arguments("x => x.Count > 0 ? x.Name : \"\"", "its own parameter")]
    [Arguments("x => ((MyViewModel)x).Name", "its own parameter")]
    [Arguments("x => x.Child?.Count", "makes the result nullable")]
    [Arguments("x => x.Child?.Next?.Count", "makes the result nullable")]
    [Arguments("x => (x.Child?.Next)!.Count", "makes the result nullable")]
    public async Task RXUIBIND022_NotAPropertyPath_ReportsDiagnostic(string selector, string reason)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(WithSelector(selector));
        var reported = diagnostics.Where(static d => d.Id == SelectorNotPropertyPathDiagnosticId).ToArray();

        await Assert.That(reported.Length).IsEqualTo(1);
        await Assert.That(reported[0].GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Contains(reason);
    }

    /// <summary>Verifies a method call or an indexer in the path is left to RXUIBIND006, so the selector is reported once.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("x => x.Child!.Describe()")]
    [Arguments("x => x.Children[0].Name")]
    [Arguments("x => x.Child?.Describe()")]
    public async Task RXUIBIND022_PathRXUIBIND006Reports_NoDiagnostic(string selector)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(WithSelector(selector));

        await Assert.That(diagnostics.Count(static d => d.Id == SelectorNotPropertyPathDiagnosticId)).IsEqualTo(0);
        await Assert.That(diagnostics.Count(static d => d.Id == UnsupportedPathSegmentDiagnosticId)).IsEqualTo(1);
    }

    /// <summary>Verifies the other path checks read a member written after <c>?.</c> as they read one written after <c>.</c>.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ConditionalPath_PrivateMember_ReportsRXUIBIND003()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private MyViewModel? Hidden { get; set; }
                                                 public string Name { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.Hidden?.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);

        await Assert.That(diagnostics.Count(static d => d.Id == PrivateMemberDiagnosticId)).IsEqualTo(1);
        await Assert.That(diagnostics.Count(static d => d.Id == SelectorNotPropertyPathDiagnosticId)).IsEqualTo(0);
    }

    /// <summary>Verifies a lambda with more than one parameter, which no selector parameter accepts, is not read as a path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SelectorShapeFault_TwoParameters_IsNotAPath()
    {
        const string Source = """
                              class Model
                              {
                                  public string Name { get; set; } = "";

                                  public System.Func<Model, Model, string> Selector() => (x, y) => x.Name;
                              }
                              """;
        var compilation = AnalyzerTestHelper.CreateCompilation(Source);
        var tree = compilation.SyntaxTrees[0];
        var lambda = (await tree.GetRootAsync()).DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax>().Single();

        var fault = BindingInvocationAnalyzer.SelectorShapeFault(lambda, compilation.GetSemanticModel(tree), CancellationToken.None);

        await Assert.That(fault).Contains("its own parameter");
    }

    /// <summary>Writes a selector into the call of <see cref="SelectorShapeSource"/>.</summary>
    /// <param name="selector">The selector lambda.</param>
    /// <returns>The source to analyze.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static string WithSelector(string selector) => SelectorShapeSource.Replace("SELECTOR", selector, StringComparison.Ordinal);
}
