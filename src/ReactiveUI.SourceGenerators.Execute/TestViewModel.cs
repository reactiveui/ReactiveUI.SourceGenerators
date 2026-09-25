// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides a comprehensive source-generator execution sample.</summary>
/// <seealso cref="ReactiveUI.Reactive.ReactiveObject" />
/// <seealso cref="ReactiveUI.Reactive.IActivatableViewModel" />
/// <seealso cref="System.IDisposable" />
/// <seealso cref="IDisposable" />
[DataContract]
public partial class TestViewModel : ReactiveObject, IActivatableViewModel, IDisposable
{
    /// <summary>Represents the initial nullable read-only value.</summary>
    private const double NegativeInitialReadOnlyValue = -1.0D;

    /// <summary>Represents the initial non-null read-only value.</summary>
    private const double NegativeNonNullReadOnlyValue = -5.0D;

    /// <summary>Represents the updated read-only value.</summary>
    private const double NegativeUpdatedReadOnlyValue = -2.0D;

    /// <summary>Represents the first observed double value.</summary>
    private const double FirstObservedDoubleValue = 10.0D;

    /// <summary>Represents the second observed double value.</summary>
    private const double SecondObservedDoubleValue = 11.0D;

    /// <summary>Represents the initial observable-as-property value.</summary>
    private const int ObservableAsPropertyInitialValue = 11_223_344;

    /// <summary>Represents the argument passed to the sample command.</summary>
    private const int CommandArgumentValue = 100;

    /// <summary>Represents the expected sample command result.</summary>
    private const int CommandResultValue = 200;

    /// <summary>Represents the value published after the observable updates.</summary>
    private const int ObservableUpdatedValue = 11;

    /// <summary>Represents the default observable value.</summary>
    private const int DefaultObservableValue = 9;

    /// <summary>Represents the offset applied by the observable command.</summary>
    private const double ObservableCommandOffset = 10.0D;

    /// <summary>Represents the cancellation delay in milliseconds.</summary>
    private const int CancellationDelayMilliseconds = 2_000;

    /// <summary>Represents the initial value of <see cref="Test2Property"/>.</summary>
    private const double Test2InitialValue = 1.1D;

    /// <summary>Represents the initial value of <see cref="Test11Property"/> and <see cref="Test13Property"/>.</summary>
    private const double Test11InitialValue = 11.1D;

    /// <summary>Represents the initial value of <see cref="PLCPort"/>.</summary>
    private const int PlcInitialPort = 9000;

    /// <summary>Represents the initial value of <see cref="ObservableAsPropertyFromProperty"/>.</summary>
    private const int FromPropertyInitialValue = 10;

    /// <summary>Provides the observable used to control the private command.</summary>
    private readonly IObservable<bool> _observable = Observable.Return(true);

    /// <summary>Publishes nullable values for the observable-as-property example.</summary>
    private readonly Subject<double?> _testSubject = new();

    /// <summary>Publishes non-null values for the observable-as-property example.</summary>
    private readonly Subject<double> _testNonNullSubject = new();

    /// <summary>Publishes values for the partial observable-as-property example.</summary>
    private readonly Subject<int> _fromPartialTestSubject = new();

    /// <summary>Provides the scheduler used by generated reactive commands.</summary>
    private readonly IScheduler _scheduler = RxSchedulers.MainThreadScheduler;

    // [ObservableAsProperty] moved to ReactiveUI.Binding. The read-only properties below are backed by
    // ObservableAsPropertyHelper fields written by hand with ReactiveUI's ToProperty. On ReactiveUI.Binding, mark a
    // partial property [ObservableAsProperty] and assign the generated _nameHelper field with ToProperty instead.
    /// <summary>Backs <see cref="Test2Property"/>.</summary>
    private readonly ObservableAsPropertyHelper<double?> _test2PropertyHelper;

    /// <summary>Backs <see cref="ObservableAsPropertyTest3Property"/>.</summary>
    private readonly ObservableAsPropertyHelper<double> _observableAsPropertyTest3PropertyHelper;

