// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="PropertyChangingObservable{T}"/>.</summary>
public class PropertyChangingObservableTests
{
    /// <summary>The initial name value assigned to the view model under test.</summary>
    private const string InitialName = "Alice";

    /// <summary>The expected number of emissions when the initial value plus one change are observed.</summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>The expected number of emissions after a third notification.</summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>A sample age value assigned to an unrelated property.</summary>
    private const int SampleAge = 26;

    /// <summary>The number of rapid property-changing iterations in the concurrency test.</summary>
    private const int ConcurrencyIterations = 100;

    /// <summary>A subscriber can wait for another writer while preserving its captured before-change value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OnNext_WaitsForCompetingWriter_DeliversCapturedValueWithoutOverlap()
    {
        const int joinTimeoutSeconds = 5;
        var source = new TestViewModel { Name = InitialName };
        var writerFinished = false;
        Thread? writer = null;
        var recorder = new EmissionRecorder<string?>
        {
            OnFirstValue = () =>
            {
                writer = new(() => source.Name = "Bob") { IsBackground = true };
                writer.Start();
                writerFinished = writer.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
            },
        };
        var observable = new PropertyChangingObservable<string?>(source, nameof(source.Name), static x => ((TestViewModel)x).Name);
        using var subscription = observable.Subscribe(recorder);

        var joined = writer!.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
        var values = recorder.Snapshot();
        await Assert.That(joined).IsTrue();
        await Assert.That(writerFinished).IsTrue();
        await Assert.That(recorder.MaxConcurrentEmissions).IsEqualTo(1);
        await Assert.That(values).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[0]).IsEqualTo(InitialName);
        await Assert.That(values[1]).IsEqualTo(InitialName);
        await Assert.That(source.Name).IsEqualTo("Bob");
    }

    /// <summary>A nested before-change notification reaches the observer before the nested setter writes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OnNext_ReentrantWrite_DeliversBeforeNestedWrite()
    {
        var source = new TestViewModel { Name = InitialName };
        var values = new List<string?>();
        var valueSeenDuringNestedNotification = string.Empty;
        var observable = new PropertyChangingObservable<string?>(source, nameof(source.Name), static x => ((TestViewModel)x).Name);
        using var subscription = observable.Subscribe(value =>
        {
            values.Add(value);
            if (values.Count == 1)
            {
                source.Name = "Bob";
            }
            else
            {
                valueSeenDuringNestedNotification = source.Name;
            }
        });

        await Assert.That(values).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(values[1]).IsEqualTo(InitialName);
        await Assert.That(valueSeenDuringNestedNotification).IsEqualTo(InitialName);
        await Assert.That(source.Name).IsEqualTo("Bob");
    }

    /// <summary>Verifies that Subscribe throws ArgumentNullException when observer is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel { Name = InitialName };
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

        var action = () => observable.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when source is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = static () => new PropertyChangingObservable<string>(null!, "Name", static x => "test");

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when propertyName is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullPropertyName_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyChangingObservable<string>(vm, null!, static x => "test");

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that constructor throws ArgumentNullException when getter is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyChangingObservable<string>(vm, "Name", null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Subscribe emits the current value immediately.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsCurrentValueImmediately()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

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
        var observable = new PropertyChangingObservable<string>(vm, "Name", static x => ((ThrowingGetterViewModel)x).Name);

        var action = () => observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        await Assert.That(action).ThrowsExactly<InvalidOperationException>();

        vm.Fail = false;
        vm.RaisePropertyChanging("Name");

        await Assert.That(vm.HandlerCount).IsEqualTo(0);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>A getter failure during event attachment releases the handler before propagating.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Subscribe_GetterThrowsDuringAttachment_DetachesHandler()
    {
        var source = new ThrowingGetterViewModel { NotifyDuringAttachment = true };
        var observable = new PropertyChangingObservable<string>(source, nameof(source.Name), static value => ((ThrowingGetterViewModel)value).Name);
        var observer = new EmissionRecorder<string>();

        await Assert.That(() => observable.Subscribe(observer)).ThrowsExactly<InvalidOperationException>();
        await Assert.That(source.HandlerCount).IsEqualTo(0);
        await Assert.That(observer.Snapshot()).IsEmpty();
    }

    /// <summary>Verifies that PropertyChanging event triggers a value emission (old value).</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_TriggersEmission()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Name = "Bob";

        // Should have 2 values: initial "Alice", and "Alice" again when PropertyChanging fired before it became "Bob"
        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(results[0]).IsEqualTo(InitialName);
        await Assert.That(results[1]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that only matching property name triggers emission.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_WrongProperty_DoesNotTriggerEmission()
    {
        var vm = new TestViewModel { Name = InitialName, Age = SampleAge };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        vm.Age = SampleAge;

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>Verifies that null/empty property name matches all properties.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_NullOrEmpty_TriggersEmission()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name);

        _ = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        vm.RaisePropertyChanging(string.Empty);
        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);

        vm.RaisePropertyChanging(null);
        await Assert.That(results).Count().IsEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>Verifies that disposal removes the event handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_RemovesEventHandler()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

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
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), static x => ((TestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(static _ => { }, static _ => { }, static () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>Verifies that a PropertyChanging event fired after dispose does not throw (observer is null path).</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChangingAfterDispose_DoesNotThrow()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));
        subscription.Dispose();

        // Fire event after dispose - should be safely ignored
        vm.RaisePropertyChanging("Name");

        // Only the initial value should have been emitted
        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>A notification captured before disposal cannot deliver after disposal.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnPropertyChanging_HandlerCapturedBeforeDisposal_DoesNotDeliver()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", static x => ((ManualTestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, static _ => { }, static () => { }));

        var action = () => vm.RaisePropertyChanging("Name", subscription.Dispose);
        await Assert.That(action).ThrowsNothing();
        await Assert.That(results).Count().IsEqualTo(1);
        subscription.Dispose();
    }

    /// <summary>
    /// Verifies that the observer-is-null branch in OnPropertyChanging is safely handled when
    /// PropertyChanging fires concurrently with disposal on another thread.
    /// Covers PropertyChangingObservable line 86 (observer is null race-condition guard).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_ConcurrentDispose_ObserverNullGuardDoesNotThrow()
    {
        var vm = new ConcurrentChangingTestViewModel();
        var observable =
            new PropertyChangingObservable<string>(vm, "Name", static x => ((ConcurrentChangingTestViewModel)x).Name);
        var results = new List<string>();

        var subscription = observable.Subscribe(new AnonymousObserver<string>(
            results.Add,
            static _ => { },
            static () => { }));

        // Start a background task that rapidly disposes while property changes occur
        var disposed = false;
        var disposeTask = Task.Run(async () =>
        {
            await Task.Delay(1);
            subscription.Dispose();
            disposed = true;
        });

        // Rapidly raise property changing events
        for (var i = 0; i < ConcurrencyIterations && !disposed; i++)
        {
            vm.Name = $"Value{i}";
            vm.RaisePropertyChanging("Name");
        }

        await disposeTask;

        // The test succeeds if no exception was thrown during concurrent dispose + property change
        await Assert.That(results).Count().IsGreaterThanOrEqualTo(1);
    }

    /// <summary>A test view model that implements <see cref="INotifyPropertyChanging"/> for concurrent disposal tests.</summary>
    private sealed class ConcurrentChangingTestViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = InitialName;

        /// <summary>Raises the <see cref="PropertyChanging"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanging(string? propertyName) =>
            PropertyChanging?.Invoke(this, new(propertyName));
    }

    /// <summary>A test view model that implements <see cref="INotifyPropertyChanging"/> with manual event raising.</summary>
    private sealed class ManualTestViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Raises the <see cref="PropertyChanging"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        /// <param name="beforeRaise">An action to run after capturing the event handlers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanging(string? propertyName, Action? beforeRaise = null)
        {
            var handlers = PropertyChanging;
            beforeRaise?.Invoke();
            handlers?.Invoke(this, new(propertyName));
        }
    }

    /// <summary>A view model whose getter throws on demand and which counts its attached handlers.</summary>
    private sealed class ThrowingGetterViewModel : INotifyPropertyChanging
    {
        /// <summary>The attached handlers.</summary>
        private PropertyChangingEventHandler? _propertyChanging;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging
        {
            add
            {
                _propertyChanging += value;
                if (NotifyDuringAttachment)
                {
                    value?.Invoke(this, new(nameof(Name)));
                }
            }

            remove => _propertyChanging -= value;
        }

        /// <summary>Gets or sets a value indicating whether adding a handler raises a notification.</summary>
        public bool NotifyDuringAttachment { get; set; }

        /// <summary>Gets or sets a value indicating whether reading <see cref="Name"/> throws.</summary>
        public bool Fail { get; set; } = true;

        /// <summary>Gets the name, throwing while <see cref="Fail"/> is set.</summary>
        public string Name => Fail ? throw new InvalidOperationException("getter") : InitialName;

        /// <summary>Gets the number of attached handlers.</summary>
        public int HandlerCount => _propertyChanging?.GetInvocationList().Length ?? 0;

        /// <summary>Raises the <see cref="PropertyChanging"/> event for the specified property.</summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePropertyChanging(string? propertyName) =>
            _propertyChanging?.Invoke(this, new(propertyName));
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
