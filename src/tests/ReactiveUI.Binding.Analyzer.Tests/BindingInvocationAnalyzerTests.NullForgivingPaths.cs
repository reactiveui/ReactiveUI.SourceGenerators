// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>
/// Checks that RXUIBIND003, RXUIBIND006 and RXUIBIND010 read a property path through the null-forgiving
/// operator and through parentheses, wherever either one sits in the path.
/// </summary>
public partial class BindingInvocationAnalyzerTests
{
    /// <summary>Observes the path with <c>WhenChanged</c>.</summary>
    private const string WhenChangedForm = "WhenChanged";

    /// <summary>Puts the path on the source side of <c>BindOneWay</c>.</summary>
    private const string BindOneWaySourceForm = "BindOneWaySource";

    /// <summary>Puts the path on the target side of <c>BindTwoWay</c>.</summary>
    private const string BindTwoWayTargetForm = "BindTwoWayTarget";

    /// <summary>Where <see cref="NullForgivingModel"/> takes the binding call.</summary>
    private const string CallPlaceholder = "__CALL__";

    /// <summary>The types a null-forgiving path is written against; the binding call is made from inside the model.</summary>
    private const string NullForgivingModel = """

        namespace TestApp
        {
            public class Address
            {
                public string City { get; set; } = "";
            }

            public class NotifyingAddress : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string City { get; set; } = "";
            }

            public class Inner : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name { get; set; } = "";
            }

            public class Model : INotifyPropertyChanged
            {
                public readonly string _name = "";

                public Inner _inner = new();

                public event PropertyChangedEventHandler? PropertyChanged;

                public string Name { get; set; } = "";

                public Address? Address { get; set; }

                public NotifyingAddress? NotifyingAddress { get; set; }

                public System.Collections.Generic.List<string> Items { get; set; } = new();

                public System.Collections.Generic.List<Inner> InnerItems { get; set; } = new();

                private string? Secret { get; set; }

                private NotifyingAddress? HiddenAddress { get; set; }

                public string GetName() => "";

                public Inner GetInner() => new();

                public void Test()
                {
                    __CALL__
                }
            }
        }
        """;

    /// <summary>A path through a type that raises no notification is reported wherever a <c>!</c> or parenthesis sits.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND010_NullForgivingOrParenthesizedPath_ReportsDiagnostic(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x.Address!.City",
            "x => x.Address.City!",
            "x => x.Address!.City!",
            "x => (x.Address).City",
            "x => (x.Address!).City")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == SilentPathLinkDiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A path through a type that notifies reports nothing wherever a <c>!</c> or parenthesis sits.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND010_NullForgivingOrParenthesizedNotifyingPath_ReportsNothing(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x.NotifyingAddress!.City",
            "x => x.NotifyingAddress.City!",
            "x => (x.NotifyingAddress!).City")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == SilentPathLinkDiagnosticId)).IsEqualTo(0);
    }

    /// <summary>A private member is reported wherever a <c>!</c> or parenthesis sits in the path.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND003_NullForgivingOrParenthesizedPath_ReportsDiagnostic(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x.Secret!",
            "x => x.HiddenAddress!.City",
            "x => x.HiddenAddress!.City!",
            "x => (x.Secret)",
            "x => (x.HiddenAddress!).City")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == PrivateMemberDiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A public path reports no private member wherever a <c>!</c> or parenthesis sits.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND003_NullForgivingOrParenthesizedPublicPath_ReportsNothing(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x.NotifyingAddress!.City",
            "x => x.Name!",
            "x => (x.NotifyingAddress!).City")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == PrivateMemberDiagnosticId)).IsEqualTo(0);
    }

    /// <summary>A read-only leaf field, indexer or method call is reported wherever a <c>!</c> or parenthesis sits in the path.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND006_NullForgivingOrParenthesizedPath_ReportsDiagnostic(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x._name!",
            "x => x.GetName()!",
            "x => x.GetInner()!.Name",
            "x => x.Items[0]!",
            "x => x.InnerItems[0]!.Name",
            "x => x.Items![0]",
            "x => x.Name!.ToUpperInvariant()",
            "x => (x._name)",
            "x => (x.GetInner()).Name")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == UnsupportedPathSegmentDiagnosticId)).IsEqualTo(1);
    }

    /// <summary>A property path reports no unsupported segment wherever a <c>!</c> or parenthesis sits.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MatrixDataSource]
    public async Task RXUIBIND006_NullForgivingOrParenthesizedPropertyPath_ReportsNothing(
        [Matrix(WhenChangedForm, BindOneWaySourceForm, BindTwoWayTargetForm)] string form,
        [Matrix(
            "x => x.NotifyingAddress!.City",
            "x => x.Name!",
            "x => (x.NotifyingAddress!).City",
            "x => x._inner!.Name",
            "x => (x._inner!).Name")] string lambda)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync<BindingInvocationAnalyzer>(
            BuildNullForgivingSource(form, lambda));

        await Assert.That(diagnostics.Count(static d => d.Id == UnsupportedPathSegmentDiagnosticId)).IsEqualTo(0);
    }

    /// <summary>Builds a compilation source whose only variable part is one property path in one binding call.</summary>
    /// <param name="form">The binding call the path is written in.</param>
    /// <param name="lambda">The property path lambda.</param>
    /// <returns>The source text.</returns>
    private static string BuildNullForgivingSource(string form, string lambda)
    {
        const string Bindings = "ReactiveUI.Binding.__ReactiveUIGeneratedBindings";

        var call = form switch
        {
            WhenChangedForm => $"{Bindings}.WhenChanged(this, {lambda});",
            BindOneWaySourceForm => $"{Bindings}.BindOneWay(this, this, {lambda}, x => x.Name);",
            _ => $"{Bindings}.BindTwoWay(this, this, x => x.Name, {lambda});",
        };

        return Preamble + NullForgivingModel.Replace(CallPlaceholder, call);
    }
}
