// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Runs a command when a <see cref="Button"/> raises <see cref="Button.Clicked"/>, or when any object raises the event a caller names.</summary>
public sealed class ClickedCommandBinder : ICreatesCommandBinding
{
    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) =>
        typeof(Button).IsAssignableFrom(typeof(T)) ? BindingAffinity.ExactType : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class =>
        target is Button button
            ? BindCommandToObject<Button, EventArgs>(
                command,
                button,
                commandParameter,
                handler => button.Clicked += new EventHandler(handler),
                handler => button.Clicked -= new EventHandler(handler))
            : null;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class
    {
        Console.WriteLine($"The binder is asked for the {eventName} event, which carries {typeof(TEventArgs).Name}");

        return eventName == nameof(Button.Clicked) ? BindCommandToObject(command, target, commandParameter) : null;
    }

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs
    {
        if (command is null)
        {
            return null;
        }

        BehaviorSignal<object?> parameter = new(null);

        return new MultipleDisposable(
            commandParameter.Subscribe(parameter.OnNext),
            Signal.FromEventPattern(addHandler, removeHandler).Subscribe(_ => command.Execute(parameter.Value)),
            parameter);
    }
}
