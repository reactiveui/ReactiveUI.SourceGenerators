// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="PropertyObservable{T}"/>.</summary>
public class PropertyObservableTests
{
    /// <summary>How long to wait for a property notification to arrive.</summary>
    private const int NotificationDelayMilliseconds = 10;

    /// <summary>The initial name value assigned to the view model under test.</summary>
    private const string InitialName = "Alice";

    /// <summary>The expected number of emissions when the initial value plus one change are observed.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The expected number of emissions after a third notification.</summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>A sample age value assigned to an unrelated property.</summary>
    private const int SampleAge = 26;

    /// <summary>The number of rapid property-changed iterations in the concurrency test.</summary>
    private const int ConcurrencyIterations = 100;

    /// <summary>A subscriber can wait for a competing property write without blocking that writer.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OnNext_WaitsForCompetingWriter_DeliversLatestValueWithoutOverlap()
    {
        const int joinTimeoutSeconds = 5;
        var source = new TestViewModel { Name = InitialName };
        var writerFinished = false;
        Thread? writer = null;
        var recorder = new EmissionRecorder<string?>();
        var observable = new PropertyObservable<string?>(source, nameof(source.Name), static x => ((TestViewModel)x).Name, true);
        recorder.OnFirstValue = () =>
        {
            writer = new(() => source.Name = "Bob") { IsBackground = true };
            writer.Start();
            writerFinished = writer.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
        };
        using var subscription = observable.Subscribe(recorder);

        var joined = writer!.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
        var values = recorder.Snapshot();
        await Assert.That(joined).IsTrue();
        await Assert.That(writerFinished).IsTrue();
        await Assert.That(recorder.MaxConcurrentEmissions).IsEqualTo(1);
        await Assert.That(values).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo(InitialName);
        await Assert.That(values[1]).IsEqualTo("Bob");
    }