    /// <summary>Backs <see cref="ObservableAsPropertyTest2Property"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _observableAsPropertyTest2PropertyHelper;

    /// <summary>Backs <see cref="ObservableAsPropertyFromProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _observableAsPropertyFromPropertyHelper;

    /// <summary>Backs <see cref="PLCActive"/>.</summary>
    private readonly ObservableAsPropertyHelper<string?> _plcActiveHelper;

    /// <summary>Backs <see cref="PLCStatus"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _plcStatusHelper;

    /// <summary>Backs <see cref="PLCPort"/>.</summary>
    private readonly ObservableAsPropertyHelper<int> _plcPortHelper;

    /// <summary>Backs <see cref="InstanceOfPLC"/>.</summary>
    private readonly ObservableAsPropertyHelper<PLCInstance> _plcInstanceHelper;

    /// <summary>Backs <see cref="ReferenceTypeObservableProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<object> _referenceTypeObservablePropertyHelper;

    /// <summary>Backs <see cref="NullableReferenceTypeObservableProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<object?> _nullableReferenceTypeObservablePropertyHelper;

    /// <summary>Backs <see cref="MyReadOnlyProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<double?> _myReadOnlyPropertyHelper;

    /// <summary>Backs <see cref="MyReadOnlyNonNullProperty"/>.</summary>
    private readonly ObservableAsPropertyHelper<double> _myReadOnlyNonNullPropertyHelper;

    /// <summary>Backs <see cref="Test11Property"/>; replaced each time the view model activates.</summary>
    private ObservableAsPropertyHelper<double?> _test11PropertyHelper;

    /// <summary>Stores the value <see cref="ObservableAsPropertyTest3Property"/> follows.</summary>
    [Reactive]
    private double _test13Property = Test11InitialValue;

    /// <summary>Stores the value used by the reactive test property.</summary>
    [property: Test(AParameter = "Test Input")]
    [Reactive]
    private double? _test12Property = 12.1D;

    /// <summary>Stores the protected reactive test property value.</summary>
    [Reactive(SetModifier = AccessModifier.Protected)]
    [property: JsonInclude]
    [DataMember]
    private int _test1Property;

    /// <summary>Tracks whether the instance has disposed its resources.</summary>
    private bool _disposedValue;

    /// <summary>Stores the mutable string test value.</summary>
    [Reactive]
    private string _myStringProperty = "test";

    /// <summary>Stores the nullable reactive name.</summary>
    [property: JsonInclude]
    [DataMember]
    [Reactive(Inheritance = InheritanceModifier.Virtual, SetModifier = AccessModifier.Protected)]
    private string? _name;

    /// <summary>Stores the required reactive value.</summary>
    [Reactive(nameof(MyDoubleProperty), nameof(MyStringProperty), SetModifier = AccessModifier.Init, UseRequired = true)]
    private string _mustBeSet;

    /// <summary>Stores the people included in the derived list example.</summary>
    [Reactive]
    private IEnumerable<Person> _people = [new Person()];

    /// <summary>Stores the nullable double reactive value.</summary>
    [Reactive]
    private double? _myDoubleProperty;

    /// <summary>Stores the non-null double reactive value.</summary>
    [Reactive]
    private double _myDoubleNonNullProperty;

    /// <summary>Stores the PLC instance used by the reactive property example.</summary>
    [Reactive]
    private PLCInstance _plcInstanceCore = new();

    /// <summary>Stores the derived observable collection of visible people.</summary>
    [BindableDerivedList]
    private ReadOnlyObservableCollection<Person>? _visiblePeople;

