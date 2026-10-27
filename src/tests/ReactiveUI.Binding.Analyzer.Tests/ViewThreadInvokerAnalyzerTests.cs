// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="ViewThreadInvokerAnalyzer"/>, which reports RXUIBIND017.</summary>
public class ViewThreadInvokerAnalyzerTests
{
    /// <summary>The diagnostic reported for a binding onto a UI object without its platform package.</summary>
    private const string MissingInvokerId = "RXUIBIND017";

    /// <summary>The WPF platform package.</summary>
    private const string WpfPackage = "ReactiveUI.Binding.Wpf";

    /// <summary>
    /// A runtime stub with a writing and a non-writing binding API, a generated class carrying the assembly's name,
    /// and one type from each UI platform.
    /// </summary>
    private const string Preamble = """
                                    using System;
                                    using ReactiveUI.Binding;

                                    namespace ReactiveUI.Binding
                                    {
                                        public static class ReactiveUIBindingExtensions
                                        {
                                            public static IDisposable BindOneWay<TSource, TTarget>(this TSource source, TTarget target) => null;

                                            public static IDisposable OneWayBindUnsafe<TView>(this TView view) => null;

                                            public static IDisposable BindCommand<TView, TControl>(this TView view, TControl control) => null;

                                            public static IDisposable WhenChanged<TSource>(this TSource source) => null;
                                        }

                                        internal static class __ReactiveUIGeneratedBindings_TestAssembly
                                        {
                                            public static IDisposable BindTo<TTarget>(this object source, TTarget target) => null;
                                        }
                                    }

                                    namespace System.Windows.Threading
                                    {
                                        public class DispatcherObject { }
                                    }

                                    namespace System.Windows.Forms
                                    {
                                        public class Control { }
                                    }

                                    namespace Microsoft.Maui.Controls
                                    {
                                        public class BindableObject { }
                                    }

                                    namespace TestApp
                                    {
                                        public class WpfView : System.Windows.Threading.DispatcherObject { }

                                        public class WinFormsView : System.Windows.Forms.Control { }

                                        public class MauiButton : Microsoft.Maui.Controls.BindableObject { }

                                        public class Model { }
                                    }

                                    namespace Elsewhere
                                    {
                                        public static class Other
                                        {
                                            public static IDisposable BindOneWay<TSource, TTarget>(this TSource source, TTarget target) => null;
                                        }
                                    }

                                    """;

    /// <summary>A binding onto a WPF object, with no WPF package, is reported and names the package.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_OntoAWpfObjectWithoutThePackage_IsReported()
    {
        var diagnostics = await GetDiagnosticsAsync(Usage("new Model().BindOneWay(new WpfView());"));

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].GetMessage()).Contains(WpfPackage);
        await Assert.That(diagnostics[0].GetMessage()).Contains("WpfView");
    }

    /// <summary>An <c>Unsafe</c> binding onto a WinForms control, with no WinForms package, is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UnsafeBinding_OntoAWinFormsControlWithoutThePackage_IsReported()
    {
        var diagnostics = await GetDiagnosticsAsync(Usage("new WinFormsView().OneWayBindUnsafe();"));

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].GetMessage()).Contains("ReactiveUI.Binding.WinForms");
    }

    /// <summary>A command bound to a MAUI control, with no MAUI package, is reported for the control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_OntoAMauiControlWithoutThePackage_IsReported()
    {
        var diagnostics = await GetDiagnosticsAsync(Usage("new Model().BindCommand(new MauiButton());"));

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].GetMessage()).Contains("ReactiveUI.Binding.Maui");
    }

    /// <summary>A call bound to a generated class that carries the assembly's name is recognised.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GeneratedOverloadWithAnAssemblyQualifiedClass_IsReported()
    {
        var diagnostics = await GetDiagnosticsAsync(Usage("new Model().BindTo(new WpfView());"));

        await Assert.That(diagnostics.Length).IsEqualTo(1);
    }

    /// <summary>With either flavour of the platform invoker referenced, nothing is reported.</summary>
    /// <param name="invokerNamespace">The namespace the referenced invoker is declared in.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("ReactiveUI.Binding.Wpf")]
    [Arguments("ReactiveUI.Binding.Reactive.Wpf")]
    public async Task BindOneWay_WithThePackage_IsNotReported(string invokerNamespace)
    {
        var source = Usage("new Model().BindOneWay(new WpfView());")
            + $"namespace {invokerNamespace} {{ public sealed class DispatcherViewThreadInvoker {{ }} }}";

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    /// <summary>A binding onto an object no UI platform owns needs no platform package.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_OntoAPlainObject_IsNotReported() =>
        await Assert.That((await GetDiagnosticsAsync(Usage("new Model().BindOneWay(new Model());"))).Length).IsEqualTo(0);

    /// <summary>Observation writes nothing to the UI object, so it is not reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Observation_OfAWpfObject_IsNotReported() =>
        await Assert.That((await GetDiagnosticsAsync(Usage("new WpfView().WhenChanged();"))).Length).IsEqualTo(0);

    /// <summary>A method that only shares a binding API's name is not a binding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameNamedMethodOutsideTheBindingApi_IsNotReported() =>
        await Assert.That((await GetDiagnosticsAsync(Usage("Elsewhere.Other.BindOneWay(new Model(), new WpfView());"))).Length).IsEqualTo(0);

    /// <summary>The writing APIs are recognised with and without their <c>Unsafe</c> suffix.</summary>
    /// <param name="name">The method name.</param>
    /// <param name="expected">Whether the API writes to a target.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments("Bind", true)]
    [Arguments("BindUnsafe", true)]
    [Arguments("BindTwoWay", true)]
    [Arguments("BindCommandUnsafe", true)]
    [Arguments("WhenChanged", false)]
    [Arguments("WhenChangedUnsafe", false)]
    public async Task IsWritingApi_RecognisesTheWritingApis(string name, bool expected) =>
        await Assert.That(ViewThreadInvokerAnalyzer.IsWritingApi(name)).IsEqualTo(expected);

    /// <summary>Wraps a statement in the test's usage class after the shared preamble.</summary>
    /// <param name="statement">The statement to run.</param>
    /// <returns>The source.</returns>
    private static string Usage(string statement) => Preamble + $$"""
                                                                  namespace TestApp
                                                                  {
                                                                      public static class Usage
                                                                      {
                                                                          public static void Run()
                                                                          {
                                                                              {{statement}}
                                                                          }
                                                                      }
                                                                  }

                                                                  """;

    /// <summary>Runs the analyzer and keeps its own diagnostic.</summary>
    /// <param name="source">The source to analyze.</param>
    /// <returns>The RXUIBIND017 diagnostics.</returns>
    private static async Task<Microsoft.CodeAnalysis.Diagnostic[]> GetDiagnosticsAsync(string source)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<ViewThreadInvokerAnalyzer>(source);
        return [.. diagnostics.Where(static d => d.Id == MissingInvokerId)];
    }
}
