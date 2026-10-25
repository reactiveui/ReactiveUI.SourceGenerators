// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Forms;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Routes writes to a WinForms <c>Control</c> onto the <c>ControlSequencer</c> of the thread that created its handle.</summary>
/// <remarks>A control that needs no invoke, including one with no handle yet, is written inline.</remarks>
public sealed class ControlViewThreadInvoker : SequencerViewThreadInvoker<Control, ControlSequencer>
{
    /// <summary>Gets the shared instance, which generated bindings route their WinForms writes through.</summary>
    public static ControlViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override ControlSequencer? SequencerFor(Control target) =>
        // InvokeRequired is false until the control or a parent has a handle, and BeginInvoke needs one.
        target.InvokeRequired ? ControlSequencer.For(target) : null;
}
