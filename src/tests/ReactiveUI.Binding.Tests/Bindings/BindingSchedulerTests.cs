// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Covers which thread a binding delivers its write on.</summary>
[NotInParallel]
public class BindingSchedulerTests
{
    /// <summary>The value driven through a routed binding.</summary>
    private const string Written = "written";

    /// <summary>A target no invoker claims.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>A write from the thread that owns the target is applied inline, even when the host set a main thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_OnTheOwningThread_WritesInline()
    {
        var target = new object();
        var invoker = new StubViewThreadInvoker(target) { HasAccess = true };
        var host = new RecordingSynchronizationContext();
        var observer = new RecordingObserver<string>();

        using (RegisterInvoker(invoker))
        using (UseMainThread(host))
        {
            var source = new ManualObservable<string>();
            using var subscription = BindingSchedulers.ObserveOnViewThread(source, target).Subscribe(observer);

            source.Observer?.OnNext(Written);

            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(Written);
            await Assert.That(invoker.PostCount).IsEqualTo(0);
            await Assert.That(host.PostCount).IsEqualTo(0);
        }
    }

    /// <summary>A write from another thread is posted through the invoker that claims the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_FromAnotherThread_PostsThroughTheInvoker()
    {
        var target = new object();
        var invoker = new StubViewThreadInvoker(target);
        var observer = new RecordingObserver<string>();

        using (RegisterInvoker(invoker))
        {
            var source = new ManualObservable<string>();
            using var subscription = BindingSchedulers.ObserveOnViewThread(source, target).Subscribe(observer);

            source.Observer?.OnNext(Written);

            await Assert.That(observer.Values.Count).IsEqualTo(0);
            await Assert.That(invoker.PostCount).IsEqualTo(1);

            invoker.RunPosted();

            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(Written);
        }
    }

    /// <summary>A write from another thread goes through the main thread the host set, not the invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_FromAnotherThreadWhenTheHostSetsAMainThread_DeliversThroughIt()
    {
        var target = new object();
        var invoker = new StubViewThreadInvoker(target);
        var host = new RecordingSynchronizationContext();
        var observer = new RecordingObserver<string>();

        using (RegisterInvoker(invoker))
        using (UseMainThread(host))
        {
            var source = new ManualObservable<string>();
            using var subscription = BindingSchedulers.ObserveOnViewThread(source, target).Subscribe(observer);

            source.Observer?.OnNext(Written);

            await Assert.That(host.PostCount).IsEqualTo(1);
            await Assert.That(invoker.PostCount).IsEqualTo(0);
            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(Written);
        }
    }

    /// <summary>A target nothing claims is written where the notification was raised, even when the host set a main thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WhenNothingClaimsTheTarget_HandsBackTheSource()
    {
        using (RegisterInvoker(new StubViewThreadInvoker()))
        using (UseMainThread(new RecordingSynchronizationContext()))
        {
            var source = new ManualObservable<string>();

            await Assert.That(BindingSchedulers.ObserveOnViewThread(source, UnclaimedTarget)).IsSameReferenceAs(source);
        }
    }

    /// <summary>With no target there is nothing to route, so the source is handed back.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithNoTarget_HandsBackTheSource()
    {
        var source = new ManualObservable<string>();

        await Assert.That(BindingSchedulers.ObserveOnViewThread(source, target: null)).IsSameReferenceAs(source);
    }

    /// <summary>Two targets claimed by different invokers are each posted through their own.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithTwoTargets_PostsEachThroughItsOwnInvoker()
    {
        var firstTarget = new object();
        var secondTarget = new object();
        var firstInvoker = new StubViewThreadInvoker(firstTarget);
        var secondInvoker = new StubViewThreadInvoker(secondTarget);

        using (RegisterInvoker(firstInvoker))
        using (RegisterInvoker(secondInvoker))
        {
            var first = new ManualObservable<string>();
            var second = new ManualObservable<string>();

            using var firstSub = BindingSchedulers.ObserveOnViewThread(first, firstTarget).Subscribe(new RecordingObserver<string>());
            using var secondSub = BindingSchedulers.ObserveOnViewThread(second, secondTarget).Subscribe(new RecordingObserver<string>());

            first.Observer?.OnNext(Written);

            await Assert.That(firstInvoker.PostCount).IsEqualTo(1);
            await Assert.That(secondInvoker.PostCount).IsEqualTo(0);
        }
    }

