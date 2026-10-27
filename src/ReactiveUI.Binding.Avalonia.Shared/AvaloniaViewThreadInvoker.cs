// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia;
#else
namespace ReactiveUI.Binding.Avalonia;
#endif

/// <summary>Routes writes to an <c>AvaloniaObject</c> onto the <c>AvaloniaScheduler</c> of the dispatcher that owns it.</summary>
public sealed class AvaloniaViewThreadInvoker : SequencerViewThreadInvoker<AvaloniaObject, AvaloniaScheduler>
{
    /// <summary>Gets the shared instance, which generated bindings route their Avalonia writes through.</summary>
    public static AvaloniaViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override AvaloniaScheduler SequencerFor(AvaloniaObject target) => AvaloniaScheduler.For(target.Dispatcher);
}
