// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Threading;
using Microsoft.Maui.Controls;
using PlatformBindingsVerification.Common;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Maui.Builder;

namespace PlatformBindingsVerification.Maui;

/// <summary>
/// Feeds a real MAUI <see cref="Label"/> from an <see cref="ObservableAsPropertyHelper{T}"/>. Outside a running
/// MAUI application <see cref="BindableObject.Dispatcher"/> throws <see cref="InvalidOperationException"/>; the
/// MAUI view thread invoker catches that and treats the label as having no owning thread, so the write lands
/// inline - which is what this verification actually exercises, and is why it can run without a display.
/// </summary>
public static class MauiVerification
{
    /// <summary>The first count pushed, from the constructing thread.</summary>
    private const int FirstCount = 1;

    /// <summary>The second count pushed, from a background thread.</summary>
    private const int SecondCount = 2;

    /// <summary>Builds the view model and control, pushes a value from a background thread, and confirms the label picks it up inline (no MAUI application is running to own a dispatcher).</summary>
    /// <returns><see langword="true"/> when the label shows the value pushed from the background thread.</returns>
    public static bool Verify()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithMaui().BuildApp();
        ViewThreadInvokers.Refresh();

        PushSource<int> counts = new();
        MauiCounterViewModel viewModel = new(counts);
        Label label = new();

        using (viewModel.BindOneWay(label, static x => x.Text, static v => v.Text))
        {
            counts.OnNext(FirstCount);

            if (label.Text != FirstCount.ToString(CultureInfo.InvariantCulture))
            {
                return false;
            }

            Thread backgroundThread = new(() => counts.OnNext(SecondCount));
            backgroundThread.Start();
            backgroundThread.Join();

            return label.Text == SecondCount.ToString(CultureInfo.InvariantCulture);
        }
    }
}
