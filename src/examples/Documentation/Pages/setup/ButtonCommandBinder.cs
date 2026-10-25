// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Attaches a command to a MAUI <see cref="Button"/> and forwards the command parameter to the button.</summary>
[System.Diagnostics.DebuggerDisplay("ButtonCommandBinder: BindCount = {BindCount}")]
public sealed class ButtonCommandBinder : ICreatesCommandBinding
{
    /// <summary>The score that makes this binder win over any other binder for a button.</summary>
    private const int ButtonAffinity = 100;

    /// <summary>Gets the number of commands this binder has attached.</summary>
    public int BindCount { get; private set; }

    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) => typeof(T) == typeof(Button) ? ButtonAffinity : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class
    {
        if (command is null || target is not Button button)
        {
            return null;
        }

        BindCount++;
        button.Command = command;
        return new MultipleDisposable(
            commandParameter.Subscribe(parameter => button.CommandParameter = parameter),
            new ActionDisposable(() => button.Command = null));
    }

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class =>
        throw new NotSupportedException($"This binder attaches commands to the Command property, not to the {eventName} event ({typeof(TEventArgs).Name}).");

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs => null;
}
