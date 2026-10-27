// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>Tests that the generated InvokeCommand code executes the command at runtime.</summary>
public class InvokeCommandTests
{
    /// <summary>The value the stream is primed with.</summary>
    private const string InitialValue = "initial";

    /// <summary>The value the stream produces after the first one.</summary>
    private const string SecondValue = "second";

    /// <summary>The number of executions a test expects after offering two values.</summary>
    private const int TwoExecutions = 2;

    /// <summary>The first value the chain test offers.</summary>
    private const int FirstNumber = 1;

    /// <summary>The value the chain test offers after replacing the child.</summary>
    private const int SecondNumber = 2;

    /// <summary>Verifies each value the stream produces is offered to the command as its parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_ExecutesWithEachValue()
    {
        var viewModel = new SharedScenarios.InvokeCommand.CommandProperty.MyViewModel();
        var command = new TrackingCommand();
        viewModel.Save = command;
        var values = new Subject<string>();

        using var invocation = InvokeCommandScenarios.CommandProperty(values, viewModel);

        values.OnNext(InitialValue);
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(TwoExecutions);
        await Assert.That(command.LastParameter).IsEqualTo(SecondValue);
    }

    /// <summary>Verifies a command assigned after the subscription still receives the values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_CommandAssignedAfterSubscribing_Executes()
    {
        var viewModel = new SharedScenarios.InvokeCommand.CommandProperty.MyViewModel();
        var command = new TrackingCommand();
        var values = new Subject<string>();

        using var invocation = InvokeCommandScenarios.CommandProperty(values, viewModel);

        viewModel.Save = command;
        values.OnNext(InitialValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies a value a command refuses is dropped rather than executed or held.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_CommandRefusesTheValue_DoesNotExecute()
    {
        var viewModel = new SharedScenarios.InvokeCommand.CommandProperty.MyViewModel();
        var command = new TrackingCommand { CanExecuteResult = false };
        viewModel.Save = command;
        var values = new Subject<string>();

        using var invocation = InvokeCommandScenarios.CommandProperty(values, viewModel);

        values.OnNext(InitialValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);

        command.CanExecuteResult = true;
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Verifies a view model with no command assigned drops the values it is offered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_NoCommandAssigned_DropsTheValues()
    {
        var viewModel = new SharedScenarios.InvokeCommand.CommandProperty.MyViewModel();
        var values = new Subject<string>();

        using var invocation = InvokeCommandScenarios.CommandProperty(values, viewModel);

        values.OnNext(InitialValue);

        await Assert.That(viewModel.Save).IsNull();
    }

    /// <summary>Verifies disposing the invocation stops executing the command.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandProperty_Disposed_StopsExecuting()
    {
        var viewModel = new SharedScenarios.InvokeCommand.CommandProperty.MyViewModel();
        var command = new TrackingCommand();
        viewModel.Save = command;
        var values = new Subject<string>();

        var invocation = InvokeCommandScenarios.CommandProperty(values, viewModel);
        invocation.Dispose();

        values.OnNext(InitialValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Verifies the command is reached through the chain, and followed when the chain changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepCommandPath_FollowsTheChain()
    {
        var viewModel = new SharedScenarios.InvokeCommand.DeepCommandPath.MyViewModel();
        var first = new TrackingCommand();
        var second = new TrackingCommand();
        viewModel.Child = new SharedScenarios.InvokeCommand.DeepCommandPath.ChildViewModel { Save = first };
        var values = new Subject<int>();

        using var invocation = InvokeCommandScenarios.DeepCommandPath(values, viewModel);

        values.OnNext(FirstNumber);

        await Assert.That(first.ExecuteCount).IsEqualTo(1);
        await Assert.That(first.LastParameter).IsEqualTo(FirstNumber);

        viewModel.Child = new SharedScenarios.InvokeCommand.DeepCommandPath.ChildViewModel { Save = second };
        values.OnNext(SecondNumber);

        await Assert.That(first.ExecuteCount).IsEqualTo(1);
        await Assert.That(second.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>An <see cref="ICommand"/> that records what it was offered.</summary>
    private sealed class TrackingCommand : ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { /* CanExecute is read per emission rather than tracked. */ }
            remove { /* CanExecute is read per emission rather than tracked. */ }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="CanExecute"/> returns <see langword="true"/>.</summary>
        public bool CanExecuteResult { get; set; } = true;

        /// <summary>Gets the number of times <see cref="Execute"/> has been called.</summary>
        public int ExecuteCount { get; private set; }

        /// <summary>Gets the parameter passed to the most recent <see cref="Execute"/> call.</summary>
        public object? LastParameter { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter) => CanExecuteResult;

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }
    }
}
