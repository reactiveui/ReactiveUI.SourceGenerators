// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>Tests for the WPF view-thread invoker.</summary>
public class DispatcherViewThreadInvokerTests
{
    /// <summary>How long a callback posted to another UI thread is given to arrive.</summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    /// <summary>A target the invoker does not recognise.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>The shared instance generated bindings route through is one invoker, reused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Instance_IsSharedAndClaimsADispatcherObject()
    {
        await Assert.That(DispatcherViewThreadInvoker.Instance).IsSameReferenceAs(DispatcherViewThreadInvoker.Instance);
        await Assert.That(DispatcherViewThreadInvoker.Instance.Claims(new Fixture())).IsTrue();
    }

    /// <summary>A dispatcher object is claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Claims_WithADispatcherObject_ClaimsIt() =>
        await Assert.That(new DispatcherViewThreadInvoker().Claims(new Fixture())).IsTrue();

    /// <summary>Anything that is not a dispatcher object is left to another invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Claims_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new DispatcherViewThreadInvoker().Claims(UnclaimedTarget)).IsFalse();

    /// <summary>The thread that created an object may write to it directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_OnTheThreadThatCreatedTheObject_IsTrue() =>
        await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(new Fixture())).IsTrue();

    /// <summary>An object owned by a second UI thread may not be written from this one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_ForAnObjectOwnedByASecondUiThread_IsFalse()
    {
        using var owner = new UiThread();
        var target = owner.Invoke(static () => new Fixture());

        await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(target)).IsFalse();
    }

    /// <summary>A frozen object belongs to no thread, so any thread may write to it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_ForAFrozenObjectFromAnotherUiThread_IsTrue()
    {
        using var owner = new UiThread();
        var frozen = owner.Invoke(static () => CreateFrozen());

        await Assert.That(new DispatcherViewThreadInvoker().CheckAccess(frozen)).IsTrue();
    }

    /// <summary>A posted callback runs on the UI thread that owns the object.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAnObjectOwnedByASecondUiThread_RunsOnThatThread()
    {
        var postingThreadId = Environment.CurrentManagedThreadId;

        using var owner = new UiThread();
        var target = owner.Invoke(static () => new Fixture());
        using var arrived = new ManualResetEventSlim();
        var ranOnThreadId = 0;

        new DispatcherViewThreadInvoker().Post(
            target,
            _ =>
            {
                ranOnThreadId = Environment.CurrentManagedThreadId;
                arrived.Set();
            },
            null);

        await Assert.That(arrived.Wait(Patience)).IsTrue();
        await Assert.That(ranOnThreadId).IsEqualTo(owner.ThreadId);
        await Assert.That(ranOnThreadId).IsNotEqualTo(postingThreadId);
    }

    /// <summary>A callback posted to a frozen object runs inline with its state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAFrozenObject_RunsInline()
    {
        var postingThreadId = Environment.CurrentManagedThreadId;
        var state = new object();
        var ranOnThreadId = 0;
        object? received = null;

        new DispatcherViewThreadInvoker().Post(
            CreateFrozen(),
            s =>
            {
                ranOnThreadId = Environment.CurrentManagedThreadId;
                received = s;
            },
            state);

        await Assert.That(ranOnThreadId).IsEqualTo(postingThreadId);
        await Assert.That(received).IsSameReferenceAs(state);
    }

    /// <summary>A missing callback is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_WithNoCallback_Throws() =>
        await Assert.That(static () => new DispatcherViewThreadInvoker().Post(new Fixture(), null!, null)).ThrowsExactly<ArgumentNullException>();

    /// <summary>Creates a frozen freezable, which gives up its dispatcher.</summary>
    /// <returns>The frozen object.</returns>
    private static FreezableFixture CreateFrozen()
    {
        var frozen = new FreezableFixture();
        frozen.Freeze();
        return frozen;
    }

    /// <summary>A UI thread of its own, so an object can be owned by something other than the test's thread.</summary>
    private sealed class UiThread : IDisposable
    {
        /// <summary>Set once the dispatcher is running.</summary>
        private readonly ManualResetEventSlim _running = new();

        /// <summary>The dispatcher that owns everything created through <see cref="Invoke"/>.</summary>
        private Dispatcher? _dispatcher;

        /// <summary>Initializes a new instance of the <see cref="UiThread"/> class.</summary>
        public UiThread()
        {
            var thread = new Thread(Run) { IsBackground = true, Name = nameof(UiThread) };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _ = _running.Wait(Patience);

            ThreadId = thread.ManagedThreadId;
        }

        /// <summary>Gets the managed id of the thread this owns.</summary>
        public int ThreadId { get; }

        /// <summary>Creates something on this thread, so this thread owns it.</summary>
        /// <typeparam name="T">The type of the created object.</typeparam>
        /// <param name="create">Creates the object.</param>
        /// <returns>The created object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>(Func<T> create) => _dispatcher!.Invoke(create);

        /// <inheritdoc/>
        public void Dispose()
        {
            _dispatcher!.InvokeShutdown();
            _running.Dispose();
        }

        /// <summary>Runs the dispatcher this owns until it is shut down.</summary>
        private void Run()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _running.Set();
            Dispatcher.Run();
        }
    }

    /// <summary>A dependency object with no properties, standing in for a view.</summary>
    private sealed class Fixture : DependencyObject;

    /// <summary>A freezable with no properties, which gives up its dispatcher once frozen.</summary>
    private sealed class FreezableFixture : Freezable
    {
        /// <inheritdoc/>
        protected override Freezable CreateInstanceCore() => new FreezableFixture();
    }
}
