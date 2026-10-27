// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>A command that records each parameter it runs with and executes only when <see cref="Enabled"/> is set.</summary>
public sealed class RecordingCommand : ICommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <summary>Gets or sets a value indicating whether the command can execute.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets the parameters the command ran with, in order.</summary>
    public List<object?> Executed { get; } = [];

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter) => Enabled;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(object? parameter) => Executed.Add(parameter);
}
