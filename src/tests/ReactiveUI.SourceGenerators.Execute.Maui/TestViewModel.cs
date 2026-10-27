// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides a MAUI sample that exercises generated reactive members and commands.</summary>
[ExcludeFromCodeCoverage]
[DataContract]
public partial class TestViewModel : ReactiveObject
{
    /// <summary>Represents the argument supplied to the observable command.</summary>
    private const int ObservableCommandArgument = 100;

    /// <summary>Represents the argument supplied to the point-producing command.</summary>
    private const int PointCommandArgument = 200;

    /// <summary>Represents the coordinate used by the point-producing sample command.</summary>
    private const int PointCoordinate = 100;

    /// <summary>Represents the offset applied by the observable sample command.</summary>
    private const double ObservableCommandOffset = 10.0D;

    /// <summary>Represents the delay before the cancellation subscription is disposed.</summary>
    private const int CancellationObservationDelayMilliseconds = 1_000;

    /// <summary>Represents the duration of the cancellable sample command.</summary>
    private const int CancellableCommandDelayMilliseconds = 2_000;

    /// <summary>Backs <see cref="Test2Property"/>.</summary>
    /// <remarks>
    /// <c>[ObservableAsProperty]</c> moved to ReactiveUI.Binding, so the helper is written by hand with ReactiveUI's
    /// <c>ToProperty</c>. On ReactiveUI.Binding, mark a <c>partial</c> property <c>[ObservableAsProperty]</c> instead.
    /// </remarks>
    private ObservableAsPropertyHelper<double>? _test2PropertyHelper;

    /// <summary>Stores the reactive property sample value.</summary>
    [JsonInclude]
    [Reactive]
    [DataMember]
    private int _test1Property;

    /// <summary>Gets the initialized MAUI command sample.</summary>
    public static TestViewModel Instance { get; } = CreateInitializedInstance();

    /// <summary>Gets an observable that enables the first sample command.</summary>
    public IObservable<bool> CanExecuteTest1 => Observable.Return(_test1Property >= 0);

    /// <summary>Gets the latest value the observable command produced.</summary>
    [JsonInclude]
    [DataMember]
    public double Test2Property => _test2PropertyHelper?.Value ?? default;

    /// <summary>Writes a message when the first generated command executes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand(CanExecute = nameof(CanExecuteTest1))]
    [property: JsonInclude]
    private static void Test1() => Trace.TraceInformation("{0}", "Test1");

    /// <summary>Returns the default point from the second generated command.</summary>
    /// <returns>The default point value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static Point Test2() => default;

    /// <summary>Returns a completed task from the third generated command.</summary>
    /// <returns>A task that completes immediately.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static Task Test3Async() => Task.CompletedTask;

    /// <summary>Returns a point asynchronously from the fourth generated command.</summary>
    /// <returns>A task that produces the configured point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static Task<Point> Test4Async() => Task.FromResult(new Point(PointCoordinate, PointCoordinate));

    /// <summary>Converts a command string argument into an integer result.</summary>
    /// <param name="str">The value to convert.</param>
    /// <returns>The converted integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static int Test5StringToInt(string str) => int.Parse(str);

    /// <summary>Writes the command argument to the console.</summary>
    /// <param name="str">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static void Test6ArgOnly(string str) => Trace.TraceInformation("{0}", $">>> {str}");

    /// <summary>Returns a unit value through an observable command.</summary>
    /// <returns>An observable that returns a unit value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static IObservable<Unit> Test7Observable() => Observable.Return(Unit.Default);

    /// <summary>Returns an offset command value through an observable.</summary>
    /// <param name="i">The input value to offset.</param>
    /// <returns>An observable that returns the offset value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static IObservable<double> Test8Observable(int i) => Observable.Return(i + ObservableCommandOffset);

    /// <summary>Delays until cancellation is requested by the generated command.</summary>
    /// <param name="ct">The token that cancels the delay.</param>
    /// <returns>A task that represents the cancellable delay.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static Task Test9Async(CancellationToken ct) => Task.Delay(CancellableCommandDelayMilliseconds, ct);

