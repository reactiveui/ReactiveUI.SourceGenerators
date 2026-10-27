// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>
/// Covers what an expression chain does with the signals a link's notification stream can produce: an
/// error, a completion, a change whose sender is gone, and one that arrives after the subscription was
/// disposed. A stub notifier stands in for the platform observers so each signal is delivered
/// deliberately rather than raced for.
/// </summary>
/// <remarks>
/// These replace the process-wide notifier registration, so they cannot share the runner with tests
/// that rely on the real one.
/// </remarks>
[NotInParallel]
public class ExpressionChainNotificationTests
{
    /// <summary>The value the fixture starts out holding.</summary>
    private const string InitialValue = "initial";

    /// <summary>
    /// The one notifier every test here drives. The notification factory for a type is resolved once and
    /// cached, so a per-test instance would be registered but never consulted - the tests would end up
    /// pushing into a stub nothing had subscribed to.
    /// </summary>
    private static readonly StubNotifier SharedNotifier = new();

    /// <summary>Verifies that a failing link surfaces on the chain rather than being swallowed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LinkError_IsForwardedDownstream()
    {
        using var context = Arrange();

        context.Notifier.PushError(new InvalidOperationException("link failed"));

        await Assert.That(context.Recorder.Error).IsTypeOf<InvalidOperationException>();
    }

    /// <summary>Verifies that a link completing does not complete or fault the chain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LinkCompletion_LeavesTheChainRunning()
    {
        using var context = Arrange();

        context.Notifier.PushCompleted();

        await Assert.That(context.Recorder.Error).IsNull();
        await Assert.That(context.Recorder.Completed).IsFalse();
    }

    /// <summary>Verifies that a notification whose sender is gone yields the default rather than throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotificationWithNoSender_YieldsTheDefaultValue()
    {
        using var context = Arrange();
        var before = context.Recorder.Values.Count;

        context.Notifier.PushChange(null);

        await Assert.That(context.Recorder.Values.Count).IsGreaterThan(before);
        await Assert.That(context.Recorder.Values[^1].Value).IsNull();
        await Assert.That(context.Recorder.Error).IsNull();
    }

    /// <summary>Verifies that a notification still in flight when the chain is disposed is dropped.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NotificationAfterDispose_IsIgnored()
    {
        using var context = Arrange();
        context.Subscription.Dispose();
        var after = context.Recorder.Values.Count;

        context.Notifier.PushChange(context.Fixture);

        await Assert.That(context.Recorder.Values.Count).IsEqualTo(after);
        await Assert.That(context.Recorder.Error).IsNull();
    }

    /// <summary>Builds a chain over the fixture whose only link is fed by the stub notifier.</summary>
    /// <returns>The arranged chain, its recorder and the notifier driving it.</returns>
    private static ChainContext Arrange()
    {
        var notifier = SharedNotifier;
        notifier.Reset();

        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.WithRegistration(static resolver =>
            resolver.RegisterConstant<ICreatesObservableForProperty>(SharedNotifier));
        _ = builder.BuildApp();

        var fixture = new ObservedValueFixture { Value = InitialValue };
        Expression<Func<ObservedValueFixture, string>> expr = x => x.Value;
        var recorder = new ChainRecorder();

        var subscription = fixture.SubscribeToExpressionChain<ObservedValueFixture, string>(
                expr.Body,
                false,
                false,
                false)
            .Subscribe(recorder);

        return new(notifier, fixture, recorder, subscription);
    }

    /// <summary>Records what a chain subscription produced, including the terminal signal.</summary>
    private sealed class ChainRecorder : IObserver<IObservedChange<ObservedValueFixture, string>>
    {
        /// <summary>Gets the values the chain emitted, in order.</summary>
        public List<IObservedChange<ObservedValueFixture, string>> Values { get; } = [];

        /// <summary>Gets the error the chain signalled, if any.</summary>
        public Exception? Error { get; private set; }

        /// <summary>Gets a value indicating whether the chain completed.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(IObservedChange<ObservedValueFixture, string> value) => Values.Add(value);

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnCompleted() => Completed = true;
    }

    /// <summary>
    /// Stands in for a platform observer, handing back a stream the test drives by hand. It claims only
    /// <see cref="ObservedValueFixture"/> so replacing the registration cannot affect anything else.
    /// </summary>
    private sealed class StubNotifier : ICreatesObservableForProperty
    {
        /// <summary>An affinity no real notifier bids, so this one is always chosen.</summary>
        private const int WinningAffinity = 1000;

        /// <summary>The chain's subscribers to the link's notifications.</summary>
        private readonly List<IObserver<IObservedChange<object, object?>>> _observers = [];

        /// <summary>The expression of the link most recently subscribed.</summary>
        private System.Linq.Expressions.Expression? _expression;

        /// <inheritdoc/>
        public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
            type == typeof(ObservedValueFixture) ? WinningAffinity : 0;

        /// <summary>Forgets the previous test's subscribers so each starts from an empty stream.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _observers.Clear();

        /// <inheritdoc/>
        public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
            object sender,
            System.Linq.Expressions.Expression expression,
            string propertyName,
            bool beforeChanged,
            bool suppressWarnings)
        {
            _expression = expression;
            return new Stream(this);
        }

        /// <summary>Delivers a change notification for the given sender to the chain.</summary>
        /// <param name="sender">The sender to report, or null to report that it is gone.</param>
        public void PushChange(object? sender)
        {
            var change = new ObservedChange<object, object?>(sender!, _expression, null);
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(change);
            }
        }

        /// <summary>Faults the link's notification stream.</summary>
        /// <param name="error">The error to report.</param>
        public void PushError(Exception error)
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnError(error);
            }
        }

        /// <summary>Completes the link's notification stream.</summary>
        public void PushCompleted()
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnCompleted();
            }
        }

        /// <summary>The notification stream handed to one chain link.</summary>
        /// <param name="owner">The notifier collecting the subscribers.</param>
        /// <remarks>
        /// Unhooking deliberately keeps the subscriber, so a test can deliver a notification that was
        /// already in flight when the chain was disposed. A real stream would have stopped, which is
        /// exactly why that guard cannot otherwise be reached on purpose.
        /// </remarks>
        private sealed class Stream(StubNotifier owner) : IObservable<IObservedChange<object, object?>>
        {
            /// <inheritdoc/>
            public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer)
            {
                owner._observers.Add(observer);
                return new KeepDelivering();
            }

            /// <summary>Unhooks the link without silencing the stream.</summary>
            private sealed class KeepDelivering : IDisposable
            {
                /// <inheritdoc/>
                public void Dispose()
                {
                }
            }
        }
    }

    /// <summary>An arranged chain and everything needed to drive and inspect it.</summary>
    /// <param name="Notifier">The stub feeding the chain's only link.</param>
    /// <param name="Fixture">The observed object.</param>
    /// <param name="Recorder">What the chain produced.</param>
    /// <param name="Subscription">The chain subscription.</param>
    private sealed record ChainContext(
        StubNotifier Notifier,
        ObservedValueFixture Fixture,
        ChainRecorder Recorder,
        IDisposable Subscription) : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Subscription.Dispose();
    }
}