    /// <summary>Initializes a new instance of the <see cref="TestViewModel"/> class.</summary>
    [SetsRequiredMembers]
    public TestViewModel()
    {
        _ = new InternalTestViewModel { PublicRequiredPartialPropertyTest = true };
        MustBeSet = "Test";
        _test11PropertyHelper = CreateTest11PropertyHelper();
        _test2PropertyHelper = CreateTest2PropertyHelper();
        _observableAsPropertyTest3PropertyHelper = CreateObservableAsPropertyTest3PropertyHelper();
        _observableAsPropertyFromPropertyHelper = CreateObservableAsPropertyFromPropertyHelper();
        _plcActiveHelper = CreatePlcActiveHelper();
        _plcStatusHelper = CreatePlcStatusHelper();
        _plcPortHelper = CreatePlcPortHelper();
        _plcInstanceHelper = CreatePlcInstanceHelper();
        ReferenceTypeObservable = Observable.Return(new object());
        NullableReferenceTypeObservable = Observable.Return<object?>(null);
        _referenceTypeObservablePropertyHelper = CreateReferenceTypeHelper();
        _nullableReferenceTypeObservablePropertyHelper = CreateNullableReferenceTypeHelper();
        _observableAsPropertyTest2PropertyHelper = CreateObservableAsPropertyTest2PropertyHelper();
        _myReadOnlyPropertyHelper = CreateMyReadOnlyPropertyHelper();
        _myReadOnlyNonNullPropertyHelper = CreateMyReadOnlyNonNullPropertyHelper();
        RegisterActivation();
        InitializeObservableProperties();
        ExerciseInitialCommands();
        ExerciseReadOnlyProperties();
        ExerciseRemainingCommands();
        RegisterPeopleSubscription();
    }

    /// <summary>Gets the instance.</summary>
    /// <value>
    /// The instance.
    /// </value>
    public static TestViewModel Instance { get; } = new();

    /// <summary>Gets the test class oaph vm.</summary>
    /// <value>
    /// The test class oaph vm.
    /// </value>
    public TestClassOAPH_VM TestClassOAPH_VM { get; } = new();

    /// <summary>
    /// Gets or sets the partial property test.
    /// </summary>
    /// <value>
    /// The partial property test.
    /// </value>
    [Reactive]
    [field: JsonInclude]
    [JsonPropertyName("test")]
    public partial string? PartialPropertyTest { get; set; }

    /// <summary>
    /// Gets or sets the partial property test.
    /// </summary>
    /// <value>
    /// The partial property test.
    /// </value>
    [Reactive(UseRequired = true)]
    public required partial string? PartialRequiredPropertyTest { get; set; }

    /// <summary>Gets the internal test property. Should not prompt to replace with INPC Reactive Property.</summary>
    /// <value>
    /// The test property.
    /// </value>
    [JsonInclude]
    public string? TestInternalSetProperty { get; internal set; } = "Test";

    /// <summary>Gets the test private set property. Should not prompt to replace with INPC Reactive Property.</summary>
    /// <value>
    /// The test private set property.
    /// </value>
    [JsonInclude]
    public string? TestPrivateSetProperty { get; private set; } = "Test";

    /// <summary>Gets or sets the test automatic property.</summary>
    /// <value>
    /// The test automatic property.
    /// </value>
    [JsonInclude]
    public string? TestAutoProperty { get; set; } = "Test, should prompt to replace with INPC Reactive Property";

    /// <summary>Gets the test read only property.</summary>
    /// <value>
    /// The test read only property.
    /// </value>
    public string? TestReadOnlyProperty { get; } = "Test, should not prompt to replace with INPC Reactive Property";

    /// <summary>Gets or sets the reactive command test property. Should not prompt to replace with INPC Reactive Property.</summary>
    /// <value>
    /// The reactive command test property.
    /// </value>
    public ReactiveCommand<Unit, Unit>? ReactiveCommandTestProperty { get; set; }

    /// <summary>Gets or sets the reactive property test property. Should not prompt to replace with INPC Reactive Property.</summary>
    /// <value>
    /// The reactive property test property.
    /// </value>
    public ReactiveProperty<int>? ReactivePropertyTestProperty { get; set; }

    /// <summary>Gets the can execute test1.</summary>
    /// <value>
    /// The can execute test1.
    /// </value>
    public IObservable<bool> CanExecuteTest1 => ObservableAsPropertyTest2.Select(static x => x > 0);

    /// <summary>Gets the observable as property test2.</summary>
    /// <value>
    /// The observable as property test2.
    /// </value>
    public IObservable<int> ObservableAsPropertyTest2 => Observable.Return(DefaultObservableValue);

