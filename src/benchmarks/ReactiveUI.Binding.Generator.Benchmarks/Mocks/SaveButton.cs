// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Generator.Benchmarks.Mocks;

/// <summary>A button the mock command binding attaches to through its click event.</summary>
public class SaveButton
{
    /// <summary>Occurs when the button is clicked.</summary>
    public event EventHandler? Click;

    /// <summary>Raises <see cref="Click"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}
