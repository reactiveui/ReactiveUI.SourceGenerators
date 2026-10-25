// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Tests initialization and lifetime of the shared binding-direction stream.</summary>
public class InitialBindingSignalTests
{
    /// <summary>A null observer is rejected before attaching the source.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        using var source = new Subject<bool>();
        var signal = new InitialBindingSignal(source);
        await Assert.That(() => signal.Subscribe(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(source.HasObservers).IsFalse();
    }

    /// <summary>Attachment snapshots are replaced by one initial model signal after attachment finishes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Subscribe_SynchronousSnapshots_EmitsOneInitialSignalAfterAttachment()
    {
        var attached = false;
        var wasAttachedAtDelivery = false;
        var source = Observable.Create<bool>(observer =>
        {
            observer.OnNext(false);
            observer.OnNext(true);
            attached = true;
            return Disposable.Empty;
        });
        var observer = new RecordingObserver<bool>(_ => wasAttachedAtDelivery = attached);
        using var subscription = new InitialBindingSignal(source).Subscribe(observer);

        await Assert.That(observer.Values).Count().IsEqualTo(1);
        await Assert.That(observer.Values[0]).IsTrue();
        await Assert.That(wasAttachedAtDelivery).IsTrue();
    }

    /// <summary>A change raised inside the initial observer is delivered after the initial call returns.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Subscribe_ReentrantUpdate_SerializesBehindInitialSignal()
    {
        const int expectedValues = 2;
        var source = new ManualObservable<bool>();
        var observer = new EmissionRecorder<bool> { OnFirstValue = () => source.Observer!.OnNext(false) };
        using var subscription = new InitialBindingSignal(source).Subscribe(observer);
        var values = observer.Snapshot();

        await Assert.That(values).Count().IsEqualTo(expectedValues);
        await Assert.That(values[0]).IsTrue();
        await Assert.That(values[1]).IsFalse();
        await Assert.That(observer.MaxConcurrentEmissions).IsEqualTo(1);
    }

    /// <summary>A synchronous terminal follows the initial signal and releases the source once.</summary>
    /// <param name="fault">Whether the source terminates with an error.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Subscribe_SynchronousTerminal_DeliversInitialThenTerminalAndDisposes(bool fault)
    {
        var disposals = new StrongBox<int>();
        var error = new InvalidOperationException("source error");
        var source = Observable.Create<bool>(observer =>
        {
            if (fault)
            {
                observer.OnError(error);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Create(disposals, static counter => counter.Value++);
        });
        var observer = new RecordingObserver<bool>();
        using var subscription = new InitialBindingSignal(source).Subscribe(observer);

        await Assert.That(observer.Values).Count().IsEqualTo(1);
        await Assert.That(observer.Values[0]).IsTrue();
        await Assert.That(observer.Error).IsSameReferenceAs(fault ? error : null);
        await Assert.That(observer.IsCompleted).IsEqualTo(!fault);
        await Assert.That(disposals.Value).IsEqualTo(1);
    }

    /// <summary>A failed initial observer call detaches the source before propagating.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Subscribe_InitialObserverThrows_DisposesSource()
    {
        using var source = new Subject<bool>();
        var observer = new RecordingObserver<bool>(static _ => throw new InvalidOperationException("observer error"));

        await Assert.That(() => new InitialBindingSignal(source).Subscribe(observer)).ThrowsExactly<InvalidOperationException>();
        await Assert.That(source.HasObservers).IsFalse();
    }

    /// <summary>Captured notifications cannot deliver after disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Dispose_LateNotifications_AreIgnored()
    {
        var source = new ManualObservable<bool>();
        var observer = new RecordingObserver<bool>();
        var subscription = new InitialBindingSignal(source).Subscribe(observer);
        subscription.Dispose();
        subscription.Dispose();
        source.Observer!.OnNext(false);
        source.Observer.OnError(new InvalidOperationException("late error"));
        source.Observer.OnCompleted();

        await Assert.That(observer.Values).Count().IsEqualTo(1);
        await Assert.That(observer.Error).IsNull();
        await Assert.That(observer.IsCompleted).IsFalse();
    }
}
