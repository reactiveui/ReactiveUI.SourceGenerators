// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestClass.
/// </summary>
[ExcludeFromCodeCoverage]
[DataContract]
public partial class TestViewModel : ReactiveObject
{
    [JsonInclude]
    [DataMember]
    [ObservableAsProperty]
    private double _test2Property;

    [JsonInclude]
    [Reactive]
    [DataMember]
    private int _test1Property;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewModel"/> class.
    /// </summary>
    public TestViewModel()
    {
        Console.Out.WriteLine(Test1Command);
        Console.Out.WriteLine(Test2Command);
        Console.Out.WriteLine(Test3Command);
        Console.Out.WriteLine(Test4Command);
        Console.Out.WriteLine(Test5StringToIntCommand);
        Console.Out.WriteLine(Test6ArgOnlyCommand);
        Console.Out.WriteLine(Test7ObservableCommand);
        Console.Out.WriteLine(Test8ObservableCommand);
        Console.Out.WriteLine(Test9Command);
        Console.Out.WriteLine(Test10Command);
        Test1Command?.Execute().Subscribe();
        Test2Command?.Execute().Subscribe(r => Console.Out.WriteLine(r));
        Test3Command?.Execute().Subscribe();
        Test4Command?.Execute().Subscribe(r => Console.Out.WriteLine(r));
        Test5StringToIntCommand?.Execute("100").Subscribe(Console.Out.WriteLine);
        Test6ArgOnlyCommand?.Execute("Hello World").Subscribe();
        Test7ObservableCommand?.Execute().Subscribe();

        _test2PropertyHelper = Test8ObservableCommand!.ToProperty(this, x => x.Test2Property);

        Test8ObservableCommand?.Execute(100).Subscribe(Console.Out.WriteLine);
        Console.Out.WriteLine($"Test2Property Value: {Test2}");
        Console.Out.WriteLine($"Test2Property underlying Value: {_test2Property}");

        Test9Command?.ThrownExceptions.Subscribe(Console.Out.WriteLine);
        var cancel = Test9Command?.Execute().Subscribe();
        Task.Delay(1000).Wait();
        cancel?.Dispose();

        Test10Command?.Execute(200).Subscribe(r => Console.Out.WriteLine(r));
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    public static TestViewModel Instance { get; } = new();

    /// <summary>
    /// Gets the can execute test1.
    /// </summary>
    /// <value>
    /// The can execute test1.
    /// </value>
#pragma warning disable CA1822 // Mark members as static
    public IObservable<bool> CanExecuteTest1 => Observable.Return(true);
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Test1s this instance.
    /// </summary>
    [ReactiveCommand(CanExecute = nameof(CanExecuteTest1))]
    [property: JsonInclude]
    private void Test1() => Console.Out.WriteLine("Test1");

    /// <summary>
    /// Test2s this instance.
    /// </summary>
    /// <returns>Rectangle.</returns>
    [ReactiveCommand]
    private Point Test2() => default;

    /// <summary>
    /// Test3s the asynchronous.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ReactiveCommand]
    private async Task Test3Async() => await Task.Delay(0);

    /// <summary>
    /// Test4s the asynchronous.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ReactiveCommand]
    private async Task<Point> Test4Async() => await Task.FromResult(new Point(100, 100));

    /// <summary>
    /// Test5s the string to int.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <returns>int.</returns>
    [ReactiveCommand]
    private int Test5StringToInt(string str) => int.Parse(str);

    /// <summary>
    /// Test6s the argument only.
    /// </summary>
    /// <param name="str">The string.</param>
    [ReactiveCommand]
    private void Test6ArgOnly(string str) => Console.Out.WriteLine($">>> {str}");

    /// <summary>
    /// Test7s the observable.
    /// </summary>
    /// <returns>An Observable of Unit.</returns>
    [ReactiveCommand]
    private IObservable<Unit> Test7Observable() => Observable.Return(Unit.Default);

    /// <summary>
    /// Test8s the observable.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns>An Observable of int.</returns>
    [ReactiveCommand]
    private IObservable<double> Test8Observable(int i) => Observable.Return(i + 10.0);

    [ReactiveCommand]
    private async Task Test9Async(CancellationToken ct) => await Task.Delay(2000, ct);

    [ReactiveCommand]
    private async Task<Point> Test10Async(int size, CancellationToken ct) => await Task.FromResult(new Point(size, size));
}
