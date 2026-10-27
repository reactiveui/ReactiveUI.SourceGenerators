// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;

namespace ReactiveUI.Binding.Maui.Tests;

/// <summary>Tests for the MAUI view-thread invoker.</summary>
[NotInParallel]
public class DispatcherViewThreadInvokerTests
{
    /// <summary>A target the invoker does not recognise.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>The shared instance generated bindings route through is one invoker, reused.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Instance_IsSharedAndClaimsABindableObject()
    {
        await Assert.That(DispatcherViewThreadInvoker.Instance).IsSameReferenceAs(DispatcherViewThreadInvoker.Instance);
        await Assert.That(DispatcherViewThreadInvoker.Instance.Claims(new Label())).IsTrue();
    }

    /// <summary>A bindable object is claimed.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Claims_WithABindableObject_ClaimsIt() =>
        await Assert.That(new DispatcherViewThreadInvoker().Claims(new Label())).IsTrue();

    /// <summary>Anything that is not a bindable object is left to another invoker.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Claims_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new DispatcherViewThreadInvoker().Claims(UnclaimedTarget)).IsFalse();

    /// <summary>A caller the dispatcher says must dispatch may not write directly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CheckAccess_WhenTheDispatcherRequiresDispatch_IsFalse()
    {
        using (new DispatcherProviderScope(new RecordingDispatcher()))
        {
            await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(new Label())).IsFalse();
        }
    }

    /// <summary>A caller already on the dispatcher's thread may write directly.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CheckAccess_WhenTheDispatcherDoesNotRequireDispatch_IsTrue()
    {
        using (new DispatcherProviderScope(new RecordingDispatcher { IsDispatchRequired = false }))
        {
            await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(new Label())).IsTrue();
        }
    }

    /// <summary>An object with no dispatcher to find, as in a view's unit test, may be written from any thread.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CheckAccess_WithNoDispatcher_IsTrue()
    {
        using (new DispatcherProviderScope(null))
        {
            await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(new Label())).IsTrue();
        }
    }

    /// <summary>A posted callback goes through the dispatcher the object carries.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_GoesThroughTheDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var state = new object();
        object? received = null;

        using (new DispatcherProviderScope(dispatcher))
        {
            new DispatcherViewThreadInvoker().Post(new Label(), s => received = s, state);

            await Assert.That(dispatcher.DispatchCount).IsEqualTo(1);
            await Assert.That(received).IsSameReferenceAs(state);
        }
    }

    /// <summary>A callback posted to an object with no dispatcher to find runs inline rather than throwing.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_WithNoDispatcher_RunsInline()
    {
        var posted = false;

        using (new DispatcherProviderScope(null))
        {
            new DispatcherViewThreadInvoker().Post(new Label(), _ => posted = true, null);
        }

        await Assert.That(posted).IsTrue();
    }

    /// <summary>An object that picks a dispatcher up after it was created is written through it from then on.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_AfterTheObjectPicksUpADispatcher_GoesThroughIt()
    {
        var dispatcher = new RecordingDispatcher();
        Label target;

        using (new DispatcherProviderScope(null))
        {
            target = new();
        }

        using (new DispatcherProviderScope(dispatcher))
        {
            var posted = false;

            new DispatcherViewThreadInvoker().Post(target, _ => posted = true, null);

            await Assert.That(dispatcher.DispatchCount).IsEqualTo(1);
            await Assert.That(posted).IsTrue();
        }
    }

    /// <summary>A missing callback is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_WithNoCallback_Throws() =>
        await Assert.That(static () => new DispatcherViewThreadInvoker().Post(new Label(), null!, null)).ThrowsExactly<ArgumentNullException>();

    /// <summary>Hands out one dispatcher for the duration of a test, then puts the provider back.</summary>
    private sealed class DispatcherProviderScope : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="DispatcherProviderScope"/> class.</summary>
        /// <param name="dispatcher">The dispatcher every thread is handed inside the scope, or null for none.</param>
        public DispatcherProviderScope(IDispatcher? dispatcher) =>
            _ = DispatcherProvider.SetCurrent(new StubProvider(dispatcher));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _ = DispatcherProvider.SetCurrent(null);

        /// <summary>Hands the same dispatcher to every thread that asks.</summary>
        /// <param name="dispatcher">The dispatcher to hand out, or null for none.</param>
        private sealed class StubProvider(IDispatcher? dispatcher) : IDispatcherProvider
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IDispatcher? GetForCurrentThread() => dispatcher;
        }
    }

    /// <summary>Runs each callback inline and records that it was dispatched, standing in for a UI thread.</summary>
    private sealed class RecordingDispatcher : IDispatcher
    {
        /// <summary>Gets how many callbacks were dispatched.</summary>
        public int DispatchCount { get; private set; }

        /// <inheritdoc/>
        public bool IsDispatchRequired { get; init; } = true;

        /// <inheritdoc/>
        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool Dispatch(Action action)
        {
            DispatchCount++;
            action();
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);
    }
}
