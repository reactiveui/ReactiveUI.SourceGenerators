// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using ReactiveUI.SourceGenerators;
using Splat;

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

    /// <summary>Writes a value to the application log.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Log<T>(T value) => LogHost.Default.Info(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{value}"));

    /// <summary>Creates the helper that projects the test command output.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<double?> CreateTest2PropertyHelper() =>
        Test8ObservableCommand!.ToProperty(this, static x => x.Test2Property, initialValue: Test2InitialValue);

    /// <summary>Creates the helper that projects the activated reactive property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<double?> CreateTest11PropertyHelper() =>
        this.WhenAnyValue(static x => x.Test12Property).ToProperty(this, static x => x.Test11Property, initialValue: Test11InitialValue);

    /// <summary>Creates the helper that projects the protected observable property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<double> CreateObservableAsPropertyTest3PropertyHelper() =>
        this.WhenAnyValue(static x => x.Test13Property).ToProperty(this, static x => x.ObservableAsPropertyTest3Property);

    /// <summary>Creates the helper that projects the partial observable property.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<int> CreateObservableAsPropertyFromPropertyHelper() =>
        _fromPartialTestSubject.ToProperty(this, static x => x.ObservableAsPropertyFromProperty, initialValue: FromPropertyInitialValue);

    /// <summary>Creates the helper that projects the active PLC identifier.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<string?> CreatePlcActiveHelper() =>
        this.WhenAnyValue(static x => x.PartialRequiredPropertyTest).ToProperty(this, nameof(PLCActive), initialValue: "Not Connected");

    /// <summary>Creates the helper that projects the PLC status message.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<string> CreatePlcStatusHelper() =>
        this.WhenAnyValue(static x => x.PLCActive).Select(static x => x ?? string.Empty).ToProperty(this, nameof(PLCStatus), initialValue: string.Empty);

    /// <summary>Creates the helper that projects the PLC port.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<int> CreatePlcPortHelper() =>
        this.WhenAnyValue(static x => x.Test1Property).ToProperty(this, nameof(PLCPort), initialValue: PlcInitialPort);

    /// <summary>Creates the helper that projects the PLC instance.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<PLCInstance> CreatePlcInstanceHelper() =>
        this.WhenAnyValue(static x => x.PlcInstanceCore).ToProperty(this, nameof(InstanceOfPLC), initialValue: new());

    /// <summary>Creates the helper that follows the non-null reference type observable.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<object> CreateReferenceTypeHelper() =>
        ReferenceTypeObservable.ToProperty(this, nameof(ReferenceTypeObservableProperty), initialValue: new());

    /// <summary>Creates the helper that follows the nullable reference type observable.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<object?> CreateNullableReferenceTypeHelper() =>
        NullableReferenceTypeObservable.ToProperty(this, nameof(NullableReferenceTypeObservableProperty));

    /// <summary>Creates the helper that follows <see cref="ObservableAsPropertyTest2"/>.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<int> CreateObservableAsPropertyTest2PropertyHelper() =>
        ObservableAsPropertyTest2.ToProperty(this, nameof(ObservableAsPropertyTest2Property), initialValue: ObservableAsPropertyInitialValue);

    /// <summary>Creates the helper that follows <see cref="ObservableAsPropertyTest"/>.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<double?> CreateMyReadOnlyPropertyHelper() =>
        ObservableAsPropertyTest().ToProperty(this, nameof(MyReadOnlyProperty), initialValue: NegativeInitialReadOnlyValue);

    /// <summary>Creates the helper that follows <see cref="ObservableAsPropertyTestNonNull"/>.</summary>
    /// <returns>The initialized observable property helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObservableAsPropertyHelper<double> CreateMyReadOnlyNonNullPropertyHelper() =>
        ObservableAsPropertyTestNonNull().ToProperty(this, nameof(MyReadOnlyNonNullProperty), initialValue: NegativeNonNullReadOnlyValue);

    /// <summary>Registers the lifecycle activation subscriptions.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RegisterActivation() =>
        this.WhenActivated(disposables =>
        {
            Log("Activated");
            _test11PropertyHelper.Dispose();
            _test11PropertyHelper = CreateTest11PropertyHelper();
            disposables(_test11PropertyHelper);
            disposables(GetDataCommand.Do(static _ => Log("GetDataCommand Executed")).Subscribe());
            disposables(GetDataCommand.Execute().Subscribe());
        });

    /// <summary>Initializes observable property values before sample commands execute.</summary>
    private void InitializeObservableProperties()
    {
        Log("MyReadOnlyProperty before init");
        Log(MyReadOnlyProperty);
        Log("MyReadOnlyNonNullProperty before init");
        Log(MyReadOnlyNonNullProperty);
        Log(ObservableAsPropertyTest2Property);
        Log(ReferenceTypeObservableProperty);
        Log(NullableReferenceTypeObservableProperty);
    }

    /// <summary>Exercises the initial generated command set.</summary>
    private void ExerciseInitialCommands()
    {
        Log(Test1Command);
        Log(Test2Command);
        Log(Test3Command);
        Log(Test4Command);
        Log(Test5StringToIntCommand);
        Log(Test6ArgOnlyCommand);
        Log(Test7ObservableCommand);
        Log(Test8ObservableCommand);
        Log(Test9Command);
        Log(Test10Command);
        Test1Command?.Execute().Subscribe();
        Test2Command?.Execute().Subscribe(static r => Log(r));
        Test3Command?.Execute().Subscribe();
        Test4Command?.Execute().Subscribe(static r => Log(r));
        Test5StringToIntCommand?.Execute("100").Subscribe(static value => Log(value));
        Test6ArgOnlyCommand?.Execute("Hello World").Subscribe();
        Test7ObservableCommand?.Execute().Subscribe();
        Log($"Test2Property default Value: {Test2Property}");
        Test8ObservableCommand?.Execute(CommandArgumentValue).Subscribe(static d => Log(d));
        Log($"Test2Property Value: {Test2Property}");
        Log(ObservableAsPropertyTest2Property);
    }

    /// <summary>Exercises the nullable and non-null observable property samples.</summary>
    private void ExerciseReadOnlyProperties()
    {
        Log("MyReadOnlyProperty After Init");
        _testSubject.OnNext(NegativeUpdatedReadOnlyValue);
        Log(MyReadOnlyProperty);
        _testSubject.OnNext(FirstObservedDoubleValue);
        Log(MyReadOnlyProperty);
        _testSubject.OnNext(null);
        Log(MyReadOnlyProperty);
        Log("MyReadOnlyNonNullProperty After Init");
        _testNonNullSubject.OnNext(NegativeUpdatedReadOnlyValue);
        Log(MyReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(SecondObservedDoubleValue);
        Log(MyReadOnlyNonNullProperty);
        _testNonNullSubject.OnNext(default);
        Log(Test13Property);
        Log(MyReadOnlyNonNullProperty);
    }

    /// <summary>Exercises the remaining commands and observable property updates.</summary>
    private void ExerciseRemainingCommands()
    {
        Test9Command?.ThrownExceptions.Subscribe(static value => Log(value));
        var cancel = Test9Command?.Execute().Subscribe();
        cancel?.Dispose();
        Test10Command?.Execute(CommandResultValue).Subscribe(static r => Log(r));
        TestPrivateCanExecuteCommand?.Execute().Subscribe();
        Log($"Observable unset, value should be 10, value is : {ObservableAsPropertyFromProperty}");
        _fromPartialTestSubject.OnNext(ObservableUpdatedValue);
        Log($"Observable updated, value should be 11, value is : {ObservableAsPropertyFromProperty}");
        _ = Console.ReadLine();
    }

    /// <summary>Registers the subscription that maintains the visible people collection.</summary>
    private void RegisterPeopleSubscription() =>
        _ = this.WhenAnyValue(static vm => vm.People)
            .Subscribe(people => people
                .AsObservableChangeSet()
                .AutoRefresh(static x => x.Deleted)
                .Filter(static x => !x.Deleted)
                .Bind(out _visiblePeople)
                .Subscribe());
}
