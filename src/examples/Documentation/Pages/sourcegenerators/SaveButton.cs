// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>A button a command can be bound to through its <see cref="Click"/> event.</summary>
[System.Diagnostics.DebuggerDisplay("SaveButton")]
public sealed class SaveButton
{
    /// <summary>Occurs when the button is pressed.</summary>
    public event EventHandler? Click;

    /// <summary>Presses the button.</summary>
    public void Press() => Click?.Invoke(this, EventArgs.Empty);
}
