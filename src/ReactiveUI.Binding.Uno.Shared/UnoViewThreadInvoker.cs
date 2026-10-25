// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Uno;
#else
namespace ReactiveUI.Binding.Uno;
#endif

/// <summary>Routes writes to an Uno Platform <c>DependencyObject</c> onto the <c>DispatcherQueueSequencer</c> of the queue that owns it.</summary>
/// <remarks>Every dependency object belongs to the dispatcher queue of the thread that created it.</remarks>
public sealed class UnoViewThreadInvoker : SequencerViewThreadInvoker<DependencyObject, DispatcherQueueSequencer>
{
    /// <summary>Gets the shared instance, which generated bindings route their Uno writes through.</summary>
    public static UnoViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override DispatcherQueueSequencer SequencerFor(DependencyObject target) =>
        DispatcherQueueSequencer.For(target.DispatcherQueue);
}
