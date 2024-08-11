// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
[DataContract]
public partial class TestViewModel : ReactiveObject
{
    [JsonInclude]
    [DataMember]
    [ObservableAsProperty]
    private double _test2Property = 1.1d;

    [JsonInclude]
    [Reactive]
    [DataMember]
    private int _test1Property = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewModel"/> class.
    /// </summary>
    public TestViewModel()
    {
        InitializeCommands();

        Console.Out.WriteLine(Test1Command);
        Console.Out.WriteLine(Test2Command);
        Console.Out.WriteLine(Test3AsyncCommand);
        Console.Out.WriteLine(Test4AsyncCommand);
        Console.Out.WriteLine(Test5StringToIntCommand);
        Console.Out.WriteLine(Test6ArgOnlyCommand);
        Console.Out.WriteLine(Test7ObservableCommand);
        Console.Out.WriteLine(Test8ObservableCommand);
        Console.Out.WriteLine(Test9AsyncCommand);
        Console.Out.WriteLine(Test10AsyncCommand);
        Test1Command?.Execute().Subscribe();
        Test2Command?.Execute().Subscribe(r => Console.Out.WriteLine(r));
        Test3AsyncCommand?.Execute().Subscribe();
        Test4AsyncCommand?.Execute().Subscribe(r => Console.Out.WriteLine(r));
        Test5StringToIntCommand?.Execute("100").Subscribe(Console.Out.WriteLine);
        Test6ArgOnlyCommand?.Execute("Hello World").Subscribe();
        Test7ObservableCommand?.Execute().Subscribe();

        Console.Out.WriteLine($"Test2Property default Value: {Test2Property}");
        _test2PropertyHelper = Test8ObservableCommand!.ToProperty(this, x => x.Test2Property);

        Test8ObservableCommand?.Execute(100).Subscribe(Console.Out.WriteLine);
        Console.Out.WriteLine($"Test2Property Value: {Test2Property}");
        Console.Out.WriteLine($"Test2Property underlying Value: {_test2Property}");

        Test9AsyncCommand?.ThrownExceptions.Subscribe(Console.Out.WriteLine);
        var cancel = Test9AsyncCommand?.Execute().Subscribe();
        Task.Delay(1000).Wait();
        cancel?.Dispose();

        Test10AsyncCommand?.Execute(200).Subscribe(r => Console.Out.WriteLine(r));

        Console.ReadLine();
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    public static TestViewModel Instance { get; } = new();

    /// <summary>
    /// Gets or sets the test property.
    /// </summary>
    /// <value>
    /// The test property.
    /// </value>
    [JsonInclude]
    public string? TestProperty { get; set; } = "Test";

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
    [property: Test(AParameter = "Test Input")]
    private void Test1() => Console.Out.WriteLine("Test1");

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
