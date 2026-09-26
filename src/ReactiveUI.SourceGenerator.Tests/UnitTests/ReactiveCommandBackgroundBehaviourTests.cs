// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.Loader;
using System.Windows.Input;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Runs generated task commands to check which thread <c>RunInBackground</c> starts them on.</summary>
public class ReactiveCommandBackgroundBehaviourTests
{
    /// <summary>A view model whose commands report the thread they ran on.</summary>
    private const string Source = """
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;

        namespace Behaviour;

        public partial class ViewModel : ReactiveObject
        {
            public TaskCompletionSource<int> Ran { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public int Parameter { get; private set; }

            public bool CanCancel { get; private set; }

            [ReactiveCommand(RunInBackground = true)]
            private Task Background()
            {
                Ran.SetResult(Environment.CurrentManagedThreadId);
                return Task.CompletedTask;
            }

            [ReactiveCommand(RunInBackground = true)]
            private Task<int> WithParameter(int value, CancellationToken token)
            {
                Parameter = value;
                CanCancel = token.CanBeCanceled;
                Ran.SetResult(Environment.CurrentManagedThreadId);
                return Task.FromResult(value);
            }

            [ReactiveCommand]
            private Task Foreground()
            {
                Ran.SetResult(Environment.CurrentManagedThreadId);
                return Task.CompletedTask;
            }
        }
        """;

    /// <summary>The parameter the parameterised command is executed with.</summary>
    private const int CommandParameter = 42;

    /// <summary>How long a command is given to report that it ran.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>A task command with <c>RunInBackground</c> runs its method on another thread.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task RunInBackgroundStartsTheTaskOffTheCallingThread()
    {
        var viewModel = CreateViewModel();

        var (callingThread, ranOn) = Execute(viewModel, "BackgroundCommand", null);

        await Assert.That(ranOn).IsNotEqualTo(callingThread);
    }

    /// <summary>The background lambda passes the command parameter and a cancellable token to the method.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task RunInBackgroundPassesTheParameterAndToken()
    {
        var viewModel = CreateViewModel();

        var (callingThread, ranOn) = Execute(viewModel, "WithParameterCommand", CommandParameter);

        await Assert.That(ranOn).IsNotEqualTo(callingThread);
        await Assert.That((int)GetProperty(viewModel, "Parameter")).IsEqualTo(CommandParameter);
        await Assert.That((bool)GetProperty(viewModel, "CanCancel")).IsTrue();
    }

    /// <summary>A task command without <c>RunInBackground</c> still starts its method on the calling thread.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task WithoutRunInBackgroundTheTaskStartsOnTheCallingThread()
    {
        var viewModel = CreateViewModel();

        var (callingThread, ranOn) = Execute(viewModel, "ForegroundCommand", null);

        await Assert.That(ranOn).IsEqualTo(callingThread);
    }

    /// <summary>Executes a command from a dedicated thread and waits for its method to report the thread it ran on.</summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="commandName">The generated command property.</param>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>The thread that executed the command, and the thread the method ran on.</returns>
    /// <remarks>
    /// The executing thread stays alive until the method reports, so no other thread can share its id. A thread pool
    /// thread would not do: the pool could run the background work on the very thread that queued it.
    /// </remarks>
    private static (int CallingThread, int RanOn) Execute(object viewModel, string commandName, object? parameter)
    {
        var command = (ICommand)GetProperty(viewModel, commandName);
        var ran = ((TaskCompletionSource<int>)GetProperty(viewModel, "Ran")).Task;
        var callingThread = 0;
        var thread = new Thread(() =>
        {
            callingThread = Environment.CurrentManagedThreadId;
            command.Execute(parameter);
            _ = SpinWait.SpinUntil(() => ran.IsCompleted, Timeout);
        });
        thread.Start();
        thread.Join();

        return ran.IsCompletedSuccessfully
            ? (callingThread, ran.Result)
            : throw new TimeoutException($"{commandName} did not run.");
    }

    /// <summary>Reads a public property of the view model.</summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The property's value.</returns>
    private static object GetProperty(object viewModel, string name) => viewModel.GetType().GetProperty(name)!.GetValue(viewModel)!;

    /// <summary>Generates, compiles and loads the view model.</summary>
    /// <returns>An instance of the view model.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "SES1402", Justification = "The test loads the assembly it has just compiled from its own source.")]
    private static object CreateViewModel()
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "ReactiveCommandBackgroundBehaviour",
            [CSharpSyntaxTree.ParseText(Source, parseOptions)],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        _ = CSharpGeneratorDriver
            .Create([new ReactiveCommandGenerator().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        using var image = new MemoryStream();
        var emit = output.Emit(image);
        if (!emit.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emit.Diagnostics));
        }

        image.Position = 0;
        var assembly = new AssemblyLoadContext(nameof(ReactiveCommandBackgroundBehaviourTests), isCollectible: true).LoadFromStream(image);
        return Activator.CreateInstance(assembly.GetType("Behaviour.ViewModel", throwOnError: true)!)!;
    }
}