    /// <summary>Gets the latest value <see cref="ObservableAsPropertyTest2"/> produced.</summary>
    [Test(AParameter = "Test Input")]
    public int ObservableAsPropertyTest2Property => _observableAsPropertyTest2PropertyHelper.Value;

    /// <summary>Gets the latest value <see cref="Test8ObservableCommand"/> produced.</summary>
    [JsonInclude]
    public double? Test2Property => _test2PropertyHelper.Value;

    /// <summary>Gets the latest <see cref="Test12Property"/>, following it while the view model is active.</summary>
    public double? Test11Property => _test11PropertyHelper.Value;

    /// <summary>Gets the latest <see cref="Test13Property"/>.</summary>
    public double ObservableAsPropertyTest3Property => _observableAsPropertyTest3PropertyHelper.Value;

    /// <summary>Gets the latest value <see cref="ObservableAsPropertyTest"/> produced.</summary>
    public double? MyReadOnlyProperty => _myReadOnlyPropertyHelper.Value;

    /// <summary>Gets the latest value <see cref="ObservableAsPropertyTestNonNull"/> produced.</summary>
    public double MyReadOnlyNonNullProperty => _myReadOnlyNonNullPropertyHelper.Value;

    /// <summary>Gets the current active PLC identifier or name.</summary>
    public string? PLCActive => _plcActiveHelper.Value;

    /// <summary>Gets the current PLC status message, initialized with an empty string.</summary>
    public string PLCStatus => _plcStatusHelper.Value;

    /// <summary>Gets the TCP port number used to communicate with the PLC.</summary>
    public int PLCPort => _plcPortHelper.Value;

    /// <summary>Gets the current instance of the PLC (Programmable Logic Controller) used by the application.</summary>
    public PLCInstance InstanceOfPLC => _plcInstanceHelper.Value;

    /// <summary>Gets the Activator which will be used by the View when Activation/Deactivation occurs.</summary>
    public ViewModelActivator Activator { get; } = new();

    /// <summary>Gets the observable as property from property.</summary>
    /// <value>
    /// The observable as property from property.
    /// </value>
    public int ObservableAsPropertyFromProperty => _observableAsPropertyFromPropertyHelper.Value;

    /// <summary>
    /// Gets or sets the value for internal use within the partial class or assembly.
    /// </summary>
    [Reactive]
    internal partial int InternalPartialPropertyTest { get; set; }

    /// <summary>Gets the latest value <see cref="ReferenceTypeObservable"/> produced.</summary>
    private object ReferenceTypeObservableProperty => _referenceTypeObservablePropertyHelper.Value;

    /// <summary>Gets the latest value <see cref="NullableReferenceTypeObservable"/> produced.</summary>
    private object? NullableReferenceTypeObservableProperty => _nullableReferenceTypeObservablePropertyHelper.Value;

    /// <summary>Gets the observable used for the non-null reference type example.</summary>
    private IObservable<object> ReferenceTypeObservable { get; }

    /// <summary>Gets the observable used for the nullable reference type example.</summary>
    private IObservable<object?> NullableReferenceTypeObservable { get; }

    /// <summary>Gets observables as property test.</summary>
    /// <returns>
    /// Observable of double.
    /// </returns>
    public IObservable<double?> ObservableAsPropertyTest() => _testSubject;