    /// <summary>Verifies that Subscribe throws ArgumentNullException when observer is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel { Name = InitialName };
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        var action = () => observable.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when source is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = static () => new PropertyObservable<string>(null!, "Name", static x => "test", false);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when propertyName is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullPropertyName_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyObservable<string>(vm, null!, static x => "test", false);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when getter is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyObservable<string>(vm, "Name", null!, false);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Subscribe emits the current value immediately.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsCurrentValueImmediately()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that a getter throwing on subscribe propagates and leaves no handler attached.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_GetterThrows_DetachesHandler()
    {
        var vm = new ThrowingGetterViewModel();
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ThrowingGetterViewModel)x).Name, false);

        var action = () => observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        await Assert.That(action).ThrowsExactly<InvalidOperationException>();

        vm.Fail = false;
        vm.RaisePropertyChanged("Name");

        await Assert.That(vm.HandlerCount).IsEqualTo(0);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>Verifies that PropertyChanged event triggers a new value emission.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_TriggersEmission()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Name = "Bob";

        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(results[1]).IsEqualTo("Bob");
    }

    /// <summary>Verifies that only matching property name triggers emission.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_WrongProperty_DoesNotTriggerEmission()
    {
        var vm = new TestViewModel { Name = InitialName, Age = SampleAge };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Age = SampleAge;

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that null/empty property name matches all properties.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_NullOrEmpty_TriggersEmission()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name, false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        vm.RaisePropertyChanged(string.Empty);
        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);

        vm.RaisePropertyChanged(null);
        await Assert.That(results).Count().IsEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>Verifies that distinctUntilChanged=true suppresses duplicate values.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DistinctUntilChanged_True_SuppressesDuplicates()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name, true);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Name = InitialName; // Same value
        vm.RaisePropertyChanged("Name");

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>Verifies that distinctUntilChanged=false allows duplicate values.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DistinctUntilChanged_False_AllowsDuplicates()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name, false);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Name = InitialName; // Same value
        vm.RaisePropertyChanged("Name");

        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
    }

    /// <summary>Verifies that disposal removes the event handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_RemovesEventHandler()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        subscription.Dispose();

        vm.Name = "Bob";
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that double dispose does not throw.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_NoException()
    {
        var vm = new TestViewModel { Name = InitialName };
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name, false);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(static _ => { }, static _ => { }, static () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>Verifies that a PropertyChanged event fired after dispose does not throw (observer is null path).</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChangedAfterDispose_DoesNotThrow()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name, false);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        subscription.Dispose();

        // Fire event after dispose - should be safely ignored
        vm.RaisePropertyChanged("Name");

        // Only the initial value should have been emitted
        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>A notification captured before disposal cannot deliver after disposal.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnPropertyChanged_HandlerCapturedBeforeDisposal_DoesNotDeliver()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name, false);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        var action = () => vm.RaisePropertyChanged("Name", subscription.Dispose);
        await Assert.That(action).ThrowsNothing();
        await Assert.That(results).Count().IsEqualTo(1);
        subscription.Dispose();
    }

    /// <summary>
    /// Verifies that the observer-is-null branch in OnPropertyChanged is safely handled when
    /// PropertyChanged fires concurrently with disposal on another thread.
    /// Covers PropertyObservable line 96 (observer is null race-condition guard).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_ConcurrentDispose_ObserverNullGuardDoesNotThrow()
    {
        var vm = new ConcurrentTestViewModel();
        var observable = new PropertyObservable<string>(vm, "Name", static x => ((ConcurrentTestViewModel)x).Name, false);
        var results = new List<string>();

        var subscription = observable.Subscribe(new AnonymousObserver<string>(
            results.Add,
            static _ => { },
            static () => { }));

        // Start a background task that rapidly disposes while property changes occur
        var disposed = false;
        var disposeTask = Task.Run(async () =>
        {
            await Task.Delay(NotificationDelayMilliseconds);
            subscription.Dispose();
            disposed = true;
        });

        // Rapidly raise property changed events
        for (var i = 0; i < ConcurrencyIterations && !disposed; i++)
        {
            vm.Name = $"Value{i}";
            vm.RaisePropertyChanged("Name");
        }

        await disposeTask;

        // The test succeeds if no exception was thrown during concurrent dispose + property change
        await Assert.That(results).Count().IsGreaterThanOrEqualTo(1);
    }

    /// <summary>A test view model that implements <see cref="INotifyPropertyChanged"/> for concurrent disposal tests.</summary>
    private sealed class ConcurrentTestViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = InitialName;

        /// <summary>Raises the <see cref="PropertyChanged"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(string? propertyName) =>
            PropertyChanged?.Invoke(this, new(propertyName));
    }

    /// <summary>A test view model that implements <see cref="INotifyPropertyChanged"/> with manual event raising.</summary>
    private sealed class ManualTestViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Raises the <see cref="PropertyChanged"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="beforeRaise">An action to run after capturing the event handlers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(string? propertyName, Action? beforeRaise = null)
        {
            var handlers = PropertyChanged;
            beforeRaise?.Invoke();
            handlers?.Invoke(this, new(propertyName));
        }
    }

    /// <summary>A view model whose getter throws on demand and which counts its attached handlers.</summary>
    private sealed class ThrowingGetterViewModel : INotifyPropertyChanged
    {
        /// <summary>The attached handlers.</summary>
        private PropertyChangedEventHandler? _propertyChanged;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }

        /// <summary>Gets or sets a value indicating whether reading <see cref="Name"/> throws.</summary>
        public bool Fail { get; set; } = true;

        /// <summary>Gets the name, throwing while <see cref="Fail"/> is set.</summary>
        public string Name => Fail ? throw new InvalidOperationException("getter") : InitialName;

        /// <summary>Gets the number of attached handlers.</summary>
        public int HandlerCount => _propertyChanged?.GetInvocationList().Length ?? 0;

        /// <summary>Raises the <see cref="PropertyChanged"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanged(string? propertyName) =>
            _propertyChanged?.Invoke(this, new(propertyName));
    }

    /// <summary>A simple observer that delegates to provided actions.</summary>
    /// <typeparam name="T">The type of elements observed.</typeparam>
    /// <param name="onNext">The action to invoke for each element.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onCompleted">The action to invoke on completion.</param>
    private sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted) : IObserver<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => onCompleted();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => onError(error);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value) => onNext(value);
    }
}
