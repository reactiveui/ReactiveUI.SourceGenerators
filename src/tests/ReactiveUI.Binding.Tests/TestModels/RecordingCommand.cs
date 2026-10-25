// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A command that records what it was offered and whether it accepted it.</summary>
public sealed class RecordingCommand : ICommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add { /* CanExecute is asked per emission rather than tracked from the event. */ }
        remove { /* CanExecute is asked per emission rather than tracked from the event. */ }
    }

    /// <summary>Gets or sets a value indicating whether this command accepts what it is offered.</summary>
    public bool CanExecuteResult { get; set; } = true;

    /// <summary>Gets the number of times this command was executed.</summary>
    public int ExecuteCount { get; private set; }

    /// <summary>Gets the parameter of the most recent execution.</summary>
    public object? LastParameter { get; private set; }

    /// <summary>Gets the parameter of the most recent <see cref="CanExecute"/> question.</summary>
    public object? LastQuestionedParameter { get; private set; }

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        LastQuestionedParameter = parameter;
        return CanExecuteResult;
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        ExecuteCount++;
        LastParameter = parameter;
    }
}
