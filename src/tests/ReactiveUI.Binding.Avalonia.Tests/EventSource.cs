// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>An object with a CLR event the test raises.</summary>
public sealed class EventSource
{
    /// <summary>Raised by <see cref="Raise"/>.</summary>
    public event EventHandler<EventArgs>? Fired;

    /// <summary>Gets a value indicating whether any handler is attached.</summary>
    public bool HasHandlers => Fired is not null;

    /// <summary>Raises <see cref="Fired"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Raise() => Fired?.Invoke(this, EventArgs.Empty);
}
