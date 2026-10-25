// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Exposes the command property pattern recognized by the binding generator.</summary>
public sealed class BoundCommandControl
{
    /// <summary>Occurs when the control is pressed.</summary>
    public event EventHandler? Click;

    /// <summary>Gets or sets the command installed by the binding.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Gets or sets the argument supplied when the control executes its command.</summary>
    public object? CommandParameter { get; set; }

    /// <summary>Raises the default event and executes the installed command when available.</summary>
    public void Press()
    {
        Click?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }
}
