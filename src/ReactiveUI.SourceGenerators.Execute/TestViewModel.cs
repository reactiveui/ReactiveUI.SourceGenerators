// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestClass.
/// </summary>
/// <seealso cref="ReactiveObject" />
/// <seealso cref="IActivatableViewModel" />
/// <seealso cref="IDisposable" />
[DataContract]
public partial class TestViewModel : ReactiveObject, IActivatableViewModel, IDisposable
{
    private readonly IObservable<bool> _observable = Observable.Return(true);
    private readonly Subject<double?> _testSubject = new();
    private readonly Subject<double> _testNonNullSubject = new();

    [property: JsonInclude]
    [DataMember]
    [ObservableAsProperty]
    private double? _test2Property = 1.1d;

    [ObservableAsProperty(ReadOnly = false)]
    private double? _test11Property = 11.1d;

    [ObservableAsProperty(ReadOnly = false)]
    private double _test13Property = 11.1d;

    [property: Test(AParameter = "Test Input")]
    [Reactive]
    private double? _test12Property = 12.1d;

    [Reactive(SetModifier = AccessModifier.Protected)]
    [property: JsonInclude]
    [DataMember]
    private int _test1Property;
    private bool _disposedValue;

    [Reactive]
    private string _myStringProperty = "test";

    [property: JsonInclude]
    [DataMember]
    [Reactive(Inheritance = InheritanceModifier.Virtual, SetModifier = AccessModifier.Protected)]
    private string? _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewModel"/> class.
    /// </summary>
    public TestViewModel()
    {
        this.WhenActivated(disposables =>
        {
            Console.Out.WriteLine("Activated");
            _test11PropertyHelper = this.WhenAnyValue(x => x.Test12Property).ToProperty(this, x => x.Test11Property, out _).DisposeWith(disposables);
        });

        Console.Out.WriteLine("MyReadOnlyProperty before init");

        // only settable prior to init, after init it will be ignored.
        _myReadOnlyProperty = -1.0;
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);

        Console.Out.WriteLine("MyReadOnlyNonNullProperty before init");

