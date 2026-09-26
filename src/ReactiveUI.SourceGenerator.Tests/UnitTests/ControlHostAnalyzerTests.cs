// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="ControlHostAnalyzer" />.</summary>
public sealed class ControlHostAnalyzerTests
{
    /// <summary>A WinForms host generated without ObservedProperty.</summary>
    private const string WithoutObservedPropertyId = "RXUISG0022";

    /// <summary>Both hosts, and a class that is neither.</summary>
    private const string HostsSource = """
        using ReactiveUI.SourceGenerators.WinForms;

        namespace Hosts
        {
            [RoutedControlHost("System.Windows.Forms.UserControl")]
            public partial class RoutedHost { }

            [ViewModelControlHost("System.Windows.Forms.UserControl")]
            public partial class ViewModelHost { }

            [System.Obsolete]
            public class NotAHost { }
        }
        """;

    /// <summary>ReactiveUI.Binding without ObservedProperty: a release before 8.4.0.</summary>
    private const string BindingWithoutObservedProperty = """
        namespace ReactiveUI.Binding
        {
            public interface IViewFor<T> { }
        }
        """;

    /// <summary>ReactiveUI.Binding 8.4.0 or later, with ObservedProperty.</summary>
    private const string BindingWithObservedProperty = """
        namespace ReactiveUI.Binding
        {
            public interface IViewFor<T> { }

            public static class ObservedProperty { }
        }
        """;

    /// <summary>Each host is reported when the compilation has no ReactiveUI.Binding with ObservedProperty.</summary>
    /// <param name="bindingSource">Source standing in for ReactiveUI.Binding, or none.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [Arguments("")]
    [Arguments(BindingWithoutObservedProperty)]
    public async Task ReportsHostsWithoutObservedProperty(string bindingSource)
    {
        var reported = await GetReported(bindingSource);

        await Assert.That(reported).IsEquivalentTo(["RoutedHost", "ViewModelHost"]);
    }

    /// <summary>No host is reported when ReactiveUI.Binding has ObservedProperty.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DoesNotReportHostsWithObservedProperty() =>
        await Assert.That(await GetReported(BindingWithObservedProperty)).IsEmpty();

    /// <summary>The diagnostic names the ReactiveUI.Binding version that has ObservedProperty.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task NamesTheMinimumBindingVersion()
    {
        var descriptor = new ControlHostAnalyzer().SupportedDiagnostics[0];
        var message = Diagnostic.Create(descriptor, Location.None, "RoutedHost", "8.4.0").GetMessage(System.Globalization.CultureInfo.InvariantCulture);

        await Assert.That(descriptor.Id).IsEqualTo(WithoutObservedPropertyId);
        await Assert.That(descriptor.DefaultSeverity).IsEqualTo(DiagnosticSeverity.Info);
        await Assert.That(message).Contains("ReactiveUI.Binding 8.4.0 or later");
    }

    /// <summary>A null analysis context is rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task InitializeRejectsANullContext() =>
        await Assert.That(static () => new ControlHostAnalyzer().Initialize(null!)).Throws<ArgumentNullException>();

    /// <summary>Analyzes the hosts with a ReactiveUI.Binding stand-in and gets the text each host is reported at.</summary>
    /// <param name="bindingSource">Source standing in for ReactiveUI.Binding, or none.</param>
    /// <returns>The source text at each reported location.</returns>
    private static async Task<List<string>> GetReported(string bindingSource)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var tree = CSharpSyntaxTree.ParseText(HostsSource, parseOptions);
        var compilation = CSharpCompilation.Create(
            nameof(ControlHostAnalyzerTests),
            [tree, CSharpSyntaxTree.ParseText(bindingSource, parseOptions)],
            TestCompilationReferences.CreateForAssemblies(typeof(object).Assembly, typeof(RoutedControlHostAttribute).Assembly),
            new(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = await compilation.WithAnalyzers([new ControlHostAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        var text = await tree.GetTextAsync();

        var reported = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == WithoutObservedPropertyId)
            {
                reported.Add(text.ToString(diagnostic.Location.SourceSpan));
            }
        }

        return reported;
    }
}
