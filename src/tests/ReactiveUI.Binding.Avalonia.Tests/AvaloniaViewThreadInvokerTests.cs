// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>Tests for the Avalonia view thread invoker, compiled once against each runtime flavour.</summary>
[NotInParallel]
public class AvaloniaViewThreadInvokerTests
{
    /// <summary>How long a test waits for the owning thread to run a posted callback.</summary>
    private static readonly TimeSpan PostTimeout = TimeSpan.FromSeconds(10);

    /// <summary>The shared instance is one invoker, reused.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Instance_IsShared() =>
        await Assert.That(AvaloniaViewThreadInvoker.Instance).IsSameReferenceAs(AvaloniaViewThreadInvoker.Instance);

    /// <summary>An Avalonia object is claimed.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Claims_WithAnAvaloniaObject_ClaimsIt() =>
        AvaloniaTestSession.Run(static async () => await Assert.That(AvaloniaViewThreadInvoker.Instance.Claims(new TestObject())).IsTrue());

    /// <summary>Anything that is not an Avalonia object is left to another invoker.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Claims_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(AvaloniaViewThreadInvoker.Instance.Claims(new())).IsFalse();

    /// <summary>The thread that owns the object's dispatcher may write to it.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task CheckAccess_OnTheOwningThread_IsTrue() =>
        AvaloniaTestSession.Run(static async () => await Assert.That(AvaloniaViewThreadInvoker.Instance.CheckAccess(new TestObject())).IsTrue());

    /// <summary>Any other thread may not.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CheckAccess_OnAnotherThread_IsFalse()
    {
        var target = await CreateOnOwningThread();

        var access = await Task.Run(() => AvaloniaViewThreadInvoker.Instance.CheckAccess(target));

        await Assert.That(access).IsFalse();
    }

    /// <summary>A callback posted from another thread runs on the object's owning thread with its state.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>The body waits on the owning thread, which keeps that thread's dispatcher running as an app's would.</remarks>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task Post_FromAnotherThread_RunsTheCallbackOnTheOwningThread() =>
        AvaloniaTestSession.Run(static async () =>
        {
            TestObject target = new();
            TaskCompletionSource<(object? State, bool OnOwningThread)> ran = new(TaskCreationOptions.RunContinuationsAsynchronously);
            object state = new();

            await Task.Run(() => AvaloniaViewThreadInvoker.Instance.Post(target, s => ran.SetResult((s, target.CheckAccess())), state));
            var (received, onOwningThread) = await ran.Task.WaitAsync(PostTimeout);

            await Assert.That(received).IsSameReferenceAs(state);
            await Assert.That(onOwningThread).IsTrue();
        });

    /// <summary>A null callback is rejected.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_WithANullCallback_Throws()
    {
        var target = await CreateOnOwningThread();

        await Assert.That(() => AvaloniaViewThreadInvoker.Instance.Post(target, null!, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Creates a test object on the session's UI thread, so that thread owns it.</summary>
    /// <returns>The object.</returns>
    private static async Task<TestObject> CreateOnOwningThread()
    {
        TestObject? target = null;
        await AvaloniaTestSession.Run(() =>
        {
            target = new();
            return Task.CompletedTask;
        });

        return target!;
    }
}