    /// <summary>Returns a point based on the command argument.</summary>
    /// <param name="size">The width and height of the point.</param>
    /// <param name="ct">The cancellation token accepted by the command.</param>
    /// <returns>A task that returns the requested point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ReactiveCommand]
    private static Task<Point> Test10Async(int size, CancellationToken ct) => Task.FromResult(new Point(size, size));

    /// <summary>Creates and initializes the singleton after its constructor has completed.</summary>
    /// <returns>The initialized command sample.</returns>
    private static TestViewModel CreateInitializedInstance()
    {
        var instance = new TestViewModel();
        instance.InitializeCommandCoverage();
        return instance;
    }

    /// <summary>Schedules asynchronous disposal of the cancellable command subscription.</summary>
    /// <param name="subscription">The command subscription to dispose after the observation delay.</param>
    private static void ScheduleCancellation(IDisposable? subscription)
    {
        if (subscription is null)
        {
            return;
        }

        _ = DisposeAfterDelayAsync(subscription);
    }

    /// <summary>Disposes a subscription after allowing the command to run briefly.</summary>
    /// <param name="subscription">The subscription to dispose.</param>
    /// <returns>A task that represents the delayed disposal.</returns>
    private static async Task DisposeAfterDelayAsync(IDisposable subscription)
    {
        await Task.Delay(CancellationObservationDelayMilliseconds).ConfigureAwait(false);
        subscription.Dispose();
    }

    /// <summary>Exercises every command shape generated for the MAUI sample.</summary>
    private void InitializeCommandCoverage()
    {
        WriteGeneratedCommands();
        ExecuteInitialCommands();
        InitializeObservableProperty();
        ExecuteRemainingCommands();
    }

    /// <summary>Writes the generated commands so their creation is visible in the sample output.</summary>
    private void WriteGeneratedCommands()
    {
        Trace.TraceInformation("{0}", Test1Command);
        Trace.TraceInformation("{0}", Test2Command);
        Trace.TraceInformation("{0}", Test3Command);
        Trace.TraceInformation("{0}", Test4Command);
        Trace.TraceInformation("{0}", Test5StringToIntCommand);
        Trace.TraceInformation("{0}", Test6ArgOnlyCommand);
        Trace.TraceInformation("{0}", Test7ObservableCommand);
        Trace.TraceInformation("{0}", Test8ObservableCommand);
        Trace.TraceInformation("{0}", Test9Command);
        Trace.TraceInformation("{0}", Test10Command);
    }

    /// <summary>Executes the generated commands that do not require cancellation.</summary>
    private void ExecuteInitialCommands()
    {
        _ = Test1Command?.Execute().Subscribe();
        _ = Test2Command?.Execute().Subscribe(static result => Trace.TraceInformation("{0}", result));
        _ = Test3Command?.Execute().Subscribe();
        _ = Test4Command?.Execute().Subscribe(static result => Trace.TraceInformation("{0}", result));
        _ = Test5StringToIntCommand?.Execute("100").Subscribe(static value => Trace.TraceInformation("{0}", value));
        _ = Test6ArgOnlyCommand?.Execute("Hello World").Subscribe();
        _ = Test7ObservableCommand?.Execute().Subscribe();
    }

    /// <summary>Initializes and exercises the observable-as-property sample.</summary>
    private void InitializeObservableProperty()
    {
        _test2PropertyHelper = Test8ObservableCommand!.ToProperty(this, static viewModel => viewModel.Test2Property);
        _ = Test8ObservableCommand.Execute(ObservableCommandArgument).Subscribe(static value => Trace.TraceInformation("{0}", value));
        Trace.TraceInformation("{0}", $"Test2Property Value: {Test2Property}");
    }

    /// <summary>Executes the cancellable and argument-taking generated commands.</summary>
    private void ExecuteRemainingCommands()
    {
        _ = Test9Command?.ThrownExceptions.Subscribe(static value => Trace.TraceInformation("{0}", value));
        ScheduleCancellation(Test9Command?.Execute().Subscribe());
        _ = Test10Command?.Execute(PointCommandArgument).Subscribe(static result => Trace.TraceInformation("{0}", result));
    }
}