    /// <summary>Observables as property test non null.</summary>
    /// <returns>Observable of double.</returns>
    public IObservable<double> ObservableAsPropertyTestNonNull() => _testNonNullSubject;

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _testSubject.Dispose();
            _testNonNullSubject.Dispose();
            _fromPartialTestSubject.Dispose();
            _test2PropertyHelper.Dispose();
            _test11PropertyHelper.Dispose();
            _observableAsPropertyTest2PropertyHelper.Dispose();
            _observableAsPropertyTest3PropertyHelper.Dispose();
            _observableAsPropertyFromPropertyHelper.Dispose();
            _plcActiveHelper.Dispose();
            _plcStatusHelper.Dispose();
            _plcPortHelper.Dispose();
            _plcInstanceHelper.Dispose();
            _referenceTypeObservablePropertyHelper.Dispose();
            _nullableReferenceTypeObservablePropertyHelper.Dispose();
            _myReadOnlyPropertyHelper.Dispose();
            _myReadOnlyNonNullPropertyHelper.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>Associates a sample result with this view-model instance.</summary>
    /// <typeparam name="T">The type of the sample result.</typeparam>
    /// <param name="value">The result to return.</param>
    /// <returns>The supplied result.</returns>
    private T UseInstance<T>(T value)
    {
        GC.KeepAlive(this);
        return value;
    }

    /// <summary>Writes a sample command message while retaining the owning view model.</summary>
    /// <param name="message">The message to write.</param>
    private void WriteSampleMessage(string message)
    {
        GC.KeepAlive(this);
        Console.Out.WriteLine(message);
    }

    /// <summary>Test1s this instance.</summary>
    [ReactiveCommand(CanExecute = nameof(CanExecuteTest1))]
    [property: JsonInclude]
    [property: Test(AParameter = "Test Input")]
    private void Test1() => WriteSampleMessage("Test1 Command Executed");

    /// <summary>Test3s the asynchronous.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ReactiveCommand]
    private Task Test3Async() => UseInstance(Task.Delay(0));

    /// <summary>Test4s the asynchronous.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ReactiveCommand]
    private Task<Point> Test4Async() => UseInstance(Task.FromResult(new Point(CommandArgumentValue, CommandArgumentValue)));

    /// <summary>Test5s the string to int.</summary>
    /// <param name="str">The string.</param>
    /// <returns>int.</returns>
    [ReactiveCommand]
    private int Test5StringToInt(string str) => UseInstance(int.Parse(str));

    /// <summary>Test6s the argument only.</summary>
    /// <param name="str">The string.</param>
    [ReactiveCommand]
    private void Test6ArgOnly(string str) => WriteSampleMessage($">>> {str}");

    /// <summary>Test7s the observable.</summary>
    /// <returns>An Observable of Unit.</returns>
    [ReactiveCommand]
    private IObservable<Unit> Test7Observable() => UseInstance(Observable.Return(Unit.Default));

    /// <summary>Test8s the observable.</summary>
    /// <param name="i">The i.</param>
    /// <returns>An Observable of int.</returns>
    [ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
    private IObservable<double?> Test8Observable(int i) => UseInstance(Observable.Return<double?>(i + ObservableCommandOffset));

    /// <summary>Executes the cancellable reactive command sample.</summary>
    /// <param name="ct">The cancellation token for the command.</param>
    /// <returns>A task that completes when the command finishes.</returns>
    [ReactiveCommand]
    private Task Test9Async(CancellationToken ct) => UseInstance(Task.Delay(CancellationDelayMilliseconds, ct));

    /// <summary>Executes the parameterized cancellable reactive command sample.</summary>
    /// <param name="size">The size of the generated point.</param>
    /// <param name="ct">The cancellation token for the command.</param>
    /// <returns>A task that produces the generated point.</returns>
    [ReactiveCommand]
    private Task<Point> Test10Async(int size, CancellationToken ct) => UseInstance(Task.FromResult(new Point(size, size)));

    /// <summary>Executes the command with a private observable can-execute source.</summary>
    [ReactiveCommand(CanExecute = nameof(_observable), OutputScheduler = nameof(_scheduler))]
    private void TestPrivateCanExecute() => WriteSampleMessage("TestPrivateCanExecute");

    /// <summary>Retrieves the empty data sequence used by the reactive command sample.</summary>
    /// <param name="ct">The cancellation token for the command.</param>
    /// <returns>A task that produces an empty data sequence.</returns>
    [ReactiveCommand]
    private Task<System.Collections.IEnumerable> GetData(CancellationToken ct) =>
        UseInstance(Task.FromResult<System.Collections.IEnumerable>(Array.Empty<System.Collections.IEnumerable>()));
}
