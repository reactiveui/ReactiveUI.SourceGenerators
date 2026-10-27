// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="ToPropertyInitialValueAnalyzer"/>, which reports RXUIBIND014.</summary>
public class ToPropertyInitialValueAnalyzerTests
{
    /// <summary>The diagnostic reported for an ambiguous positional initial value.</summary>
    private const string DiagnosticId = "RXUIBIND014";

    /// <summary>The metadata name of the class that declares the stubs.</summary>
    private const string StubClassName = "ReactiveUI.Binding.ReactiveUIBindingExtensions";

    /// <summary>The number of arguments up to and including the initial value: the source, the selector and the value.</summary>
    private const int ArgumentsThroughInitialValue = 3;

    /// <summary>The number of parameters of the stub that takes no initial value.</summary>
    private const int SelectorOnlyParameterCount = 6;

    /// <summary>The two stub shapes that compete for a positional string initial value, as the runtime declares them.</summary>
    private const string Preamble = """
                                    using System;
                                    using System.ComponentModel;
                                    using System.Runtime.CompilerServices;
                                    using ReactiveUI.Binding;

                                    namespace ReactiveUI.Binding
                                    {
                                        public sealed class ObservableAsPropertyHelper<T>
                                        {
                                            public T Value => default;
                                        }

                                        public static class ReactiveUIBindingExtensions
                                        {
                                            public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
                                                this IObservable<TRet> target,
                                                TObj source,
                                                Func<TObj, TRet> property,
                                                [CallerArgumentExpression("property")] string propertyExpression = "",
                                                [CallerFilePath] string callerFilePath = "",
                                                [CallerLineNumber] int callerLineNumber = 0)
                                                where TObj : class
                                                => throw new InvalidOperationException();

                                            [OverloadResolutionPriority(1)]
                                            public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
                                                this IObservable<TRet> target,
                                                TObj source,
                                                Func<TObj, TRet> property,
                                                TRet initialValue,
                                                [CallerArgumentExpression("property")] string propertyExpression = "",
                                                [CallerFilePath] string callerFilePath = "",
                                                [CallerLineNumber] int callerLineNumber = 0)
                                                where TObj : class
                                                => throw new InvalidOperationException();
                                        }
                                    }

                                    namespace TestApp
                                    {
                                        public class MyViewModel
                                        {
                                            public string Title => "";

                                            public int Count => 0;

                                            public void Wire(IObservable<string> titles, IObservable<int> counts)
                                            {
                                                CALL
                                            }
                                        }
                                    }
                                    """;

