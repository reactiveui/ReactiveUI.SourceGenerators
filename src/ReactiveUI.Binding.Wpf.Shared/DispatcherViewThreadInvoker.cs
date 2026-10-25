// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Threading;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Routes writes to a WPF <c>DispatcherObject</c> onto the <c>DispatcherSequencer</c> of the dispatcher that owns it.</summary>
/// <remarks>The sequencer posts at normal priority. An object with no dispatcher, such as a frozen <c>Freezable</c>, is written inline.</remarks>
public sealed class DispatcherViewThreadInvoker : SequencerViewThreadInvoker<DispatcherObject, DispatcherSequencer>
{
    /// <summary>Gets the shared instance, which generated bindings route their WPF writes through.</summary>
    public static DispatcherViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override DispatcherSequencer? SequencerFor(DispatcherObject target) =>
        target.Dispatcher is { } dispatcher ? DispatcherSequencer.For(dispatcher) : null;
}
