// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI.Binding.CommandBinding;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.CommandBinding;

/// <summary>Tests for <see cref="CommandInvoker"/>, which both generated and runtime InvokeCommand paths use.</summary>
public class CommandInvokerTests
{
    /// <summary>The first value a test offers.</summary>
    private const string FirstValue = "first";

    /// <summary>The value a test offers after the first.</summary>
    private const string SecondValue = "second";

    /// <summary>The number of executions expected after offering two accepted values.</summary>
    private const int TwoExecutions = 2;

    /// <summary>The message a faulted sequence carries.</summary>
    private const string FaultMessage = "faulted";

    /// <summary>Each value the sequence produces is offered to the command as its parameter.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_FixedCommand_ExecutesWithEachValue()
    {
        var command = new RecordingCommand();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, command);

        values.OnNext(FirstValue);
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(TwoExecutions);
        await Assert.That(command.LastParameter).IsEqualTo(SecondValue);
    }

    /// <summary>A value the command refuses is dropped, and a later accepted value still executes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_FixedCommand_RefusedValueIsDropped()
    {
        var command = new RecordingCommand { CanExecuteResult = false };
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, command);

        values.OnNext(FirstValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
        await Assert.That(command.LastQuestionedParameter).IsEqualTo(FirstValue);

        command.CanExecuteResult = true;
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>Disposing stops the executions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_FixedCommand_Disposed_StopsExecuting()
    {
        var command = new RecordingCommand();
        var values = new Subject<string>();

        CommandInvoker.Invoke(values, command).Dispose();

        values.OnNext(FirstValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>The command the sequence of commands last produced is the one executed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_ExecutesTheLatestCommand()
    {
        var first = new RecordingCommand();
        var second = new RecordingCommand();
        var commands = new Subject<ICommand?>();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        commands.OnNext(first);
        values.OnNext(FirstValue);
        commands.OnNext(second);
        values.OnNext(SecondValue);

        await Assert.That(first.ExecuteCount).IsEqualTo(1);
        await Assert.That(second.ExecuteCount).IsEqualTo(1);
        await Assert.That(second.LastParameter).IsEqualTo(SecondValue);
    }

    /// <summary>A command the sequence has not produced yet drops the values offered meanwhile.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_NoCommandYet_DropsTheValues()
    {
        var command = new RecordingCommand();
        var commands = new Subject<ICommand?>();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        values.OnNext(FirstValue);
        commands.OnNext(command);
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo(SecondValue);
    }

    /// <summary>A null command drops the values offered while it stands.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_NullCommand_DropsTheValues()
    {
        var command = new RecordingCommand();
        var commands = new Subject<ICommand?>();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        commands.OnNext(command);
        commands.OnNext(null);
        values.OnNext(FirstValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Replacing the command does not itself execute anything.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_NewCommandAlone_DoesNotExecute()
    {
        var command = new RecordingCommand();
        var commands = new Subject<ICommand?>();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        values.OnNext(FirstValue);
        commands.OnNext(command);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>Disposing stops observing the commands as well as the values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_Disposed_StopsExecuting()
    {
        var command = new RecordingCommand();
        var commands = new Subject<ICommand?>();
        var values = new Subject<string>();

        CommandInvoker.Invoke(values, commands).Dispose();

        commands.OnNext(command);
        values.OnNext(FirstValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
        await Assert.That(commands.HasObservers).IsFalse();
    }

    /// <summary>A fault in the values is surfaced rather than swallowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ValuesFault_SurfacesTheFault()
    {
        var command = new RecordingCommand();
        var values = new ManualObservable<string>();

        using var invocation = CommandInvoker.Invoke(values, command);

        await Assert.That(() => values.Observer!.OnError(new InvalidOperationException(FaultMessage)))
            .Throws<InvalidOperationException>();
    }

    /// <summary>A fault in the values is surfaced while a command is being observed, too.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_ValuesFault_SurfacesTheFault()
    {
        var commands = new Subject<ICommand?>();
        var values = new ManualObservable<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        await Assert.That(() => values.Observer!.OnError(new InvalidOperationException(FaultMessage)))
            .Throws<InvalidOperationException>();
    }

    /// <summary>A fault in the observed commands is surfaced rather than swallowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommandFaults_SurfacesTheFault()
    {
        var commands = new ManualObservable<ICommand?>();
        var values = new Subject<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        await Assert.That(() => commands.Observer!.OnError(new InvalidOperationException(FaultMessage)))
            .Throws<InvalidOperationException>();
    }

    /// <summary>A completed sequence of values leaves a fixed command untouched rather than faulting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_FixedCommand_ValuesComplete_DoesNotThrow()
    {
        var command = new RecordingCommand();
        var values = new ManualObservable<string>();

        using var invocation = CommandInvoker.Invoke(values, command);

        values.Observer!.OnCompleted();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>A completed sequence leaves the subscription in place rather than faulting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_SequencesComplete_DoesNotThrow()
    {
        var command = new RecordingCommand();
        var commands = new ManualObservable<ICommand?>();
        var values = new ManualObservable<string>();

        using var invocation = CommandInvoker.Invoke(values, commands);

        commands.Observer!.OnNext(command);
        commands.Observer!.OnCompleted();
        values.Observer!.OnCompleted();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>A null sequence is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_NullSource_Throws() =>
        await Assert.That(static () => CommandInvoker.Invoke<string>(null!, new RecordingCommand()))
            .Throws<ArgumentNullException>();

    /// <summary>A null command is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_NullCommand_Throws() =>
        await Assert.That(static () => CommandInvoker.Invoke(new Subject<string>(), (ICommand)null!))
            .Throws<ArgumentNullException>();

    /// <summary>A null sequence of commands is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_NullCommandSequence_Throws() =>
        await Assert.That(static () => CommandInvoker.Invoke(new Subject<string>(), (IObservable<ICommand?>)null!))
            .Throws<ArgumentNullException>();

    /// <summary>A null sequence is rejected by the observed-command overload too.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invoke_ObservedCommand_NullSource_Throws() =>
        await Assert.That(static () => CommandInvoker.Invoke<string>(null!, new Subject<ICommand?>()))
            .Throws<ArgumentNullException>();
}
