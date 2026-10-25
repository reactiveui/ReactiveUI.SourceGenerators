// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Tests for <see cref="PluginPropertyObservable{T}"/>, the observation a registration drives.</summary>
public class PluginPropertyObservableTests
{
    /// <summary>The property these tests observe.</summary>
    private const string ObservedPropertyName = "Name";

    /// <summary>The value the fixture starts out holding.</summary>
    private const string InitialName = "initial";

    /// <summary>The value the fixture is moved to.</summary>
    private const string UpdatedName = "updated";

    /// <summary>The completion marker recorded after all delivered values.</summary>
    private const string CompletionMarker = "complete";

    /// <summary>The error marker recorded after all delivered values.</summary>
    private const string ErrorMarker = "error";

    /// <summary>The single emission an observation makes when nothing has moved.</summary>
    private static readonly string[] InitialOnly = [InitialName];

    /// <summary>The pair an observation makes when the property is moved once.</summary>
    private static readonly string[] InitialThenUpdated = [InitialName, UpdatedName];

    /// <summary>A subscriber can wait for a provider notification from another thread without deadlocking.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OnNext_WaitsForCompetingWriter_DeliversLatestValueWithoutOverlap()
    {
        const int joinTimeoutSeconds = 5;
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var writerFinished = false;
        Thread? writer = null;
        var recorder = new EmissionRecorder<string>
        {
            OnFirstValue = () =>
            {
                writer = new(() =>
                {
                    fixture.Name = UpdatedName;
                    notifications.Raise(fixture);
                }) { IsBackground = true };
                writer.Start();
                writerFinished = writer.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
            },
        };
        using var subscription = Build(fixture, notifications).Subscribe(recorder);

        var joined = writer!.Join(TimeSpan.FromSeconds(joinTimeoutSeconds));
        await Assert.That(joined).IsTrue();
        await Assert.That(writerFinished).IsTrue();
        await Assert.That(recorder.MaxConcurrentEmissions).IsEqualTo(1);
        await Assert.That(recorder.Snapshot()).IsEquivalentTo(InitialThenUpdated);
    }

    /// <summary>A terminal raised during attachment follows the initial value, and only the first terminal is delivered.</summary>
    /// <param name="fault">Whether the provider's first terminal is an error.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Subscribe_TerminalDuringAttachment_DeliversInitialValueBeforeTerminal(bool fault)
    {
        const int expectedNotifications = 2;
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var sequence = new List<string>();
        var error = new InvalidOperationException("provider error");
        notifications.AfterSubscribe = () =>
        {
            if (fault)
            {
                notifications.Fault(error);
            }

            notifications.Complete();
            notifications.Fault(error);
        };

        using var subscription = Build(fixture, notifications).Subscribe(
            sequence.Add,
            _ => sequence.Add(ErrorMarker),
            () => sequence.Add(CompletionMarker));
        fixture.Name = UpdatedName;
        notifications.Raise(fixture);

        await Assert.That(sequence).Count().IsEqualTo(expectedNotifications);
        await Assert.That(sequence[0]).IsEqualTo(InitialName);
        await Assert.That(sequence[1]).IsEqualTo(fault ? ErrorMarker : CompletionMarker);
    }

    /// <summary>A provider terminal waits for the current observer and the pending property read.</summary>
    /// <param name="fault">Whether the terminal is an error.</param>
    /// <param name="sameValue">Whether the pending notification leaves the value unchanged.</param>
    /// <param name="distinct">Whether equal consecutive values are suppressed.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [MatrixDataSource]
    public async Task OnNext_ReentrantChangeAndTerminal_DeliversPendingValueBeforeTerminal(
        [Matrix(false, true)] bool fault,
        [Matrix(false, true)] bool sameValue,
        [Matrix(false, true)] bool distinct)
    {
        const int valueAndTerminalCount = 2;
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var sequence = new List<string>();
        var insideObserver = false;
        var completionOverlapped = false;
        using var subscription = Build(fixture, notifications, distinct).Subscribe(
            value =>
            {
                insideObserver = true;
                sequence.Add(value);
                if (sequence.Count == 1)
                {
                    fixture.Name = sameValue ? InitialName : UpdatedName;
                    notifications.Raise(fixture);
                    if (fault)
                    {
                        notifications.Fault(new InvalidOperationException(ErrorMarker));
                    }
                    else
                    {
                        notifications.Complete();
                    }
                }

                insideObserver = false;
            },
            _ =>
            {
                completionOverlapped = insideObserver;
                sequence.Add(ErrorMarker);
            },
            () =>
            {
                completionOverlapped = insideObserver;
                sequence.Add(CompletionMarker);
            });

        var emitsPending = !sameValue || !distinct;
        await Assert.That(sequence).Count().IsEqualTo(valueAndTerminalCount + (emitsPending ? 1 : 0));
        await Assert.That(sequence[0]).IsEqualTo(InitialName);
        if (emitsPending)
        {
            await Assert.That(sequence[1]).IsEqualTo(sameValue ? InitialName : UpdatedName);
        }

        await Assert.That(sequence[^1]).IsEqualTo(fault ? ErrorMarker : CompletionMarker);
        await Assert.That(completionOverlapped).IsFalse();
    }

