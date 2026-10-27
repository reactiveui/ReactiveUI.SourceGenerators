// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Android.Views;

/// <summary>Provides the Android click contract for measuring generated command wiring.</summary>
public class View
{
    /// <summary>Occurs when the benchmark requests a click.</summary>
    public event EventHandler? Click;

    /// <summary>Gets or sets the command's enabled state.</summary>
    public bool Enabled { get; set; }

    /// <summary>Raises the native event without allocating event arguments.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseClick() => Click?.Invoke(this, EventArgs.Empty);
}