    /// <summary>The fallback a generated binding passes is used when no registered invoker claims the target.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithAFallback_UsesItWhenNothingRegisteredClaimsTheTarget()
    {
        var fallback = new StubViewThreadInvoker();
        var observer = new RecordingObserver<string>();

        using (RegisterInvoker(new StubViewThreadInvoker()))
        {
            var source = new ManualObservable<string>();
            using var subscription = BindingSchedulers.ObserveOnViewThread(source, UnclaimedTarget, fallback).Subscribe(observer);

            source.Observer?.OnNext(Written);
            fallback.RunPosted();

            await Assert.That(fallback.PostCount).IsEqualTo(1);
            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(Written);
        }
    }

    /// <summary>A registered invoker that claims the target takes precedence over the fallback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithAFallback_PrefersARegisteredInvoker()
    {
        var target = new object();
        var registered = new StubViewThreadInvoker(target);
        var fallback = new StubViewThreadInvoker();

        using (RegisterInvoker(registered))
        {
            var source = new ManualObservable<string>();
            using var subscription = BindingSchedulers.ObserveOnViewThread(source, target, fallback).Subscribe(new RecordingObserver<string>());

            source.Observer?.OnNext(Written);

            await Assert.That(registered.PostCount).IsEqualTo(1);
            await Assert.That(fallback.PostCount).IsEqualTo(0);
        }
    }

    /// <summary>With a fallback but no target there is nothing to route, so the source is handed back.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithAFallbackAndNoTarget_HandsBackTheSource()
    {
        var source = new ManualObservable<string>();

        await Assert.That(BindingSchedulers.ObserveOnViewThread(source, null, new StubViewThreadInvoker())).IsSameReferenceAs(source);
    }

    /// <summary>A missing source is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithNoSource_Throws() =>
        await Assert.That(static () => BindingSchedulers.ObserveOnViewThread<string>(null!, UnclaimedTarget)).ThrowsExactly<ArgumentNullException>();

    /// <summary>A missing source is rejected when a fallback is passed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithAFallbackButNoSource_Throws() =>
        await Assert.That(static () => BindingSchedulers.ObserveOnViewThread<string>(null!, UnclaimedTarget, new StubViewThreadInvoker()))
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>A missing fallback is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithNoFallback_Throws() =>
        await Assert.That(static () => BindingSchedulers.ObserveOnViewThread(new ManualObservable<string>(), UnclaimedTarget, null!))
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>A null target claims nothing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ForTarget_WithNoTarget_ClaimsNothing()
    {
        ViewThreadInvokers.Refresh();

        await Assert.That(ViewThreadInvokers.ForTarget(target: null)).IsNull();
    }

    /// <summary>Clearing the synchronization context clears the main thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseSynchronizationContext_WithNoContext_ClearsTheMainThread()
    {
        BindingSchedulers.UseSynchronizationContext(new RecordingSynchronizationContext());
        BindingSchedulers.UseSynchronizationContext(null);

        await Assert.That(BindingSchedulers.MainThread).IsNull();
    }

    /// <summary>Registers an invoker and drops the resolved set again on dispose.</summary>
    /// <param name="invoker">The invoker to register.</param>
    /// <returns>A scope that drops the resolved set on dispose.</returns>
    private static InvokerScope RegisterInvoker(IViewThreadInvoker invoker)
    {
        Locator.CurrentMutable.RegisterConstant(invoker);
        ViewThreadInvokers.Refresh();

        return new();
    }

    /// <summary>Delivers writes from another thread through a context until the scope is disposed.</summary>
    /// <param name="context">The context standing in for the host's main thread.</param>
    /// <returns>A scope that clears the main thread on dispose.</returns>
    private static MainThreadScope UseMainThread(SynchronizationContext context)
    {
        BindingSchedulers.UseSynchronizationContext(context);

        return new();
    }

    /// <summary>Drops the resolved set so a later test does not inherit this one's invoker.</summary>
    private sealed class InvokerScope : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => ViewThreadInvokers.Refresh();
    }

    /// <summary>Clears the main thread so a later test does not inherit it.</summary>
    private sealed class MainThreadScope : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => BindingSchedulers.UseSynchronizationContext(null);
    }
}