    /// <summary>Disposing inside the final getter prevents its value and terminal from reaching the observer.</summary>
    /// <param name="fault">Whether the pending terminal is an error.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task OnNext_FinalGetterDisposes_StopsFinalValueAndTerminal(bool fault)
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var values = new List<string>();
        var terminated = false;
        var disposeOnRead = false;
        IDisposable? subscription = null;
        var observable = new PluginPropertyObservable<string>(
            notifications,
            fixture,
            NameExpression(),
            ObservedPropertyName,
            _ =>
            {
                if (disposeOnRead)
                {
                    subscription!.Dispose();
                }

                return fixture.Name;
            },
            false,
            true);
        subscription = observable.Subscribe(
            value =>
            {
                values.Add(value);
                if (value == UpdatedName)
                {
                    fixture.Name = "final";
                    notifications.Raise(fixture);
                    disposeOnRead = true;
                    if (fault)
                    {
                        notifications.Fault(new InvalidOperationException(ErrorMarker));
                    }
                    else
                    {
                        notifications.Complete();
                    }
                }
            },
            _ => terminated = true,
            () => terminated = true);
        using (subscription)
        {
            fixture.Name = UpdatedName;
            notifications.Raise(fixture);

            await Assert.That(values).IsEquivalentTo(InitialThenUpdated);
            await Assert.That(terminated).IsFalse();
            await Assert.That(notifications.SubscriberCount).IsEqualTo(0);
        }
    }

    /// <summary>Subscribing reports the property's current value without waiting for a notification.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_BeforeAnyNotification_ReportsTheCurrentValue()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();

        using (Build(fixture, notifications).Subscribe(new RecordingObserver<string>(observed, [])))
        {
            await Assert.That(observed).IsEquivalentTo(InitialOnly);
        }
    }

    /// <summary>
    /// A notification from the registration re-reads the property through the emitted accessor, so the value
    /// comes from the object rather than from the change the registration handed over.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_RegistrationNotifies_ReadsTheValueBackOffTheObject()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();

        using (Build(fixture, notifications).Subscribe(new RecordingObserver<string>(observed, [])))
        {
            fixture.Name = UpdatedName;
            notifications.Raise(fixture);

            await Assert.That(observed).IsEquivalentTo(InitialThenUpdated);
        }
    }

    /// <summary>A notification that leaves the value alone is not passed on when the distinct gate is asked for.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_RegistrationNotifiesWithoutAChange_SuppressesTheDuplicate()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();

        using (Build(fixture, notifications).Subscribe(new RecordingObserver<string>(observed, [])))
        {
            notifications.Raise(fixture);

            await Assert.That(observed).IsEquivalentTo(InitialOnly);
        }
    }

    /// <summary>Disposing stops the observation and drops the registration's own subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_AfterSubscribing_StopsObservingAndReleasesTheRegistration()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();

        var subscription = Build(fixture, notifications).Subscribe(new RecordingObserver<string>(observed, []));
        subscription.Dispose();

        fixture.Name = UpdatedName;
        notifications.Raise(fixture);

        await Assert.That(observed).IsEquivalentTo(InitialOnly);
        await Assert.That(notifications.SubscriberCount).IsEqualTo(0);
    }

    /// <summary>A getter that throws on subscribe propagates and drops the registration's own subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_GetterThrows_ReleasesTheRegistration()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();
        var fail = true;
        var observable = new PluginPropertyObservable<string>(
            notifications,
            fixture,
            NameExpression(),
            ObservedPropertyName,
            source => fail ? throw new InvalidOperationException("getter") : ((ObservedFixture)source).Name,
            false,
            true);

        var action = () => observable.Subscribe(new RecordingObserver<string>(observed, []));
        await Assert.That(action).ThrowsExactly<InvalidOperationException>();

        fail = false;
        notifications.Raise(fixture);

        await Assert.That(notifications.SubscriberCount).IsEqualTo(0);
        await Assert.That(observed).IsEmpty();
    }

    /// <summary>A registration that notifies from inside its own subscribe is still released when the getter throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_RegistrationNotifiesDuringSubscribeAndGetterThrows_ReleasesTheRegistration()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications { NotifyDuringSubscribe = fixture };
        var observed = new List<string>();
        var fail = true;
        var observable = new PluginPropertyObservable<string>(
            notifications,
            fixture,
            NameExpression(),
            ObservedPropertyName,
            source => fail ? throw new InvalidOperationException("getter") : ((ObservedFixture)source).Name,
            false,
            true);

        var action = () => observable.Subscribe(new RecordingObserver<string>(observed, []));
        await Assert.That(action).ThrowsExactly<InvalidOperationException>();

        fail = false;
        notifications.Raise(fixture);

        await Assert.That(notifications.SubscriberCount).IsEqualTo(0);
        await Assert.That(observed).IsEmpty();
    }

    /// <summary>Every argument the observation cannot work without is rejected outright.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullRegistration_ThrowsArgumentNullException()
    {
        var action = static () => new PluginPropertyObservable<string>(
            null!,
            new ObservedFixture(),
            NameExpression(),
            ObservedPropertyName,
            static observed => ((ObservedFixture)observed).Name,
            false,
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>The observed object is required, because the value is read back off it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = static () => new PluginPropertyObservable<string>(
            new PluginNotifications(),
            null!,
            NameExpression(),
            ObservedPropertyName,
            static observed => ((ObservedFixture)observed).Name,
            false,
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>The accessor is required, because it is what replaces reading the change by reflection.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var action = static () => new PluginPropertyObservable<string>(
            new PluginNotifications(),
            new ObservedFixture(),
            NameExpression(),
            ObservedPropertyName,
            null!,
            false,
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A fault from the registration reaches the observer rather than being swallowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_RegistrationFaults_PassesTheFaultOn()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var faults = new List<Exception>();

        using (Build(fixture, notifications).Subscribe(new RecordingObserver<string>([], faults)))
        {
            notifications.Fault(new InvalidOperationException("registration gave up"));

            await Assert.That(faults).Count().IsEqualTo(1);
        }
    }

    /// <summary>A registration that finishes ends the observation with it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_RegistrationCompletes_EndsTheObservation()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observer = new RecordingObserver<string>([], []);

        using (Build(fixture, notifications).Subscribe(observer))
        {
            notifications.Complete();

            await Assert.That(observer.Completed).IsTrue();
        }
    }

    /// <summary>Disposing twice releases the registration once.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_CalledTwice_ReleasesTheRegistrationOnce()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();

        var subscription = Build(fixture, notifications).Subscribe(new RecordingObserver<string>([], []));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(notifications.SubscriberCount).IsEqualTo(0);
    }

    /// <summary>
    /// A notification that races past disposal is dropped. The registration is told to stop, but it may
    /// already be part way through delivering one, and the observation must not report after it has ended.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_NotifiedAfterDisposal_ReportsNothingFurther()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observed = new List<string>();

        var subscription = Build(fixture, notifications).Subscribe(new RecordingObserver<string>(observed, []));
        subscription.Dispose();

        fixture.Name = UpdatedName;
        notifications.RaisePastUnsubscription(fixture);

        await Assert.That(observed).IsEquivalentTo(InitialOnly);
    }

    /// <summary>
    /// A fault that races past disposal is dropped rather than reported. The registration is told to stop, but
    /// it may already be part way through faulting, and an observation that has ended must not raise on it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_FaultedAfterDisposal_ReportsNothingFurther()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var faults = new List<Exception>();

        var subscription = Build(fixture, notifications).Subscribe(new RecordingObserver<string>([], faults));
        subscription.Dispose();

        notifications.FaultPastUnsubscription(new InvalidOperationException("after disposal"));

        await Assert.That(faults).IsEmpty();
    }

    /// <summary>A completion that races past disposal is dropped, for the same reason a fault is.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_CompletedAfterDisposal_ReportsNothingFurther()
    {
        var fixture = new ObservedFixture { Name = InitialName };
        var notifications = new PluginNotifications();
        var observer = new RecordingObserver<string>([], []);

        var subscription = Build(fixture, notifications).Subscribe(observer);
        subscription.Dispose();

        notifications.CompletePastUnsubscription();

        await Assert.That(observer.Completed).IsFalse();
    }

    /// <summary>Names the observed property the way generated code does, as a compiler-built expression.</summary>
    /// <returns>The body of a lambda naming the property.</returns>
    private static System.Linq.Expressions.Expression NameExpression()
    {
        Expression<Func<ObservedFixture, string>> property = fixture => fixture.Name;
        return property.Body;
    }

    /// <summary>Builds the observation under test over a fixture and a registration.</summary>
    /// <param name="fixture">The object being observed.</param>
    /// <param name="notifications">The registration driving the observation.</param>
    /// <param name="distinct">Whether equal consecutive values are suppressed.</param>
    /// <returns>The observation.</returns>
    private static PluginPropertyObservable<string> Build(ObservedFixture fixture, PluginNotifications notifications, bool distinct = true) =>
        new(
            notifications,
            fixture,
            NameExpression(),
            ObservedPropertyName,
            static observed => ((ObservedFixture)observed).Name,
            false,
            distinct);

    /// <summary>An object with a property to observe.</summary>
    private sealed class ObservedFixture : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets or sets the observed property.</summary>
        public string Name
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        = string.Empty;
    }

    /// <summary>A registration whose notifications the test raises by hand.</summary>
    private sealed class PluginNotifications : ICreatesObservableForProperty, IObservable<IObservedChange<object, object?>>
    {
        /// <summary>The observers currently subscribed through the registration.</summary>
        private readonly List<IObserver<IObservedChange<object, object?>>> _observers = [];

        /// <summary>Whoever subscribed last, kept so a notification can be delivered past unsubscription.</summary>
        private IObserver<IObservedChange<object, object?>>? _lastObserver;

        /// <summary>Gets how many observers are still subscribed.</summary>
        public int SubscriberCount => _observers.Count;

        /// <summary>Gets or sets the object to notify about from inside <see cref="Subscribe"/>, or <see langword="null"/> to stay silent.</summary>
        public object? NotifyDuringSubscribe { get; set; }

        /// <summary>Gets or sets an action raised after attaching the observer.</summary>
        public Action? AfterSubscribe { get; set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) => int.MaxValue;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings) => this;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer)
        {
            _observers.Add(observer);
            _lastObserver = observer;
            if (NotifyDuringSubscribe is { } sender)
            {
                observer.OnNext(new ObservedChange<object, object?>(sender, null, null));
            }

            AfterSubscribe?.Invoke();
            return new Unsubscriber(_observers, observer);
        }

        /// <summary>Raises one notification to every subscriber.</summary>
        /// <param name="sender">The object the notification is about.</param>
        public void Raise(object sender)
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(new ObservedChange<object, object?>(sender, null, null));
            }
        }

        /// <summary>Raises one notification to whoever subscribed last, whether or not it has since unsubscribed.</summary>
        /// <param name="sender">The object the notification is about.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaisePastUnsubscription(object sender) =>
            _lastObserver?.OnNext(new ObservedChange<object, object?>(sender, null, null));

        /// <summary>Faults whoever subscribed last, whether or not it has since unsubscribed.</summary>
        /// <param name="error">The fault to report.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FaultPastUnsubscription(Exception error) => _lastObserver?.OnError(error);

        /// <summary>Completes whoever subscribed last, whether or not it has since unsubscribed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompletePastUnsubscription() => _lastObserver?.OnCompleted();

        /// <summary>Faults every subscriber.</summary>
        /// <param name="error">The fault to report.</param>
        public void Fault(Exception error)
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnError(error);
            }
        }

        /// <summary>Completes every subscriber.</summary>
        public void Complete()
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnCompleted();
            }
        }

        /// <summary>Removes one observer when its subscription is dropped.</summary>
        /// <param name="observers">The subscribed observers.</param>
        /// <param name="observer">The observer to remove.</param>
        private sealed class Unsubscriber(
            List<IObserver<IObservedChange<object, object?>>> observers,
            IObserver<IObservedChange<object, object?>> observer) : IDisposable
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => observers.Remove(observer);
        }
    }

    /// <summary>Records everything an observation emits.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="received">The list each value is appended to.</param>
    /// <param name="faults">The list each fault is appended to.</param>
    private sealed class RecordingObserver<T>(List<T> received, List<Exception> faults) : IObserver<T>
    {
        /// <summary>Gets a value indicating whether the observation ended.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value) => received.Add(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => faults.Add(error);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => Completed = true;
    }
}
