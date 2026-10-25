// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI.Binding.Tests.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Mixins;

/// <summary>
/// Tests the <c>InvokeCommand</c> surface the runtime library serves: the overload taking the command itself,
/// which has nothing to generate, and the overload naming a property, which reaches the runtime engine when no
/// generated dispatch claimed the call site.
/// </summary>
public class InvokeCommandTests
{
    /// <summary>The first value a test offers.</summary>
    private const string FirstValue = "first";

    /// <summary>The value a test offers after the first.</summary>
    private const string SecondValue = "second";

    /// <summary>The number of executions expected after offering two accepted values.</summary>
    private const int TwoExecutions = 2;

    /// <summary>The overload taking the command executes it with each value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_GivenTheCommand_ExecutesWithEachValue()
    {
        var command = new RecordingCommand();
        var values = new Subject<string>();

        using var invocation = values.InvokeCommand(command);

        values.OnNext(FirstValue);
        values.OnNext(SecondValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(TwoExecutions);
        await Assert.That(command.LastParameter).IsEqualTo(SecondValue);
    }

    /// <summary>The overload taking the command rejects a null command rather than dropping every value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_GivenNoCommand_Throws() =>
        await Assert.That(static () => new Subject<string>().InvokeCommand((ICommand)null!))
            .Throws<ArgumentNullException>();

    /// <summary>The overload naming a property executes the command that property holds.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_NamingAProperty_ExecutesThroughTheRuntimeEngine()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var command = new RecordingCommand();
        var viewModel = new DispatchStubViewModel { Run = command };
        var values = new Subject<string>();

        using var invocation = values.InvokeCommandUnsafe(viewModel, x => x.Run);

        values.OnNext(FirstValue);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo(FirstValue);
    }

    /// <summary>An absent target holds no property to observe, so the values are dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_NamingAPropertyOnNothing_DropsTheValues()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();

        var values = new Subject<string>();

        using var invocation = values.InvokeCommandUnsafe((DispatchStubViewModel?)null, x => x.Run);

        values.OnNext(FirstValue);

        await Assert.That(values.HasObservers).IsFalse();
    }

    /// <summary>A null selector is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_NullSelector_Throws() =>
        await Assert.That(static () =>
                new Subject<string>().InvokeCommandUnsafe(new DispatchStubViewModel(), null!))
            .Throws<ArgumentNullException>();

    /// <summary>A null sequence is rejected rather than deferred to the first value.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeCommand_NullSource_Throws() =>
        await Assert.That(static () =>
                ((IObservable<string>)null!).InvokeCommandUnsafe(new DispatchStubViewModel(), x => x.Run))
            .Throws<ArgumentNullException>();
}
