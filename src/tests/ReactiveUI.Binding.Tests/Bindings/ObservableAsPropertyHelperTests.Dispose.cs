// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Tests for how <see cref="ObservableAsPropertyHelper{T}.Dispose"/> stops the helper from following its source.</summary>
public sealed partial class ObservableAsPropertyHelperTests
{
    /// <summary>Disposing unsubscribes from the source; the property keeps the value it last delivered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_StopsFollowingSource_ValueKeepsLastDeliveredValue()
    {
        const int IgnoredValue = 2;
        var source = new Subject<int>();
        var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0);

        source.OnNext(SuppliedValue);
        fixture.Dispose();

        await Assert.That(() => source.OnNext(IgnoredValue)).ThrowsNothing();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>
    /// A value that still reaches the helper's observer after disposal - because the source kept the reference
    /// rather than honouring the unsubscription - is ignored by the disposed guard inside the observer itself.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_SourceKeepsPushingAnyway_DisposedGuardIgnoresLaterValues()
    {
        const int IgnoredValue = 2;
        var source = new ManualObservable<int>();
        var received = new List<int>();
        var fixture = new ObservableAsPropertyHelper<int>(source, received.Add, 0);
        received.Clear();

        source.Observer!.OnNext(SuppliedValue);
        fixture.Dispose();
        source.Observer!.OnNext(IgnoredValue);

        await Assert.That(received).IsEquivalentTo([SuppliedValue]);
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>Disposing twice is a no-op; the second call does not throw or change anything.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_CalledTwice_SecondCallIsNoOp()
    {
        var fixture = new ObservableAsPropertyHelper<int>(NeverSource, static _ => { }, SuppliedValue);

        fixture.Dispose();

        await Assert.That(() => fixture.Dispose()).ThrowsNothing();
        await Assert.That(fixture.Value).IsEqualTo(SuppliedValue);
    }

    /// <summary>
    /// A helper disposed reentrantly from inside the source's <c>Subscribe</c> call, while a deferred activation is
    /// still in progress, disposes the subscription it was about to keep instead of throwing or leaking it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_DuringDeferredActivation_DisposesTheNewSubscriptionRatherThanThrowing()
    {
        var source = new DisposeDuringSubscribeObservable();
        var fixture = new ObservableAsPropertyHelper<int>(source, static _ => { }, 0, true);
        source.Helper = fixture;

        await Assert.That(() =>
        {
            _ = fixture.Value;
            return Task.CompletedTask;
        }).ThrowsNothing();

        await Assert.That(source.SubscriptionDisposed).IsTrue();
    }

    /// <summary>An observable whose <c>Subscribe</c> call disposes the helper reentrantly before returning a subscription.</summary>
    private sealed class DisposeDuringSubscribeObservable : IObservable<int>
    {
        /// <summary>Gets or sets the helper to dispose from within <see cref="Subscribe"/>.</summary>
        public ObservableAsPropertyHelper<int>? Helper { get; set; }

        /// <summary>Gets a value indicating whether the subscription this observable handed out was disposed.</summary>
        public bool SubscriptionDisposed { get; private set; }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<int> observer)
        {
            Helper?.Dispose();
            return new TrackingDisposable(() => SubscriptionDisposed = true);
        }
    }

    /// <summary>A disposable that runs a callback the first time it is disposed.</summary>
    /// <param name="onDispose">The callback to run.</param>
    private sealed class TrackingDisposable(Action onDispose) : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => onDispose();
    }
}
