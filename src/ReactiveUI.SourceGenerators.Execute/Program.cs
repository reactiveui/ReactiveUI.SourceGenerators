// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// EntryPoint.
/// </summary>
public static class EntryPoint
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    public static void Main() => new TestClass();
}

/// <summary>
/// TestClass.
/// </summary>
[DataContract]
public partial class TestClass : ReactiveObject
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
    /// Initializes a new instance of the <see cref="TestClass"/> class.
    /// </summary>
    public TestClass()
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
    /// Gets the can execute test1.
    /// </summary>
    /// <value>
    /// The can execute test1.
    /// </value>
    public IObservable<bool> CanExecuteTest1 => Observable.Return(true);

    /// <summary>
    /// Test1s this instance.
    /// </summary>
    [ReactiveCommand(CanExecute = nameof(CanExecuteTest1))]
    private void Test1() => Console.Out.WriteLine("Test1");

    /// <summary>
    /// Test2s this instance.
    /// </summary>
    /// <returns>Rectangle.</returns>
    [ReactiveCommand]
    private Rectangle Test2() => default;

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
    private async Task<Rectangle> Test4Async() => await Task.FromResult(new Rectangle(0, 0, 100, 100));

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
    private async Task<Rectangle> Test10Async(int size, CancellationToken ct) => await Task.FromResult(new Rectangle(0, 0, size, size));
}

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
