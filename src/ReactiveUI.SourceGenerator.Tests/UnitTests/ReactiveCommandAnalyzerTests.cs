// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Unit tests for <see cref="ReactiveCommandAnalyzer" />.</summary>
public sealed class ReactiveCommandAnalyzerTests
{
    /// <summary>A method signature the generator cannot use.</summary>
    private const string InvalidSignatureId = "RXUISG0002";

    /// <summary>An async void command method.</summary>
    private const string AsyncVoidId = "RXUISG0008";

    /// <summary>A scheduler that does not resolve.</summary>
    private const string UnresolvedSchedulerId = "RXUISG0021";

    /// <summary>Commands covering every rule, each reported at the text it names.</summary>
    private const string Source = """
        using System.Threading;
        using System.Threading.Tasks;
        using ReactiveUI;
        using ReactiveUI.Primitives.Concurrency;
        using ReactiveUI.SourceGenerators;

        namespace Analyzed;

        public partial class ViewModel : ReactiveObject
        {
            private readonly ISequencer _sequencer = null!;

            private object NotAScheduler => null!;

            [ReactiveCommand]
            private void TwoParameters(int first, int second) { }

            [ReactiveCommand]
            private Task<int> ParameterAndToken(int value, CancellationToken token) => Task.FromResult(value);

            [ReactiveCommand]
            private Task TwoParametersAndToken(int first, int second, CancellationToken token) => Task.CompletedTask;

            [ReactiveCommand]
            private async void FireAndForget() => await Task.Yield();

            [ReactiveCommand]
            private async Task Awaited() => await Task.Yield();

            [ReactiveCommand(OutputScheduler = "RxSchedulers.MainThreadScheduler", BackgroundScheduler = "RxSchedulers.TaskpoolScheduler")]
            private void ShortNames() { }

            [ReactiveCommand(OutputScheduler = nameof(_sequencer))]
            private void Member() { }

            [ReactiveCommand(OutputScheduler = "Missing")]
            private void MissingScheduler() { }

            [ReactiveCommand(BackgroundScheduler = nameof(NotAScheduler))]
            private void WrongType() { }

            private readonly Holder _holder = new();

            [ReactiveCommand(OutputScheduler = "_holder.Sequencer")]
            private void AnotherObjectsMember() { }

            [ReactiveCommand(OutputScheduler = "not (valid")]
            private void Unparsable() { }

            [ReactiveCommand(OutputScheduler = "RxSchedulers")]
            private void TypeName() { }

            [System.Obsolete]
            private void NotACommand() { }
        }

        public sealed class Holder
        {
            public ISequencer Sequencer => null!;
        }
        """;

    /// <summary>Only a method with more than one parameter besides a CancellationToken is reported.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ReportsMethodsWithMoreThanOneCommandParameter()
    {
        var reported = await GetReported(InvalidSignatureId);

        await Assert.That(reported).IsEquivalentTo(["TwoParameters", "TwoParametersAndToken"]);
    }

    /// <summary>Only an async void method is reported; an async Task method is not.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ReportsAsyncVoidMethods()
    {
        var reported = await GetReported(AsyncVoidId);

        await Assert.That(reported).IsEquivalentTo(["FireAndForget"]);
    }

    /// <summary>Only schedulers the generator cannot resolve are reported; short names and members are not.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ReportsSchedulersThatDoNotResolve()
    {
        var reported = await GetReported(UnresolvedSchedulerId);

        await Assert.That(reported).IsEquivalentTo(["\"Missing\"", "nameof(NotAScheduler)", "\"_holder.Sequencer\"", "\"not (valid\"", "\"RxSchedulers\""]);
    }

    /// <summary>Without ReactiveUI's command factories no scheduler can resolve, so every one is reported.</summary>
    /// <param name="reactiveUiStub">Source standing in for ReactiveUI: none, or a command type without factories.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    [Arguments("")]
    [Arguments("namespace ReactiveUI { public static class ReactiveCommand { public static void Create() { } } }")]
    public async Task ReportsSchedulersWithoutReactiveUiCommandFactories(string reactiveUiStub)
    {
        var source = reactiveUiStub + """

            namespace Analyzed
            {
                public partial class ViewModel
                {
                    public static object Anything => null!;

                    [ReactiveUI.SourceGenerators.ReactiveCommand(OutputScheduler = "Anything")]
                    private void Command() { }
                }
            }
            """;
        var references = TestCompilationReferences.CreateForAssemblies(typeof(object).Assembly, typeof(ReactiveCommandAttribute).Assembly);

        var reported = await GetReported(UnresolvedSchedulerId, source, references);

        await Assert.That(reported).IsEquivalentTo(["\"Anything\""]);
    }

    /// <summary>A scheduler argument whose value is not a string, or does not compile, is left to the compiler.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task IgnoresSchedulerArgumentsThatAreNotStrings()
    {
        const string source = """
            using ReactiveUI.SourceGenerators;

            public partial class ViewModel
            {
                [ReactiveCommand(OutputScheduler = 42)]
                private void Command() { }

                [ReactiveCommand(OutputScheduler = UndefinedName)]
                private void Unbound() { }
            }
            """;

        var reported = await GetReported(UnresolvedSchedulerId, source, TestCompilationReferences.CreateDefault());

        await Assert.That(reported).IsEmpty();
    }

    /// <summary>A null analysis context is rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task InitializeRejectsANullContext() =>
        await Assert.That(static () => new ReactiveCommandAnalyzer().Initialize(null!)).Throws<ArgumentNullException>();

    /// <summary>The analyzer reports its three rules.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task SupportsItsRules()
    {
        var ids = new List<string>();
        foreach (var descriptor in new ReactiveCommandAnalyzer().SupportedDiagnostics)
        {
            ids.Add(descriptor.Id);
        }

        await Assert.That(ids).IsEquivalentTo([InvalidSignatureId, AsyncVoidId, UnresolvedSchedulerId]);
    }

    /// <summary>Analyzes <see cref="Source"/> and gets the text each diagnostic with an ID is reported at.</summary>
    /// <param name="id">The diagnostic ID.</param>
    /// <returns>The source text at each reported location.</returns>
    private static Task<List<string>> GetReported(string id) => GetReported(id, Source, TestCompilationReferences.CreateDefault());

    /// <summary>Analyzes a source and gets the text each diagnostic with an ID is reported at.</summary>
    /// <param name="id">The diagnostic ID.</param>
    /// <param name="source">The source to analyze.</param>
    /// <param name="references">The compilation's references.</param>
    /// <returns>The source text at each reported location.</returns>
    private static async Task<List<string>> GetReported(string id, string source, ImmutableArray<MetadataReference> references)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13));
        var compilation = CSharpCompilation.Create(
            nameof(ReactiveCommandAnalyzerTests),
            [tree],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = await compilation.WithAnalyzers([new ReactiveCommandAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        var text = await tree.GetTextAsync();

        var reported = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == id)
            {
                reported.Add(text.ToString(diagnostic.Location.SourceSpan));
            }
        }

        return reported;
    }
}
