// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>Tests for the WinForms view-thread invoker.</summary>
public class ControlViewThreadInvokerTests
{
    /// <summary>How long a callback posted to a control's thread is given to arrive.</summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    /// <summary>A target the invoker does not recognise.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>The shared instance generated bindings route through is one invoker, reused.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Instance_IsSharedAndClaimsAControl()
    {
        using var control = new Control();

        await Assert.That(ControlViewThreadInvoker.Instance).IsSameReferenceAs(ControlViewThreadInvoker.Instance);
        await Assert.That(ControlViewThreadInvoker.Instance.Claims(control)).IsTrue();
    }

    /// <summary>A control is claimed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Claims_WithAControl_ClaimsIt()
    {
        using var control = new Control();

        await Assert.That(new ControlViewThreadInvoker().Claims(control)).IsTrue();
    }

    /// <summary>Anything that is not a control is left to another invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Claims_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new ControlViewThreadInvoker().Claims(UnclaimedTarget)).IsFalse();

    /// <summary>A control with no handle belongs to no thread yet, and asking does not create the handle.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_ForAControlWithNoHandle_IsTrue()
    {
        using var control = new Control();

        await Assert.That(new ControlViewThreadInvoker().CheckAccess(control)).IsTrue();
        await Assert.That(control.IsHandleCreated).IsFalse();
    }

    /// <summary>The thread that created a control's handle may write to it directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_ForAControlOwnedByTheCallingThread_IsTrue()
    {
        using var control = new Control();

        // Reading the handle creates it on this thread, which is what gives the control its owner.
        _ = control.Handle;

        await Assert.That(new ControlViewThreadInvoker().CheckAccess(control)).IsTrue();
    }

    /// <summary>A control owned by another thread may not be written from this one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CheckAccess_ForAControlOwnedByAnotherThread_IsFalse()
    {
        using var owner = new ControlThread();

        await Assert.That(new ControlViewThreadInvoker().CheckAccess(owner.Control)).IsFalse();
    }

    /// <summary>A callback posted to a control with no handle runs inline with its state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAControlWithNoHandle_RunsInline()
    {
        var postingThreadId = Environment.CurrentManagedThreadId;
        using var control = new Control();
        var state = new object();
        var ranOnThreadId = 0;
        object? received = null;

        new ControlViewThreadInvoker().Post(
            control,
            s =>
            {
                ranOnThreadId = Environment.CurrentManagedThreadId;
                received = s;
            },
            state);

        await Assert.That(ranOnThreadId).IsEqualTo(postingThreadId);
        await Assert.That(received).IsSameReferenceAs(state);
        await Assert.That(control.IsHandleCreated).IsFalse();
    }

    /// <summary>A posted callback runs on the thread that owns the control, with no state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAControlOwnedByAnotherThread_RunsOnThatThread()
    {
        var postingThreadId = Environment.CurrentManagedThreadId;

        using var owner = new ControlThread();
        using var arrived = new ManualResetEventSlim();
        var ranOnThreadId = 0;

        new ControlViewThreadInvoker().Post(
            owner.Control,
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

    /// <summary>A control bound before its handle exists is written on the thread that later creates the handle.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_AfterTheHandleIsCreatedOnAnotherThread_RunsOnThatThread()
    {
        using var owner = new ControlThread();
        var control = new Control();
        var invoker = new ControlViewThreadInvoker();
        using var created = new ManualResetEventSlim();

        // Reading the handle on the owning thread creates it there, which is what gives the control its owner.
        _ = owner.Control.BeginInvoke(() =>
        {
            _ = control.Handle;
            created.Set();
        });

        await Assert.That(created.Wait(Patience)).IsTrue();

        using var arrived = new ManualResetEventSlim();
        var ranOnThreadId = 0;

        invoker.Post(
            control,
            _ =>
            {
                ranOnThreadId = Environment.CurrentManagedThreadId;
                arrived.Set();
            },
            null);

        await Assert.That(arrived.Wait(Patience)).IsTrue();
        await Assert.That(ranOnThreadId).IsEqualTo(owner.ThreadId);

        _ = owner.Control.BeginInvoke(control.Dispose);
    }

    /// <summary>A missing callback is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_WithNoCallback_Throws()
    {
        using var control = new Control();

        await Assert.That(() => new ControlViewThreadInvoker().Post(control, null!, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A control owned by a thread that pumps messages, as a WinForms application's controls are.</summary>
    private sealed class ControlThread : IDisposable
    {
        /// <summary>Keeps the message loop running until the thread is told to stop.</summary>
        private readonly ApplicationContext _context = new();

        /// <summary>Set once the message loop is running.</summary>
        private readonly ManualResetEventSlim _pumping = new();

        /// <summary>The control created on this thread, so this thread owns it.</summary>
        private Control? _control;

        /// <summary>Initializes a new instance of the <see cref="ControlThread"/> class.</summary>
        public ControlThread()
        {
            var thread = new Thread(Run) { IsBackground = true, Name = nameof(ControlThread) };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // Posts go through the message loop, so wait for it to run, not just for the handle.
            _ = _pumping.Wait(Patience);

            ThreadId = thread.ManagedThreadId;
        }

        /// <summary>Gets the control this thread owns.</summary>
        public Control Control => _control!;

        /// <summary>Gets the managed id of the thread that owns the control.</summary>
        public int ThreadId { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.ExitThread();
            _context.Dispose();
            _pumping.Dispose();
        }

        /// <summary>Creates the control and pumps its messages until the thread is told to stop.</summary>
        private void Run()
        {
            _control = new();

            // Reading the handle creates it on this thread, which is what gives the control its owner.
            _ = _control.Handle;
            Application.Idle += SignalPumping;
            Application.Run(_context);
        }

        /// <summary>Reports that the message loop is running.</summary>
        /// <param name="sender">The application raising the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SignalPumping(object? sender, EventArgs e)
        {
            Application.Idle -= SignalPumping;
            _pumping.Set();
        }
    }
}