    /// <summary>Below C# 13, a positional string initial value for a string property is reported on the argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PositionalStringInitialValue_BelowCSharp13_Reported()
    {
        var diagnostics = await RunAsync("titles.ToProperty(this, x => x.Title, \"(untitled)\");", LanguageVersion.CSharp10);

        var reported = diagnostics.Where(static d => d.Id == DiagnosticId).ToList();
        await Assert.That(reported.Count).IsEqualTo(1);
        await Assert.That(reported[0].Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(reported[0].GetMessage(System.Globalization.CultureInfo.InvariantCulture)).Contains("initialValue: \"(untitled)\"");
    }

    /// <summary>A named initial value resolves below C# 13, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NamedInitialValue_BelowCSharp13_NotReported()
    {
        var diagnostics = await RunAsync("titles.ToProperty(this, x => x.Title, initialValue: \"(untitled)\");", LanguageVersion.CSharp10);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>C# 13 honours the overload priority, so a positional initial value resolves and nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PositionalStringInitialValue_CSharp13_NotReported()
    {
        var diagnostics = await RunAsync("titles.ToProperty(this, x => x.Title, \"(untitled)\");", LanguageVersion.CSharp13);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A non-string initial value fits no caller-information parameter, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PositionalIntInitialValue_BelowCSharp13_NotReported()
    {
        var diagnostics = await RunAsync("counts.ToProperty(this, x => x.Count, 5);", LanguageVersion.CSharp10);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A mistyped call fails overload resolution but contests no initial value, so nothing is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MistypedCall_NoContestedInitialValue_NotReported()
    {
        var diagnostics = await RunAsync("titles.ToProperty(this, x => x.Count);", LanguageVersion.CSharp10);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>A named argument ends the positional scan, so a mistyped named initial value reports nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MistypedNamedInitialValue_NotReported()
    {
        var diagnostics = await RunAsync("titles.ToProperty(this, x => x.Title, initialValue: 1);", LanguageVersion.CSharp10);

        await Assert.That(diagnostics.Any(static d => d.Id == DiagnosticId)).IsFalse();
    }

    /// <summary>Called as plain static methods, the stubs take the argument one parameter later than as extensions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindPositionalInitialValue_WithUnreducedStubs_FindsTheContestedArgument()
    {
        var candidates = CandidatesFrom(StubClassName);

        await Assert.That(ToPropertyInitialValueAnalyzer.FindPositionalInitialValue(Arguments(ArgumentsThroughInitialValue), candidates))
            .IsEqualTo(ArgumentsThroughInitialValue - 1);
    }

    /// <summary>With no stub that takes an initial value, no argument is contested, even past the last parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindPositionalInitialValue_WithoutAnInitialValueStub_ReturnsNone()
    {
        var withoutInitialValue = CandidatesFrom(StubClassName)
            .Where(static c => ((IMethodSymbol)c).Parameters.Length == SelectorOnlyParameterCount)
            .ToImmutableArray();

        await Assert.That(ToPropertyInitialValueAnalyzer.FindPositionalInitialValue(Arguments(SelectorOnlyParameterCount + 1), withoutInitialValue)).IsEqualTo(-1);
    }

    /// <summary>An initial value that no candidate could also take as an optional string is not contested.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindPositionalInitialValue_WithOnlyTheInitialValueStub_ReturnsNone()
    {
        var withInitialValue = CandidatesFrom(StubClassName)
            .Where(static c => ((IMethodSymbol)c).Parameters.Length == SelectorOnlyParameterCount + 1)
            .ToImmutableArray();

        await Assert.That(ToPropertyInitialValueAnalyzer.FindPositionalInitialValue(Arguments(ArgumentsThroughInitialValue), withInitialValue)).IsEqualTo(-1);
    }

    /// <summary>A candidate that is not a method, or not one of the runtime's stubs, takes no argument as an initial value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FindPositionalInitialValue_WithForeignCandidates_ReturnsNone()
    {
        var candidates = CandidatesFrom("TestApp.Foreign");

        await Assert.That(ToPropertyInitialValueAnalyzer.FindPositionalInitialValue(Arguments(ArgumentsThroughInitialValue), candidates)).IsEqualTo(-1);
    }

    /// <summary>Parses a call's argument list with the given number of positional arguments.</summary>
    /// <param name="count">The number of arguments.</param>
    /// <returns>The arguments.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SeparatedSyntaxList<ArgumentSyntax> Arguments(int count) =>
        SyntaxFactory.ParseArgumentList($"({string.Join(", ", Enumerable.Repeat("1", count))})").Arguments;

    /// <summary>Gets every member of a type from the preamble's compilation as the candidates of an unresolved call.</summary>
    /// <param name="typeName">The metadata name of the type.</param>
    /// <returns>The members.</returns>
    private static ImmutableArray<ISymbol> CandidatesFrom(string typeName)
    {
        var source = Preamble.Replace("CALL", string.Empty, StringComparison.Ordinal)
            + "namespace TestApp { public static class Foreign { public static void ToProperty(int a) { } public static int Count => 0; } }";
        var compilation = AnalyzerTestHelper.CreateCompilation(source, LanguageVersion.CSharp13);
        return [.. compilation.GetTypeByMetadataName(typeName)!.GetMembers().Where(static m => m.Name is "ToProperty" or "Count")];
    }

    /// <summary>Runs the analyzer over the preamble with one call in place.</summary>
    /// <param name="call">The call statement.</param>
    /// <param name="version">The C# version to compile with.</param>
    /// <returns>The diagnostics reported.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Task<ImmutableArray<Diagnostic>> RunAsync(string call, LanguageVersion version) =>
        AnalyzerTestHelper.GetDiagnosticsAsync<ToPropertyInitialValueAnalyzer>(Preamble.Replace("CALL", call, StringComparison.Ordinal), version, null);
}
