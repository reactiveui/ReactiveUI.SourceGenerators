// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Routes writes to a MAUI <c>BindableObject</c> onto the <c>MauiDispatcherSequencer</c> of the dispatcher it carries.</summary>
/// <remarks>An object with no dispatcher, as in a view's unit test, is written inline.</remarks>
public sealed class DispatcherViewThreadInvoker : SequencerViewThreadInvoker<BindableObject, MauiDispatcherSequencer>
{
    /// <summary>Gets the shared instance, which generated bindings route their MAUI writes through.</summary>
    public static DispatcherViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    protected override MauiDispatcherSequencer? SequencerFor(BindableObject target) =>
        FindDispatcher(target) is { } dispatcher ? MauiDispatcherSequencer.For(dispatcher) : null;

    /// <summary>Reads the dispatcher an object carries.</summary>
    /// <param name="owner">The object to read.</param>
    /// <returns>The dispatcher, or null when neither the object, its thread nor the application has one.</returns>
    private static IDispatcher? FindDispatcher(BindableObject owner)
    {
        try
        {
            return owner.Dispatcher;
        }
        catch (InvalidOperationException)
        {
            // MAUI throws rather than answering null when it finds no dispatcher, as in a view's unit test.
            return null;
        }
    }
}
