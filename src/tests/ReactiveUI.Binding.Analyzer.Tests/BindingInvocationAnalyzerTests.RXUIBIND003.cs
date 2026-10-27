// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="BindingInvocationAnalyzer"/> — RXUIBIND003 (private or protected member access).</summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>The diagnostic id reported for a private or protected member access.</summary>
    private const string PrivateMemberDiagnosticId = "RXUIBIND003";

    /// <summary>Verifies RXUIBIND003 is reported when accessing a private property in a lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_PrivateProperty_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.Secret);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND003 is reported for the private path of a WhenAnyValue call that also observes a public one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_WhenAnyValue_PrivatePathBesideAPublicOne_ReportsDiagnostic()
    {
        const string Source = """
                              using System;
                              using System.Collections.Generic;
                              using System.ComponentModel;
                              using System.Linq.Expressions;
                              using ReactiveUI.Binding;

                              namespace ReactiveUI.Binding
                              {
                                  public static class __ReactiveUIGeneratedBindings
                                  {
                                      public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, TRet>(
                                          this TSender sender,
                                          Expression<Func<TSender, T1>> property1,
                                          Expression<Func<TSender, T2>> property2,
                                          Func<T1, T2, TRet> selector)
                                          => throw new NotImplementedException();
                                  }
                              }

                              namespace TestApp
                              {
                                  public class ItemsViewModel : INotifyPropertyChanged
                                  {
                                      public event PropertyChangedEventHandler? PropertyChanged;

                                      public string FilterText { get; set; } = "";

                                      private IReadOnlyList<string> AllItems { get; set; } = [];

                                      public IObservable<int> Counts() =>
                                          this.WhenAnyValue(x => x.AllItems, x => x.FilterText, (items, filter) => items.Count);
                                  }
                              }
                              """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND003 is reported when accessing a protected property in a lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ProtectedProperty_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 protected string InternalName { get; set; } = "";
                                             }

                                             public class Usage : MyViewModel
                                             {
                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.InternalName);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND003 is NOT reported for public property access.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_PublicProperty_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
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
        var privateMemberDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateMemberDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies RXUIBIND003 is NOT reported when a block-body lambda is used,
    /// because the analyzer only walks expression-body lambdas for private member access.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BlockBodyLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => { return x.Secret; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND003 is reported when accessing a private property in a WhenChanging lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_WhenChanging_PrivateProperty_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged, INotifyPropertyChanging
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public event PropertyChangingEventHandler? PropertyChanging;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanging(this, x => x.Secret);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND003 is reported when accessing a private property in a BindOneWay source lambda.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BindOneWay_PrivateProperty_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public MyView View { get; set; } = new();

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindOneWay(this, View, x => x.Secret, x => x.NameText);
                                                 }
                                             }

                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>Verifies RXUIBIND003 with block-body lambda does not fire for BindTwoWay.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BindTwoWay_BlockBodyLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public MyView View { get; set; } = new();

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTwoWay(this, View, x => { return x.Secret; }, x => x.NameText);
                                                 }
                                             }

                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 public string NameText { get; set; } = "";
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies that a parenthesized lambda with block body does not report RXUIBIND003 for private members.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ParenthesizedBlockBodyLambda_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, (MyViewModel x) => { return x.Secret; });
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported when accessing an internal property in a lambda,
    /// which is considered protected-level access (non-public). This validates the compound
    /// condition branch for Accessibility.Internal in the private member check.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_InternalProperty_NoDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 internal string InternalName { get; set; } = "";
                                             }

                                             public class Usage
                                             {
                                                 public void Test()
                                                 {
                                                     var vm = new MyViewModel();
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(vm, x => x.InternalName);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();

        // Internal is not Private or Protected, so no diagnostic
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 deep chain walking terminates correctly when reaching
    /// the lambda parameter (IdentifierNameSyntax). This tests the while loop exit condition
    /// where current is no longer a MemberAccessExpressionSyntax.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_DeepChain_PublicProperty_NoDiagnostic()
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
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported for a private property in a deep property chain.
    /// This ensures the while loop walks the entire chain and detects private members
    /// at intermediate positions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_DeepChain_PrivateIntermediateProperty_ReportsDiagnostic()
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
                                                 private Address PrivateAddress { get; set; } = new();

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x.PrivateAddress.City);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that RXUIBIND003 is reported for a private property accessed through
    /// a parenthesized lambda expression. This exercises the ParenthesizedLambdaExpressionSyntax
    /// branch in CheckPrivateMember.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_ParenthesizedLambda_PrivateProperty_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, (MyViewModel x) => x.Secret);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a private instance field reports RXUIBIND003 alone: generated code cannot reach it, but an
    /// instance field is otherwise a supported link, so RXUIBIND006 does not apply.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_PrivateField_ReportsPrivateMemberOnly()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyViewModel : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string _secret = "";

                                                 public void Test()
                                                 {
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.WhenChanged(this, x => x._secret);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);

        // RXUIBIND003 for private member access
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);

        // An instance field is a supported link, so RXUIBIND006 does not apply
        var unsupportedDiags = diagnostics.Where(static d => d.Id == "RXUIBIND006").ToArray();
        await Assert.That(unsupportedDiags.Length).IsEqualTo(0);
    }

    /// <summary>Verifies RXUIBIND003 is reported for BindTo when the target lambda accesses a private member.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RXUIBIND003_BindTo_PrivateMember_ReportsDiagnostic()
    {
        const string Source = Preamble + """

                                         namespace TestApp
                                         {
                                             public class MyView : INotifyPropertyChanged
                                             {
                                                 public event PropertyChangedEventHandler? PropertyChanged;
                                                 private string Secret { get; set; } = "";

                                                 public void Test()
                                                 {
                                                     IObservable<string> source = null!;
                                                     ReactiveUI.Binding.__ReactiveUIGeneratedBindings.BindTo(source, this, x => x.Secret);
                                                 }
                                             }
                                         }
                                         """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(Source);
        var privateDiags = diagnostics.Where(static d => d.Id == PrivateMemberDiagnosticId).ToArray();
        await Assert.That(privateDiags.Length).IsEqualTo(1);
    }
}
