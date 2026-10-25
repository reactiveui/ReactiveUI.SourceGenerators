// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A command that records the parameter it was executed with.</summary>
public class RecordingStubCommand : ICommand
{
    /// <inheritdoc/>
    /// <remarks>The command is always available, so nothing raises this. A binder still subscribes to it.</remarks>
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <summary>Gets the parameter the most recent execution carried.</summary>
    public object? LastParameter { get; private set; }

    /// <summary>Gets how many times the command has executed.</summary>
    public int ExecuteCount { get; private set; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(object? parameter)
    {
        ExecuteCount++;
        LastParameter = parameter;
    }
}
