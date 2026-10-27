// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>A control with plain events, which a command binds to by its <c>Click</c> event or a named one.</summary>
public sealed class AotButton
{
    /// <summary>Occurs when the button is clicked.</summary>
    public event EventHandler? Click;

    /// <summary>Occurs when the button is pressed.</summary>
    public event EventHandler? Pressed;

    /// <summary>Gets or sets a value indicating whether the button accepts input.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Raises <see cref="Click"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);

    /// <summary>Raises <see cref="Pressed"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformPress() => Pressed?.Invoke(this, EventArgs.Empty);
}
