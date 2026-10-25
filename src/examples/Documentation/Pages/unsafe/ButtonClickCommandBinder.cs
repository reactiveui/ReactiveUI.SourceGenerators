// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Runs a command each time a <see cref="Button"/> is clicked. The runtime path finds a control's binder here.</summary>
public sealed class ButtonClickCommandBinder : ICreatesCommandBinding
{
    /// <summary>The score that makes this binder win over any other binder for a button.</summary>
    private const int ButtonAffinity = 100;

    /// <summary>The name of the only event this binder understands.</summary>
    private const string ClickedEventName = nameof(Button.Clicked);

    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) => typeof(T) == typeof(Button) ? ButtonAffinity : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class => Attach(command, target as Button, commandParameter);

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class =>
        eventName == ClickedEventName
            ? Attach(command, target as Button, commandParameter)
            : throw new NotSupportedException($"A button has no {eventName} event ({typeof(TEventArgs).Name}).");

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs => null;

    /// <summary>Runs the command with the latest parameter each time the button is clicked.</summary>
    /// <param name="command">The command to run.</param>
    /// <param name="button">The button to follow.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <returns>A disposable that stops following the button, or <see langword="null"/> when there is nothing to attach.</returns>
    private static MultipleDisposable? Attach(ICommand? command, Button? button, IObservable<object?> commandParameter)
    {
        if (command is null || button is null)
        {
            return null;
        }

        BehaviorSignal<object?> parameter = new(null);
        IObservable<Primitives.Core.EventPattern<EventArgs>> clicks = Signal.FromEventPattern(handler => button.Clicked += handler, handler => button.Clicked -= handler);

        return new(
            commandParameter.Subscribe(parameter.OnNext),
            clicks.Where(_ => command.CanExecute(parameter.Value)).Subscribe(_ => command.Execute(parameter.Value)),
            parameter);
    }
}
