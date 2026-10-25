// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.ExceptionServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Backs a read-only "output property" with an observable. The property reports each new value through the
/// change callbacks it was created with, so the owning object can raise its own change notifications.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// <para>
/// This helper is normally created by <c>ToProperty</c>, whose generated code supplies callbacks that raise the
/// owning object's change notifications for the named property. It can also be created directly with callbacks
/// of your own.
/// </para>
/// <para>
/// Values equal to the previous value are skipped. With no scheduler, or with the immediate one, each value is
/// delivered on the thread that produced it. A value produced while another is being delivered waits for that
/// delivery to finish, so the callbacks never run concurrently and never nest.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// <![CDATA[
/// _fullName = this.WhenAnyValue(x => x.FirstName, x => x.LastName, (first, last) => $"{first} {last}")
///     .ToProperty(this, x => x.FullName);
///
/// public string FullName => _fullName.Value;
/// ]]>
/// </code>
/// </example>
[DebuggerDisplay("ObservableAsPropertyHelper: Value = {_core.LastValue}, IsSubscribed = {IsSubscribed}")]
public sealed class ObservableAsPropertyHelper<T> : IDisposable
{
    /// <summary>The state and source subscription behind this helper.</summary>
    private readonly Core _core;

    /// <summary>The lazily created <see cref="ThrownExceptions"/> stream.</summary>
    private ExceptionStream? _thrownExceptions;

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), default, null), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with an initial value.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, T? initialValue)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), new(initialValue, null), null), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with an initial value and a scheduler.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, T? initialValue, ISequencer? scheduler)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), new(initialValue, null), scheduler), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with an initial value and deferred subscription.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, T? initialValue, bool deferSubscription)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), new(initialValue, null), null), deferSubscription)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with an initial value, deferred subscription and a scheduler.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    public ObservableAsPropertyHelper(
        IObservable<T?> observable,
        Action<T?> onChanged,
        T? initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), new(initialValue, null), scheduler), deferSubscription)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, Action<T?>? onChanging)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), default, null), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback and an initial value.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, Action<T?>? onChanging, T? initialValue)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(initialValue, null), null), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback, an initial value and deferred subscription.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    public ObservableAsPropertyHelper(
        IObservable<T?> observable,
        Action<T?> onChanged,
        Action<T?>? onChanging,
        T? initialValue,
        bool deferSubscription)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(initialValue, null), null), deferSubscription)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback, an
    /// initial value, deferred subscription and a scheduler.
    /// </summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    public ObservableAsPropertyHelper(
        IObservable<T?> observable,
        Action<T?> onChanged,
        Action<T?>? onChanging,
        T? initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(initialValue, null), scheduler), deferSubscription)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback and an initial value factory.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the source produces one; null returns the default value.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, Action<T?>? onChanging, Func<T?>? getInitialValue)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(default, getInitialValue), null), false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback, an initial value factory and deferred subscription.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the source produces one; null returns the default value.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    public ObservableAsPropertyHelper(
        IObservable<T?> observable,
        Action<T?> onChanged,
        Action<T?>? onChanging,
        Func<T?>? getInitialValue,
        bool deferSubscription)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(default, getInitialValue), null), deferSubscription)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with an initial value factory and deferred subscription.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the source produces one; null returns the default value.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    public ObservableAsPropertyHelper(IObservable<T?> observable, Action<T?> onChanged, Func<T?> getInitialValue, bool deferSubscription)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, null), new(default, getInitialValue), null), deferSubscription)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class with a changing callback, an
    /// initial value factory, deferred subscription and a scheduler.
    /// </summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the source produces one; null returns the default value.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    public ObservableAsPropertyHelper(
        IObservable<T?> observable,
        Action<T?> onChanged,
        Action<T?>? onChanging,
        Func<T?>? getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        : this(new Core(observable, ChangeCallbacks.ForValues(onChanged, onChanging), new(default, getInitialValue), scheduler), deferSubscription)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}"/> class around a finished core.</summary>
    /// <param name="core">The fully constructed state; it is started here unless subscription is deferred.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    private ObservableAsPropertyHelper(Core core, bool deferSubscription)
    {
        _core = core;
        if (!deferSubscription)
        {
            core.Start();
        }
    }

    /// <summary>Gets the current value of the property, subscribing first when subscription was deferred.</summary>
    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _core.ReadValue()!;
    }

    /// <summary>Gets a value indicating whether the helper has subscribed to its observable.</summary>
    /// <remarks>With deferred subscription this stays false until <see cref="Value"/> is first read.</remarks>
    public bool IsSubscribed => _core.IsActivated;

    /// <summary>Gets an observable that reports each error the source produces.</summary>
    /// <remarks>An error that arrives while nothing observes this stream is rethrown on the thread that produced it.</remarks>
    public IObservable<Exception> ThrownExceptions => _thrownExceptions ??= new(_core);

    /// <summary>Creates a helper that raises an owner's change notifications through static callbacks.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="owner">The object passed to both callbacks.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="initialValue">The value the property holds before the source produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    /// <returns>The helper.</returns>
    /// <remarks>
    /// Generated <c>ToProperty</c> code calls this. The callbacks receive the owner rather than capturing it, so
    /// they are cached delegates and the helper allocates no closure.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ObservableAsPropertyHelper<T> Create(
        IObservable<T?> observable,
        object owner,
        Action<object?, T?> onChanged,
        Action<object?, T?>? onChanging,
        T? initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
    {
        ArgumentExceptionHelper.ThrowIfNull(owner);
        ArgumentExceptionHelper.ThrowIfNull(onChanged);

        return new(new Core(observable, new ChangeCallbacks(owner, onChanged, onChanging), new InitialValue(initialValue, null), scheduler), deferSubscription);
    }

    /// <summary>Creates a helper that raises an owner's change notifications through static callbacks, reading its initial value from a factory.</summary>
    /// <param name="observable">The observable the property follows.</param>
    /// <param name="owner">The object passed to both callbacks.</param>
    /// <param name="onChanged">The callback run after each new value is stored.</param>
    /// <param name="onChanging">The callback run before each new value is stored, or null for none.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the source produces one; null returns the default value.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of <see cref="Value"/> before subscribing.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    /// <returns>The helper.</returns>
    /// <remarks>Generated <c>ToProperty</c> code calls this for a call site that passes an initial value factory.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ObservableAsPropertyHelper<T> Create(
        IObservable<T?> observable,
        object owner,
        Action<object?, T?> onChanged,
        Action<object?, T?>? onChanging,
        Func<T?>? getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
    {
        ArgumentExceptionHelper.ThrowIfNull(owner);
        ArgumentExceptionHelper.ThrowIfNull(onChanged);

        return new(new Core(observable, new ChangeCallbacks(owner, onChanged, onChanging), new InitialValue(default, getInitialValue), scheduler), deferSubscription);
    }

    /// <summary>Creates a helper that holds the default value and never changes.</summary>
    /// <returns>The helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<T> Default() => Default(default, null);

    /// <summary>Creates a helper that holds one value and never changes.</summary>
    /// <param name="initialValue">The value the property holds.</param>
    /// <returns>The helper.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<T> Default(T? initialValue) => Default(initialValue, null);

    /// <summary>Creates a helper that holds one value and never changes.</summary>
    /// <param name="initialValue">The value the property holds.</param>
    /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
    /// <returns>The helper.</returns>
    public static ObservableAsPropertyHelper<T> Default(T? initialValue, ISequencer? scheduler) =>
        new(new Core(Signal.Never<T?>(), ChangeCallbacks.Ignored, new InitialValue(initialValue, null), scheduler), false);

    /// <summary>Stops following the observable. The property keeps its last value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _core.Dispose();

    /// <summary>The callbacks run before and after each value is stored, with the state each receives.</summary>
    /// <param name="ChangedState">The state passed to <paramref name="OnChanged"/>.</param>
    /// <param name="OnChanged">The callback run after each value is stored.</param>
    /// <param name="ChangingState">The state passed to <paramref name="OnChanging"/>.</param>
    /// <param name="OnChanging">The callback run before each value is stored, or null for none.</param>
    private readonly record struct ChangeCallbacks(
        object? ChangedState,
        Action<object?, T?> OnChanged,
        object? ChangingState,
        Action<object?, T?>? OnChanging)
    {
        /// <summary>Invokes a caller-supplied value callback that was stored as the callback state.</summary>
        private static readonly Action<object?, T?> _invokeValueCallback = static (state, value) => ((Action<T?>)state!)(value);

        /// <summary>Initializes a new instance of the <see cref="ChangeCallbacks"/> struct with one state for both callbacks.</summary>
        /// <param name="owner">The state passed to both callbacks.</param>
        /// <param name="onChanged">The callback run after each value is stored.</param>
        /// <param name="onChanging">The callback run before each value is stored, or null for none.</param>
        public ChangeCallbacks(object? owner, Action<object?, T?> onChanged, Action<object?, T?>? onChanging)
            : this(owner, onChanged, owner, onChanging)
        {
        }

        /// <summary>Gets callbacks that report a value to nobody.</summary>
        public static ChangeCallbacks Ignored { get; } = new(null, static (_, _) => { }, null);

        /// <summary>Wraps caller-supplied value callbacks without allocating an adapter for each.</summary>
        /// <param name="onChanged">The callback run after each value is stored.</param>
        /// <param name="onChanging">The callback run before each value is stored, or null for none.</param>
        /// <returns>The callbacks, each invoking the delegate stored as its state.</returns>
        public static ChangeCallbacks ForValues(Action<T?> onChanged, Action<T?>? onChanging)
        {
            ArgumentExceptionHelper.ThrowIfNull(onChanged);

            return new(onChanged, _invokeValueCallback, onChanging, onChanging is null ? null : _invokeValueCallback);
        }
    }

    /// <summary>The value the property holds before the source produces one.</summary>
    /// <param name="Value">The value, used when there is no factory.</param>
    /// <param name="Factory">Returns the value; null uses <paramref name="Value"/>.</param>
    private readonly record struct InitialValue(T? Value, Func<T?>? Factory)
    {
        /// <summary>Reads the initial value.</summary>
        /// <returns>The factory's result, or the stored value when there is no factory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Read() => Factory is null ? Value : Factory();
    }

    /// <summary>A value on its way to the scheduler.</summary>
    /// <param name="Owner">The core delivering it.</param>
    /// <param name="Value">The value.</param>
    private readonly record struct ScheduledDelivery(Core Owner, T? Value);

    /// <summary>
    /// The helper's state, which also observes the source. It is a separate object so the public constructors can
    /// finish building it before handing it to the source.
    /// </summary>
    private sealed class Core : IObserver<T?>, IDisposable
    {
        /// <summary>The callbacks run before and after each value is stored.</summary>
        private readonly ChangeCallbacks _callbacks;

        /// <summary>The value the property holds before the source produces one.</summary>
        private readonly InitialValue _initialValue;

        /// <summary>The scheduler deliveries run on, or null to deliver on the producing thread.</summary>
        private readonly ISequencer? _scheduler;

        /// <summary>The observable the property follows.</summary>
        private readonly IObservable<T?> _source;

        /// <summary>Guards the distinct gate, the delivery queue and the disposed flag; never held while a callback runs.</summary>
        /// <remarks>
        /// A flag taken with one compare-exchange rather than a <c>Lock</c> or a <see cref="SpinLock"/>: each critical
        /// section is a few field reads and writes and no callback runs inside one, so a waiter spins briefly instead of
        /// parking, and an int needs no separate lock object. A waiter backs off with <see cref="SpinWait"/>, which
        /// yields when the holder has been preempted. Nothing re-enters the gate. 1 while held, 0 while free.
        /// </remarks>
        private int _gate;

        /// <summary>The observers of the helper's exception stream.</summary>
        private Broadcaster<Exception> _exceptions;

        /// <summary>Values waiting behind the running delivery; created the first time a value has to wait.</summary>
        private Queue<T?>? _pending;

        /// <summary>The last value that passed the distinct gate, seeded with the initial value before the source is followed.</summary>
        private T? _distinctPrevious;

        /// <summary>Whether a delivery is running on the producing thread.</summary>
        private bool _delivering;

        /// <summary>The subscription to the source.</summary>
        private IDisposable? _subscription;

        /// <summary>1 once the helper is disposed; taken with <see cref="Interlocked.Exchange(ref int, int)"/>.</summary>
        private int _disposed;

        /// <summary>1 once the source has been subscribed to, or once subscription was claimed.</summary>
        private int _activated;

        /// <summary>The most recently delivered value.</summary>
        private T? _lastValue;

        /// <summary>Initializes a new instance of the <see cref="Core"/> class.</summary>
        /// <param name="observable">The observable the property follows.</param>
        /// <param name="callbacks">The callbacks run before and after each value is stored.</param>
        /// <param name="initialValue">The value the property holds before the source produces one.</param>
        /// <param name="scheduler">The scheduler the callbacks run on, or null to run them on the producing thread.</param>
        public Core(IObservable<T?> observable, in ChangeCallbacks callbacks, in InitialValue initialValue, ISequencer? scheduler)
        {
            ArgumentExceptionHelper.ThrowIfNull(observable);

            _source = observable;
            _callbacks = callbacks;
            _initialValue = initialValue;
            _scheduler = scheduler is null || IsImmediate(scheduler) ? null : scheduler;
        }

        /// <summary>Gets a value indicating whether the source has been subscribed to, or subscription was claimed.</summary>
        public bool IsActivated => Volatile.Read(ref _activated) > 0;

        /// <summary>Gets the most recently delivered value without activating a deferred subscription.</summary>
        public T? LastValue => _lastValue;

        /// <summary>Delivers the initial value like any other, then follows the source.</summary>
        public void Start()
        {
            var initial = _initialValue.Read();
            _lastValue = initial;
            _distinctPrevious = initial;
            Volatile.Write(ref _activated, 1);
            Publish(initial);
            _subscription = _source.Subscribe(this);
        }

        /// <summary>Reads the current value, subscribing first when subscription was deferred and nothing has claimed it.</summary>
        /// <returns>The most recently delivered value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadValue()
        {
            if (Volatile.Read(ref _activated) == 0 && Interlocked.CompareExchange(ref _activated, 1, 0) == 0)
            {
                Activate();
            }

            return _lastValue;
        }

        /// <summary>Stops following the source and drops values waiting for delivery.</summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            using (EnterGate())
            {
                _pending?.Clear();
            }

            _subscription?.Dispose();
        }

        /// <summary>Passes a source value through the distinct gate and delivers it.</summary>
        /// <param name="value">The value the source produced.</param>
        public void OnNext(T? value)
        {
            using (EnterGate())
            {
                if (Volatile.Read(ref _disposed) != 0 || EqualityComparer<T?>.Default.Equals(value, _distinctPrevious))
                {
                    return;
                }

                _distinctPrevious = value;

                if (_scheduler is null && !TryClaimDelivery(value))
                {
                    return;
                }
            }

            if (_scheduler is null)
            {
                DeliverSerialized(value);
                return;
            }

            Schedule(value);
        }

        /// <summary>Reports a source error to the helper's exception observers, or rethrows it when there are none.</summary>
        /// <param name="error">The error the source produced.</param>
        public void OnError(Exception error)
        {
            if (!_exceptions.HasObservers)
            {
                ExceptionDispatchInfo.Capture(error).Throw();
            }

            _exceptions.Next(error);
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <summary>Adds an observer of the helper's exception stream.</summary>
        /// <param name="observer">The observer to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddExceptionObserver(IObserver<Exception> observer) => _exceptions.Add(observer);

        /// <summary>Removes an observer of the helper's exception stream.</summary>
        /// <param name="observer">The observer to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveExceptionObserver(IObserver<Exception> observer) => _exceptions.Remove(observer);

        /// <summary>Determines whether a sequencer runs work on the calling thread at once.</summary>
        /// <param name="scheduler">The sequencer to test.</param>
        /// <returns><see langword="true"/> for the immediate sequencer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsImmediate(ISequencer scheduler) =>
#if REACTIVE_SHIM
            scheduler == Scheduler.Immediate;
#else
            scheduler == Sequencer.Immediate;
#endif

        /// <summary>Takes the gate until the returned scope is disposed.</summary>
        /// <returns>The scope that releases the gate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GateScope EnterGate()
        {
            if (Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            {
                WaitForGate();
            }

            return new(this);
        }

        /// <summary>Spins, backing off, until the gate is free and this thread takes it.</summary>
        /// <remarks>
        /// A critical section lasts nanoseconds, so a waiter that sleeps for a millisecond waits far longer than the
        /// holder needs. The backoff spins and yields but never calls <c>Thread.Sleep(1)</c> where the runtime offers
        /// that; .NET Framework has no such overload.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WaitForGate()
        {
            // SpinWait is a mutable struct that counts its own spins, so it stays one local across the loop.
            var spinner = default(SpinWait);
            SpinOnce(ref spinner);
            while (Volatile.Read(ref _gate) != 0 || Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            {
                SpinOnce(ref spinner);
            }

            static void SpinOnce(ref SpinWait spinner)
            {
#if NETCOREAPP3_0_OR_GREATER
                spinner.SpinOnce(-1);
#else
                spinner.SpinOnce();
#endif
            }
        }

        /// <summary>Reads the initial value and subscribes, the first time the value is read after deferred subscription.</summary>
        private void Activate()
        {
            // The helper already holds its initial value without having announced it, so a source that opens by
            // repeating it has not changed anything: seeding the distinct gate with it skips that value. A helper
            // disposed before its first read still reports the initial value it was given; it just never subscribes.
            var initial = _initialValue.Read();
            _lastValue = initial;
            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            using (EnterGate())
            {
                _distinctPrevious = initial;
            }

            var subscription = _source.Subscribe(this);

            using (EnterGate())
            {
                if (Volatile.Read(ref _disposed) == 0)
                {
                    _subscription = subscription;
                    return;
                }
            }

            subscription.Dispose();
        }

        /// <summary>Delivers a value that bypassed the distinct gate, such as the initial value.</summary>
        /// <param name="value">The value to deliver.</param>
        private void Publish(T? value)
        {
            if (_scheduler is not null)
            {
                Schedule(value);
                return;
            }

            using (EnterGate())
            {
                if (!TryClaimDelivery(value))
                {
                    return;
                }
            }

            DeliverSerialized(value);
        }

        /// <summary>Claims the delivery for the calling thread, or queues the value behind the running one.</summary>
        /// <param name="value">The value to deliver.</param>
        /// <returns><see langword="true"/> when the caller now delivers; <see langword="false"/> when the value was queued.</returns>
        /// <remarks>Called with <see cref="_gate"/> held.</remarks>
        private bool TryClaimDelivery(T? value)
        {
            if (!_delivering)
            {
                _delivering = true;
                return true;
            }

            (_pending ??= new()).Enqueue(value);
            return false;
        }

        /// <summary>Delivers a value, then every value queued while it ran.</summary>
        /// <param name="value">The value the calling thread claimed the delivery for.</param>
        private void DeliverSerialized(T? value)
        {
            try
            {
                while (true)
                {
                    Deliver(value);

                    using (EnterGate())
                    {
                        if (Volatile.Read(ref _disposed) != 0 || _pending is not { Count: > 0 } pending)
                        {
                            _delivering = false;
                            return;
                        }

                        value = pending.Dequeue();
                    }
                }
            }
            catch
            {
                // A later value must not be delivered ahead of the ones this failure stranded, so they are dropped.
                using (EnterGate())
                {
                    _delivering = false;
                    _pending?.Clear();
                }

                throw;
            }
        }

        /// <summary>Delivers a value on the scheduler.</summary>
        /// <param name="value">The value to deliver.</param>
        private void Schedule(T? value) =>
            _ = _scheduler!.Schedule(
                new ScheduledDelivery(this, value),
                static (_, delivery) =>
                {
                    delivery.Owner.Deliver(delivery.Value);
                    return EmptyDisposable.Instance;
                });

        /// <summary>Runs the changing callback, stores the value, then runs the changed callback.</summary>
        /// <param name="value">The value being delivered.</param>
        private void Deliver(T? value)
        {
            _callbacks.OnChanging?.Invoke(_callbacks.ChangingState, value);
            _lastValue = value;
            _callbacks.OnChanged(_callbacks.ChangedState, value);
        }

        /// <summary>Releases the gate when disposed; a struct, so a <c>using</c> over it allocates nothing.</summary>
        /// <param name="owner">The core whose gate was taken.</param>
        private readonly struct GateScope(Core owner) : IDisposable
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => Volatile.Write(ref owner._gate, 0);
        }
    }

    /// <summary>The <see cref="ThrownExceptions"/> stream.</summary>
    /// <param name="core">The helper's state.</param>
    private sealed class ExceptionStream(Core core) : IObservable<Exception>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<Exception> observer)
        {
            ArgumentExceptionHelper.ThrowIfNull(observer);
            core.AddExceptionObserver(observer);
            return new ExceptionSubscription(core, observer);
        }
    }

    /// <summary>Removes an observer of <see cref="ThrownExceptions"/> when disposed.</summary>
    /// <param name="core">The helper's state.</param>
    /// <param name="observer">The observer.</param>
    private sealed class ExceptionSubscription(Core core, IObserver<Exception> observer) : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => core.RemoveExceptionObserver(observer);
    }
}
