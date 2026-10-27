// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>A command that counts its executions, so a validation scenario can assert it ran.</summary>
public sealed class AotCommand : ICommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add { /* CanExecute is asked per emission rather than tracked from the event. */ }
        remove { /* CanExecute is asked per emission rather than tracked from the event. */ }
    }

    /// <summary>Gets the number of times this command was executed.</summary>
    public int ExecuteCount { get; private set; }

    /// <summary>Gets the parameter of the most recent execution.</summary>
    public object? LastParameter { get; private set; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        ExecuteCount++;
        LastParameter = parameter;
    }
}
