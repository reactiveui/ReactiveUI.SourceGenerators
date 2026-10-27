// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINDOWS
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace ReactiveUI.Binding.Uno.Tests;

/// <summary>Tests for the Uno view thread invoker against real WinUI controls on a dedicated XAML thread.</summary>
[NotInParallel]
public class UnoViewThreadInvokerTests
{
    /// <summary>A control is claimed, its owning thread may write to it, and another thread may not.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task CheckAccess_FollowsTheControlsDispatcherQueue()
    {
        var controller = DispatcherQueueController.CreateOnDedicatedThread();
        try
        {
            var (border, claimed, onOwningThread) = await RunOnQueue(controller.DispatcherQueue, static () =>
            {
                _ = WindowsXamlManager.InitializeForCurrentThread();
                Border border = new();
                return (border, UnoViewThreadInvoker.Instance.Claims(border), UnoViewThreadInvoker.Instance.CheckAccess(border));
            });

            await Assert.That(claimed).IsTrue();
            await Assert.That(onOwningThread).IsTrue();
            await Assert.That(UnoViewThreadInvoker.Instance.CheckAccess(border)).IsFalse();
        }
        finally
        {
            await controller.ShutdownQueueAsync();
        }
    }

    /// <summary>Runs <paramref name="work"/> on <paramref name="queue"/>'s thread and returns its result.</summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="queue">The queue to run on.</param>
    /// <param name="work">The work to run.</param>
    /// <returns>The work's result.</returns>
    private static Task<T> RunOnQueue<T>(DispatcherQueue queue, Func<T> work)
    {
        TaskCompletionSource<T> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = queue.TryEnqueue(() =>
        {
            try
            {
                completion.SetResult(work());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }
}
#endif
