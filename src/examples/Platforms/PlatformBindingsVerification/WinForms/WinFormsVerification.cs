// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using PlatformBindingsVerification.Common;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.WinForms.Builder;

namespace PlatformBindingsVerification.WinForms;

/// <summary>
/// Feeds a real Windows Forms <see cref="Label"/> from a <see cref="ObservableAsPropertyHelper{T}"/> whose source
/// pushes values from a background thread, mirroring <c>PlatformBindingsVerification.Wpf.WpfVerification</c> for
/// the <see cref="Control"/>-based invoker instead of the dispatcher-based one.
/// </summary>
public static class WinFormsVerification
{
    /// <summary>The first count pushed, from the constructing thread.</summary>
    private const int FirstCount = 1;

    /// <summary>The second count pushed, from a background thread.</summary>
    private const int SecondCount = 2;

    /// <summary>How long to pump the message loop while waiting for a background write to land.</summary>
    private static readonly TimeSpan PumpTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Builds the view model and control on the current (UI) thread, then pushes a value from a background thread and
    /// confirms the control picks it up through the control's own invoker.
    /// </summary>
    /// <returns><see langword="true"/> when the control shows the value pushed from the background thread.</returns>
    public static bool Verify()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithWinForms().BuildApp();
        ViewThreadInvokers.Refresh();

        PushSource<int> counts = new();
        WinFormsCounterViewModel viewModel = new(counts);
        using Label label = new();

        // Forces handle creation, so InvokeRequired reflects a real owning thread rather than routing inline.
        _ = label.Handle;

        using (viewModel.BindOneWay(label, static x => x.Text, static v => v.Text))
        {
            counts.OnNext(FirstCount);
            PumpMessageLoop(() => label.Text == FirstCount.ToString(CultureInfo.InvariantCulture));

            if (label.Text != FirstCount.ToString(CultureInfo.InvariantCulture))
            {
                return false;
            }

            var backgroundThread = new Thread(() => counts.OnNext(SecondCount));
            backgroundThread.Start();
            backgroundThread.Join();
            PumpMessageLoop(() => label.Text == SecondCount.ToString(CultureInfo.InvariantCulture));

            return label.Text == SecondCount.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Drains pending Windows messages until <paramref name="isDone"/> is <see langword="true"/> or the timeout elapses.</summary>
    /// <param name="isDone">Reports whether the awaited write has landed.</param>
    private static void PumpMessageLoop(Func<bool> isDone)
    {
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        while (!isDone() && System.Diagnostics.Stopwatch.GetElapsedTime(started) < PumpTimeout)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
    }
}
