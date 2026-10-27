// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND010 (silent path link).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported for an observed path through a type that raises no notification.</summary>
    private const string SilentPathLinkDiagnosticId = "RXUIBIND010";

    /// <summary>
    /// A type part way along the path that raises nothing is reported. The observation reads it once and stops
    /// following the path there, which nothing at the call site would otherwise say.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND010_SilentIntermediate_InPropertyPath_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Address
                                             {
                                                 public string City { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;

                                                 public Address Address { get; set; } = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Address.City);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var silentDiags = diagnostics.Where(static d => d.Id == SilentPathLinkDiagnosticId).ToArray();

        await Assert.That(silentDiags.Length).IsEqualTo(1);
    }

    /// <summary>An intermediate that notifies is followed, so nothing is reported for it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND010_NotifyingIntermediate_InPropertyPath_ReportsNothing()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class Address : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;

                                                 public string City { get; set; } = "";
                                             }

                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;

                                                 public Address Address { get; set; } = new();
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Address.City);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var silentDiags = diagnostics.Where(static d => d.Id == SilentPathLinkDiagnosticId).ToArray();

        await Assert.That(silentDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// A shallow path reports nothing here. The only type it names is the one the call was made on, which is
    /// already answered for where that type is declared.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND010_ShallowPath_ReportsNothing()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel
                                             {
                                                 public string Name { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.Name);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var silentDiags = diagnostics.Where(static d => d.Id == SilentPathLinkDiagnosticId).ToArray();

        await Assert.That(silentDiags.Length).IsEqualTo(0);
    }
}
