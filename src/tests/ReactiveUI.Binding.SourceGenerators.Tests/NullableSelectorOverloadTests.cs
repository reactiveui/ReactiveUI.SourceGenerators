// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// A lambda over a nullable reference property has to reach the generated overload without the consumer's own
/// code drawing a nullability warning.
/// </summary>
public class NullableSelectorOverloadTests
{
    /// <summary>The root namespace the scenarios build under.</summary>
    private const string RootNamespace = "TestApp";

    /// <summary>The prefix every nullable-flow diagnostic id shares.</summary>
    private const string NullabilityDiagnosticPrefix = "CS86";

    /// <summary>The consumer source, with one call site standing in for <c>__CALL__</c>.</summary>
    private const string ScenarioTemplate = """
        #nullable enable
        using System;
        using System.ComponentModel;
        using System.Threading.Tasks;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace TestApp
        {
            public class User
            {
                public string Name { get; set; } = "";
            }

            public class Issue : INotifyPropertyChanged, INotifyPropertyChanging
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public event PropertyChangingEventHandler? PropertyChanging;

                public User? Assignee { get; set; }

                public string? Title { get; set; }

                public ICommand? Save { get; set; }

                public IInteraction<string, bool> Confirm { get; set; } = null!;
            }

            public class SaveButton
            {
                public event EventHandler? Click;
            }

            public class IssueView : INotifyPropertyChanged, IViewFor<Issue>
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public Issue? ViewModel { get; set; }

                object? IViewFor.ViewModel
                {
                    get { return ViewModel; }
                    set { ViewModel = (Issue?)value; }
                }

                public User? Owner { get; set; }

                public string? Caption { get; set; }

                public SaveButton Button { get; } = new SaveButton();
            }

            public static class Usage
            {
                public static void Run(Issue issue, Issue? maybe, IssueView view, IssueView? maybeView, IObservable<string> names)
                {
                    GC.KeepAlive(__CALL__);
                }

                private static Task Handle(IInteractionContext<string, bool> context) => Task.CompletedTask;
            }
        }
        """;

    /// <summary>Every API takes a lambda over a nullable reference property without a nullability warning.</summary>
    /// <param name="call">The call site the scenario makes.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("issue.WhenChanged(x => x.Assignee)")]
    [Arguments("issue.WhenChanging(x => x.Assignee)")]
    [Arguments("issue.WhenAnyValue(x => x.Assignee)")]
    [Arguments("issue.WhenAnyValue(x => x.Assignee, x => x.Title)")]
    [Arguments("issue.WhenAnyValue(x => x.Assignee, x => x.Title, (a, t) => a)")]
    [Arguments("issue.WhenChanged(x => x.Assignee, x => x.Title, (a, t) => a)")]
    [Arguments("issue.WhenAny(x => x.Assignee, c => c.Value)")]
    [Arguments("issue.BindOneWay(view, x => x.Assignee, v => v.Owner)")]
    [Arguments("issue.BindTwoWay(view, x => x.Assignee, v => v.Owner)")]
    [Arguments("view.OneWayBind(issue, x => x.Assignee, v => v.Owner)")]
    [Arguments("view.Bind(issue, x => x.Assignee, v => v.Owner)")]
    [Arguments("view.BindInteraction(maybe, x => x.Confirm, Handle)")]
    [Arguments("view.BindCommand(maybe, x => x.Save, v => v.Button)")]
    [Arguments("names.BindTo(maybeView, v => v.Caption)")]
    [Arguments("names.InvokeCommand(maybe, x => x.Save)")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableReferenceProperty_ReportsNoNullabilityWarnings(string call) =>
        AssertNoNullabilityWarnings(call, optIn: false);

    /// <summary>The interceptor a call site is claimed by carries the same annotations.</summary>
    /// <param name="call">The call site the scenario makes.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments("issue.WhenChanged(x => x.Assignee)")]
    [Arguments("issue.WhenChanging(x => x.Assignee)")]
    [Arguments("issue.WhenAnyValue(x => x.Assignee)")]
    [Arguments("issue.WhenAny(x => x.Assignee, c => c.Value)")]
    [Arguments("issue.BindOneWay(view, x => x.Assignee, v => v.Owner)")]
    [Arguments("view.Bind(issue, x => x.Assignee, v => v.Owner)")]
    [Arguments("view.BindInteraction(maybe, x => x.Confirm, Handle)")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableReferenceProperty_InterceptedCallSite_ReportsNoNullabilityWarnings(string call) =>
        AssertNoNullabilityWarnings(call, optIn: true);

    /// <summary>Generates the scenario and asserts the compilation reports no nullable-flow warning and no error.</summary>
    /// <param name="call">The call site the scenario makes.</param>
    /// <param name="optIn">Whether the build lists the generated namespace for interception.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertNoNullabilityWarnings(string call, bool optIn)
    {
        var source = ScenarioTemplate.Replace("__CALL__", call, StringComparison.Ordinal);
        var parseOptions = optIn
            ? TestHelper.InterceptingParseOptionsFor(LanguageVersion.CSharp11)
            : TestHelper.ParseOptionsFor(LanguageVersion.CSharp11);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);

        var result = TestHelper.RunGenerator(compilation, parseOptions, RootNamespace, false);
        await result.CompilationSucceeds();

        var consumerTree = compilation.SyntaxTrees.First();
        var warnings = result.OutputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Warning
                && d.Location.SourceTree == consumerTree
                && d.Id.StartsWith(NullabilityDiagnosticPrefix, StringComparison.Ordinal))
            .Select(static d => $"{d.Id}: {d.GetMessage()} at {d.Location}")
            .ToArray();

        await Assert.That(warnings).IsEmpty().Because(string.Join(Environment.NewLine, warnings));
    }
}