        // only settable prior to init, after init it will be ignored.
        _myReadOnlyNonNullProperty = -5.0;
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);

        _observableAsPropertyTest2Property = 11223344;
        Console.Out.WriteLine(ObservableAsPropertyTest2Property);
        Console.Out.WriteLine(_observableAsPropertyTest2Property);

        _referenceTypeObservableProperty = default!;
        ReferenceTypeObservable = Observable.Return(new object());
        NullableReferenceTypeObservable = Observable.Return(new object());

        InitializeOAPH();

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

        Console.Out.WriteLine($"Test2Property default Value: {Test2Property}");
        _test2PropertyHelper = Test8ObservableCommand!.ToProperty(this, x => x.Test2Property);

        Test8ObservableCommand?.Execute(100).Subscribe(d => Console.Out.WriteLine(d));
        Console.Out.WriteLine($"Test2Property Value: {Test2Property}");
        Console.Out.WriteLine($"Test2Property underlying Value: {_test2Property}");
        Console.Out.WriteLine(ObservableAsPropertyTest2Property);

        Console.Out.WriteLine("MyReadOnlyProperty After Init");

        // setting this value should not update the _myReadOnlyPropertyHelper as the _testSubject has not been updated yet but the _myReadOnlyPropertyHelper should be updated with null upon init.
        _myReadOnlyProperty = -2.0;

        // null value expected as the _testSubject has not been updated yet, ignoring the private variable.
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        _testSubject.OnNext(10.0);

        // expected value 10 as the _testSubject has been updated.
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        _testSubject.OnNext(null);

        // expected value null as the _testSubject has been updated.
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);

        Console.Out.WriteLine("MyReadOnlyNonNullProperty After Init");

        // setting this value should not update the _myReadOnlyNonNullProperty as the _testNonNullSubject has not been updated yet but the _myReadOnlyNonNullPropertyHelper should be updated with null upon init.
        _myReadOnlyNonNullProperty = -2.0;

        // 0 value expected as the _testNonNullSubject has not been updated yet, ignoring the private variable.
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(11.0);

        // expected value 11 as the _testNonNullSubject has been updated.
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(default);

        Console.Out.WriteLine(_test13Property);
        Console.Out.WriteLine(Test13Property);
        Console.Out.WriteLine(_test13PropertyHelper);

        // expected value 0 as the _testNonNullSubject has been updated.
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);

        Test9Command?.ThrownExceptions.Subscribe(Console.Out.WriteLine);
        var cancel = Test9Command?.Execute().Subscribe();
        Task.Delay(1000).Wait();
        cancel?.Dispose();

        Test10Command?.Execute(200).Subscribe(r => Console.Out.WriteLine(r));
        TestPrivateCanExecuteCommand?.Execute().Subscribe();

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
    /// Gets the internal test property. Should not prompt to replace with INPC Reactive Property.
    /// </summary>
    /// <value>
    /// The test property.
    /// </value>
    [JsonInclude]
    public string? TestInternalSetProperty { get; internal set; } = "Test";

    /// <summary>
    /// Gets the test private set property. Should not prompt to replace with INPC Reactive Property.
    /// </summary>
    /// <value>
    /// The test private set property.
    /// </value>
    [JsonInclude]
    public string? TestPrivateSetProperty { get; private set; } = "Test";

    /// <summary>
    /// Gets or sets the test automatic property.
    /// </summary>
    /// <value>
    /// The test automatic property.
    /// </value>
    [JsonInclude]
    public string? TestAutoProperty { get; set; } = "Test, should prompt to replace with INPC Reactive Property";

    /// <summary>
    /// Gets or sets the reactive command test property. Should not prompt to replace with INPC Reactive Property.
    /// </summary>
    /// <value>
    /// The reactive command test property.
    /// </value>
    public ReactiveCommand<Unit, Unit>? ReactiveCommandTestProperty { get; set; }

    /// <summary>
    /// Gets or sets the reactive property test property. Should not prompt to replace with INPC Reactive Property.
    /// </summary>
    /// <value>
    /// The reactive property test property.
    /// </value>
    public ReactiveProperty<int>? ReactivePropertyTestProperty { get; set; }

    /// <summary>
    /// Gets the can execute test1.
    /// </summary>
    /// <value>
    /// The can execute test1.
    /// </value>
    public IObservable<bool> CanExecuteTest1 => ObservableAsPropertyTest2.Select(x => x > 0);

    /// <summary>
    /// Gets the observable as property test2.
    /// </summary>
    /// <value>
    /// The observable as property test2.
    /// </value>
    [ObservableAsProperty]
    [property: Test(AParameter = "Test Input")]
    public IObservable<int> ObservableAsPropertyTest2 => Observable.Return(9);

    /// <summary>
    /// Gets the Activator which will be used by the View when Activation/Deactivation occurs.
    /// </summary>
    public ViewModelActivator Activator { get; } = new();

    [ObservableAsProperty]
    private IObservable<object> ReferenceTypeObservable { get; }

    [ObservableAsProperty]
    private IObservable<object?> NullableReferenceTypeObservable { get; }

    /// <summary>
    /// Gets observables as property test.
    /// </summary>
    /// <returns>
    /// Observable of double.
    /// </returns>
    [ObservableAsProperty(PropertyName = "MyReadOnlyProperty")]
    public IObservable<double?> ObservableAsPropertyTest() => _testSubject;

    /// <summary>
    /// Observables as property test non null.
    /// </summary>
    /// <returns>Observable of double.</returns>
    [ObservableAsProperty(PropertyName = "MyReadOnlyNonNullProperty")]
    public IObservable<double> ObservableAsPropertyTestNonNull() => _testNonNullSubject;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _testSubject.Dispose();
                _testNonNullSubject.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Test1s this instance.
    /// </summary>
    [ReactiveCommand(CanExecute = nameof(CanExecuteTest1))]
    [property: JsonInclude]
    [property: Test(AParameter = "Test Input")]
    private void Test1() => Console.Out.WriteLine("Test1 Command Executed");

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
    private IObservable<double?> Test8Observable(int i) => Observable.Return<double?>(i + 10.0);

    [ReactiveCommand]
    private async Task Test9Async(CancellationToken ct) => await Task.Delay(2000, ct);

    [ReactiveCommand]
    private async Task<Point> Test10Async(int size, CancellationToken ct) => await Task.FromResult(new Point(size, size));

    [ReactiveCommand(CanExecute = nameof(_observable))]
    private void TestPrivateCanExecute() => Console.Out.WriteLine("TestPrivateCanExecute");
}
