// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using DynamicData;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Contains initialization and sample execution helpers for <see cref="TestViewModel"/>.</summary>
public partial class TestViewModel
{
    /// <summary>Copies the input nested object to the corresponding output nested object.</summary>
    /// <param name="class1">The nested object to copy.</param>
    /// <returns>The copied nested object, or <see langword="null"/> when the input is null.</returns>
    [ReactiveCommand]
    private static Execute.Nested2.Class1? SetProperty(Execute.Nested1.Class1? class1) =>
        class1 is null ? null : new() { Property1 = class1.Property1 };

    /// <summary>Creates the helper that projects the test command output.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<double?> CreateTest2PropertyHelper() =>
        Test8ObservableCommand!.ToProperty(this, x => x.Test2Property);

    /// <summary>Creates the helper that projects the activated reactive property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<double?> CreateTest11PropertyHelper() =>
        this.WhenAnyValue(x => x.Test12Property).ToProperty(this, x => x.Test11Property, out _);

    /// <summary>Creates the helper that projects the protected observable property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<double> CreateObservableAsPropertyTest3PropertyHelper() =>
        this.WhenAnyValue(x => x.Test13Property).ToProperty(this, x => x.ObservableAsPropertyTest3Property);

    /// <summary>Creates the helper that projects the partial observable property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<int> CreateObservableAsPropertyFromPropertyHelper() =>
        _fromPartialTestSubject.ToProperty(this, x => x.ObservableAsPropertyFromProperty);

    /// <summary>Creates the helper that projects the active PLC identifier.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<string?> CreatePlcActiveHelper() =>
        this.WhenAnyValue(x => x.PartialRequiredPropertyTest).ToProperty(this, nameof(PLCActive));

    /// <summary>Creates the helper that projects the PLC status message.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<string> CreatePlcStatusHelper() =>
        this.WhenAnyValue(x => x.PLCActive).Select(static x => x ?? string.Empty).ToProperty(this, nameof(PLCStatus));

    /// <summary>Creates the helper that projects the PLC port.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<int> CreatePlcPortHelper() =>
        this.WhenAnyValue(x => x.Test1Property).ToProperty(this, nameof(PLCPort));

    /// <summary>Creates the helper that projects the PLC instance.</summary>
    /// <returns>The initialized observable property helper.</returns>
    private ObservableAsPropertyHelper<PLCInstance> CreatePlcInstanceHelper() =>
        this.WhenAnyValue(x => x.PlcInstanceCore).ToProperty(this, nameof(InstanceOfPLC));

    /// <summary>Registers the lifecycle activation subscriptions.</summary>
    private void RegisterActivation() =>
        this.WhenActivated(disposables =>
        {
            Console.Out.WriteLine("Activated");
            _test11PropertyHelper?.Dispose();
            _test11PropertyHelper = CreateTest11PropertyHelper();
            disposables(_test11PropertyHelper);
            disposables(GetDataCommand.Do(static _ => Console.Out.WriteLine("GetDataCommand Executed")).Subscribe());
            disposables(GetDataCommand.Execute().Subscribe());
        });

    /// <summary>Initializes observable property values before sample commands execute.</summary>
    private void InitializeObservableProperties()
    {
        Console.Out.WriteLine("MyReadOnlyProperty before init");
        _myReadOnlyProperty = NegativeInitialReadOnlyValue;
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        Console.Out.WriteLine("MyReadOnlyNonNullProperty before init");
        _myReadOnlyNonNullProperty = NegativeNonNullReadOnlyValue;
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
        _observableAsPropertyTest2Property = ObservableAsPropertyInitialValue;
        Console.Out.WriteLine(ObservableAsPropertyTest2Property);
        Console.Out.WriteLine(_observableAsPropertyTest2Property);
        InitializeOAPH();
    }

    /// <summary>Exercises the initial generated command set.</summary>
    private void ExerciseInitialCommands()
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
        Test2Command?.Execute().Subscribe(static r => Console.Out.WriteLine(r));
        Test3Command?.Execute().Subscribe();
        Test4Command?.Execute().Subscribe(static r => Console.Out.WriteLine(r));
        Test5StringToIntCommand?.Execute("100").Subscribe(Console.Out.WriteLine);
        Test6ArgOnlyCommand?.Execute("Hello World").Subscribe();
        Test7ObservableCommand?.Execute().Subscribe();
        Console.Out.WriteLine($"Test2Property default Value: {Test2Property}");
        Test8ObservableCommand?.Execute(CommandArgumentValue).Subscribe(static d => Console.Out.WriteLine(d));
        Console.Out.WriteLine($"Test2Property Value: {Test2Property}");
        Console.Out.WriteLine($"Test2Property underlying Value: {_test2Property}");
        Console.Out.WriteLine(ObservableAsPropertyTest2Property);
    }

    /// <summary>Exercises the nullable and non-null observable property samples.</summary>
    private void ExerciseReadOnlyProperties()
    {
        Console.Out.WriteLine("MyReadOnlyProperty After Init");
        _myReadOnlyProperty = NegativeUpdatedReadOnlyValue;
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        _testSubject.OnNext(FirstObservedDoubleValue);
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        _testSubject.OnNext(null);
        Console.Out.WriteLine(MyReadOnlyProperty);
        Console.Out.WriteLine(_myReadOnlyProperty);
        Console.Out.WriteLine("MyReadOnlyNonNullProperty After Init");
        _myReadOnlyNonNullProperty = NegativeUpdatedReadOnlyValue;
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(SecondObservedDoubleValue);
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(default);
        Console.Out.WriteLine(_test13Property);
        Console.Out.WriteLine(Test13Property);
        Console.Out.WriteLine(_test13PropertyHelper);
        Console.Out.WriteLine(MyReadOnlyNonNullProperty);
        Console.Out.WriteLine(_myReadOnlyNonNullProperty);
    }

    /// <summary>Exercises the remaining commands and observable property updates.</summary>
    private void ExerciseRemainingCommands()
    {
        Test9Command?.ThrownExceptions.Subscribe(Console.Out.WriteLine);
        var cancel = Test9Command?.Execute().Subscribe();
        cancel?.Dispose();
        Test10Command?.Execute(CommandResultValue).Subscribe(static r => Console.Out.WriteLine(r));
        TestPrivateCanExecuteCommand?.Execute().Subscribe();
        Console.Out.WriteLine($"Observable unset, value should be 10, value is : {ObservableAsPropertyFromProperty}");
        _fromPartialTestSubject.OnNext(ObservableUpdatedValue);
        Console.Out.WriteLine($"Observable updated, value should be 11, value is : {ObservableAsPropertyFromProperty}");
        _ = Console.ReadLine();
    }

    /// <summary>Registers the subscription that maintains the visible people collection.</summary>
    private void RegisterPeopleSubscription() =>
        _ = this.WhenAnyValue(vm => vm.People)
            .Subscribe(people => people
                .AsObservableChangeSet()
                .AutoRefresh(x => x.Deleted)
                .Filter(static x => !x.Deleted)
                .Bind(out _visiblePeople)
                .Subscribe());
}
