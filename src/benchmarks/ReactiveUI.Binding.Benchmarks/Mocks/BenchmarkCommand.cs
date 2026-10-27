// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>A command that counts what reached it, so an invocation benchmark measures the wiring and not the body.</summary>
public sealed class BenchmarkCommand : ICommand
{
    /// <inheritdoc/>
    /// <remarks>The command is always available, so nothing raises this. A binder still subscribes to it.</remarks>
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <summary>Gets how many times the command has run.</summary>
    public int ExecutionCount { get; private set; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(object? parameter) => ExecutionCount++;
}
