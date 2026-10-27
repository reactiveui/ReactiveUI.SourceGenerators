// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace SharedScenarios.BindCommand.EventEnabled;

/// <summary>A WinForms-like button with Click event and Enabled property but no Command property.</summary>
public class WinFormsLikeButton
{
    /// <summary>Occurs when the button is clicked.</summary>
    public event EventHandler? Click;

    /// <summary>Gets or sets a value indicating whether the button is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Simulates a button click.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}
