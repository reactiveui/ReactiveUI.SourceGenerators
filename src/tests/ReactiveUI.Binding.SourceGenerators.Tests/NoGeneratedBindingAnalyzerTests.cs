// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias analyzer;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests;

/// <summary>
/// Covers RXUIBIND021, a binding call with no generated binding. The analyzer runs over the compilation the generators
/// produced, as it does in a build, so a call is reported only when nothing replaced it.
/// </summary>
public class NoGeneratedBindingAnalyzerTests
{
    /// <summary>The diagnostic reported for a call with no generated binding.</summary>
    private const string NoGeneratedBinding = "RXUIBIND021";

    /// <summary>A view model whose members are all declared in source, and a member another generator adds.</summary>
    private const string Model = """
        using System;
        using System.ComponentModel;
        using System.Linq.Expressions;
        using System.Windows.Input;
        using ReactiveUI.Binding;

        namespace App
        {
            public partial class Person : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public string? Name { get; set; }

                public ICommand? Save { get; set; }
            }

            public static class Usage
            {
                public static void Run(Person person, Expression<Func<Person, string?>> stored, ICommand command)
                {
                    CALLS
                }
            }
        }
        """;

    /// <summary>A call the generator binds is replaced, so nothing is reported, with or without interceptors.</summary>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GeneratedCall_IsNotReported(bool intercept) =>
        await Assert.That(await AnalyzeAsync(Calls("person.WhenAnyValue(x => x.Name);"), [], intercept)).IsEmpty();

    /// <summary>
    /// A call through a member another source generator adds has no generated binding, so it is reported. The caller
    /// cannot make another generator's output visible, so it stays a warning.
    /// </summary>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task MemberFromAnotherGenerator_IsReported(bool intercept)
    {
        var diagnostics = await AnalyzeAsync(Calls("person.WhenAnyValue(x => x.Nickname);"), [new NicknameGenerator().AsSourceGenerator()], intercept);

        await Assert.That(diagnostics.Select(static d => d.Id)).IsEquivalentTo([NoGeneratedBinding]);
        await Assert.That(diagnostics[0].GetMessage()).Contains("'WhenAnyValue' has no generated binding");
        await Assert.That(diagnostics[0].Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    /// <summary>Every member ReactiveUI.SourceGenerators adds gets a generated binding, so none of its calls is reported.</summary>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(false, false)]
    [Arguments(true, true)]
    public async Task SourceGeneratorsMembers_AreNotReported(bool useReactiveRuntime, bool intercept)
    {
        var result = SourceGeneratorsContractTests.RunBeside(SourceGeneratorsContractTests.Scenario, useReactiveRuntime, intercept);

        var diagnostics = await result.OutputCompilation
            .WithAnalyzers([new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.NoGeneratedBindingAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        await Assert.That(diagnostics).IsEmpty();
    }

    /// <summary>A path held in a variable cannot be read, so the call to the stub is reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StoredExpression_IsReported() =>
        await Assert.That((await AnalyzeAsync(Calls("person.WhenChanged(stored);"), [], false)).Select(static d => d.Id))
            .IsEquivalentTo([NoGeneratedBinding]);

    /// <summary>A <c>ToProperty</c> overload names its property without an expression tree, and still throws when not generated.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UngeneratedToProperty_IsReported() =>
        await Assert.That((await AnalyzeAsync(
                Calls("Func<Person, string?> selector = x => x.Name; System.IObservable<string?> source = null!; source.ToProperty(person, selector);"),
                [],
                false)).Select(static d => d.Id))
            .IsEquivalentTo([NoGeneratedBinding]);

    /// <summary>Methods that do their own work are not reported: an <c>Unsafe</c> overload, and <c>InvokeCommand</c> with a command object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MethodsThatRun_AreNotReported() =>
        await Assert.That(await AnalyzeAsync(
                Calls("person.WhenChangedUnsafe(stored); person.WhenChanged(x => x.Name).InvokeCommand(command);"),
                [],
                false))
            .IsEmpty();

    /// <summary>A method named like a binding API on a type other than the runtime stubs is not reported.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SameNameOnAnotherType_IsNotReported() =>
        await Assert.That(await AnalyzeAsync(
                $"{Calls("Other.WhenAnyValue(person);")}\npublic static class Other {{ public static void WhenAnyValue(object value) {{ }} }}",
                [],
                false))
            .IsEmpty();

    /// <summary>A stub declared in an extension block is recognised through its grouping type, read from source.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtensionBlockStub_IsReported()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;

            namespace Stubs
            {
                public static class ReactiveSchedulerExtensions
                {
                    extension<T>(T source)
                    {
                        public IObservable<int> WhenChanged(Expression<Func<T, int>> property) => throw new InvalidOperationException();
                    }
                }

                public static class Usage
                {
                    public static void Run(string text) => text.WhenChanged(x => x.Length);
                }
            }
            """;

        await Assert.That((await AnalyzeAsync(source, [], false, LanguageVersion.Preview)).Select(static d => d.Id))
            .IsEquivalentTo([NoGeneratedBinding]);
    }

    /// <summary>Places calls in the model's usage method.</summary>
    /// <param name="calls">The calls.</param>
    /// <returns>The source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Calls(string calls) => Model.Replace("CALLS", calls, StringComparison.Ordinal);

    /// <summary>Runs the generators, then the analyzer over what they produced.</summary>
    /// <param name="source">The consumer source.</param>
    /// <param name="siblings">Other generators that run in the same pass.</param>
    /// <param name="intercept">Whether the build opts into interceptors.</param>
    /// <param name="languageVersion">The consumer's language version.</param>
    /// <returns>The diagnostics the analyzer reports.</returns>
    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        ImmutableArray<ISourceGenerator> siblings,
        bool intercept,
        LanguageVersion languageVersion = LanguageVersion.CSharp13)
    {
        var parseOptions = intercept
            ? TestHelper.InterceptingParseOptionsFor(languageVersion)
            : TestHelper.ParseOptionsFor(languageVersion);
        var compilation = TestHelper.CreateCompilation(source, parseOptions, false, "TestAssembly", []);
        var result = TestHelper.RunGeneratorBeside(compilation, parseOptions, siblings);

        return await result.OutputCompilation
            .WithAnalyzers([new analyzer::ReactiveUI.Binding.Analyzer.Analyzers.NoGeneratedBindingAnalyzer()])
            .GetAnalyzerDiagnosticsAsync()
            .ConfigureAwait(false);
    }

    /// <summary>Stands in for another source generator: it adds a property this generator never sees.</summary>
    /// <remarks>
    /// The property is ordinary output, which other generators do not see. Post-initialization output would be seen by
    /// every generator, which is not how a generator that reads the consumer's code adds members.
    /// </remarks>
    private sealed class NicknameGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IncrementalGeneratorInitializationContext context) =>
            context.RegisterSourceOutput(context.CompilationProvider, static (output, _) => output.AddSource(
                "Nickname.g.cs",
                "namespace App { public partial class Person { public string? Nickname { get; set; } } }"));
    }
}
