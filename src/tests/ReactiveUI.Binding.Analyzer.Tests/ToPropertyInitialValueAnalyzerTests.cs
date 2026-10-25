// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.Analyzer.Analyzers;
using ReactiveUI.Binding.Analyzer.Tests.Helpers;

namespace ReactiveUI.Binding.Analyzer.Tests;

/// <summary>Tests for <see cref="ToPropertyInitialValueAnalyzer"/>, which reports RXUIBIND014.</summary>
public class ToPropertyInitialValueAnalyzerTests
{
    /// <summary>The diagnostic reported for an ambiguous positional initial value.</summary>
    private const string DiagnosticId = "RXUIBIND014";

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

    /// <summary>Runs the analyzer over the preamble with one call in place.</summary>
    /// <param name="call">The call statement.</param>
    /// <param name="version">The C# version to compile with.</param>
    /// <returns>The diagnostics reported.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Task<ImmutableArray<Diagnostic>> RunAsync(string call, LanguageVersion version) =>
        AnalyzerTestHelper.GetDiagnosticsAsync<ToPropertyInitialValueAnalyzer>(Preamble.Replace("CALL", call, StringComparison.Ordinal), version, null);
}
