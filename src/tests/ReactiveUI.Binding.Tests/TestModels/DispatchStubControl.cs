// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A control a command binding names, carrying a property to bind against and the default event.</summary>
public class DispatchStubControl
{
    /// <summary>Raised to execute a bound command. This is the event the default command binder looks for.</summary>
    public event EventHandler? Click;

    /// <summary>Gets or sets the control's text.</summary>
    public string Text { get; set; } = "a";

    /// <summary>Raises <see cref="Click"/>, executing whichever command is bound to it.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}
