// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using PlatformBindingsVerification.Common;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Wpf.Builder;

namespace PlatformBindingsVerification.Wpf;

/// <summary>
/// Feeds a real WPF <see cref="TextBlock"/> from a <see cref="ObservableAsPropertyHelper{T}"/> whose source pushes
/// values from a background thread. The view model's own <c>PropertyChanged</c> fires on whichever thread pushed
/// the value; the generated <c>OneWayBind</c> is what carries the write onto the dispatcher that owns the
/// <see cref="TextBlock"/>, per "Which thread a binding writes on" in the project README.
/// </summary>
public static class WpfVerification
{
    /// <summary>The first count pushed, from the constructing thread.</summary>
    private const int FirstCount = 1;

    /// <summary>The second count pushed, from a background thread.</summary>
    private const int SecondCount = 2;

    /// <summary>Builds the view model and control on the current (UI) thread, then pushes a value from a background thread and confirms the control picks it up on the dispatcher.</summary>
    /// <returns><see langword="true"/> when the control shows the value pushed from the background thread.</returns>
    public static bool Verify()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithWpf().BuildApp();
        ViewThreadInvokers.Refresh();

        PushSource<int> counts = new();
        WpfCounterViewModel viewModel = new(counts);
        TextBlock textBlock = new();

        using (viewModel.BindOneWay(textBlock, static x => x.Text, static v => v.Text))
        {
            counts.OnNext(FirstCount);
            PumpDispatcher();

            if (textBlock.Text != FirstCount.ToString(CultureInfo.InvariantCulture))
            {
                return false;
            }

            var backgroundThread = new Thread(() => counts.OnNext(SecondCount));
            backgroundThread.Start();
            backgroundThread.Join();
            PumpDispatcher();

            return textBlock.Text == SecondCount.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Drains the current dispatcher's queue so a posted write lands before the caller reads the control.</summary>
    private static void PumpDispatcher()
    {
        DispatcherFrame frame = new();
        _ = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
}
