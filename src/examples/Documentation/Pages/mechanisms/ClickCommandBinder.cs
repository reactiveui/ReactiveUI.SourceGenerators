// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Core;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Runs a command when a <see cref="Button"/> raises <see cref="Button.Clicked"/>. The binder passes the latest
/// command parameter to the command, enables the button only while the command can run, and never touches the
/// button's own <see cref="Button.Command"/> property. It prints a line each time it attaches to a button.
/// </summary>
public sealed class ClickCommandBinder : ICreatesCommandBinding
{
    /// <inheritdoc/>
    public int GetAffinityForObject<T>(bool hasEventTarget) =>
        typeof(Button).IsAssignableFrom(typeof(T)) ? BindingAffinity.ExactType : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class =>
        command is null || target is not Button button
            ? null
            : AttachToClicks(command, button, commandParameter, ObserveClicks(button));

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(ICommand? command, T? target, IObservable<object?> commandParameter, string eventName)
        where T : class
    {
        if (eventName != nameof(Button.Clicked))
        {
            return null;
        }

        Console.WriteLine($"The click binder is asked for the {eventName} event, which carries {typeof(TEventArgs).Name}");

        return command is null || target is not Button button
            ? null
            : AttachToClicks(command, button, commandParameter, ObserveClicks(button));
    }

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs =>
        command is null || target is not Button button
            ? null
            : AttachToClicks(command, button, commandParameter, Signal.FromEventPattern(addHandler, removeHandler));

    /// <summary>Observes the clicks of a button.</summary>
    /// <param name="button">The button to observe.</param>
    /// <returns>A stream with one item for each click.</returns>
    private static IObservable<EventPattern<EventArgs>> ObserveClicks(Button button) =>
        Signal.FromEventPattern(handler => button.Clicked += handler, handler => button.Clicked -= handler);

    /// <summary>Runs a command on each click and keeps the button enabled only while the command can run.</summary>
    /// <typeparam name="TEventArgs">The type of the click event data.</typeparam>
    /// <param name="command">The command to run.</param>
    /// <param name="button">The button whose enabled state follows the command.</param>
    /// <param name="commandParameter">The stream of parameters the command receives.</param>
    /// <param name="clicks">The clicks that run the command.</param>
    /// <returns>A subscription that detaches the command and restores the button when disposed.</returns>
    private static MultipleDisposable AttachToClicks<TEventArgs>(
        ICommand command,
        Button button,
        IObservable<object?> commandParameter,
        IObservable<EventPattern<TEventArgs>> clicks)
        where TEventArgs : EventArgs
    {
        Console.WriteLine($"The click binder is attached to the {button.Text} button");

        bool wasEnabled = button.IsEnabled;
        BehaviorSignal<object?> parameter = new(null);
        IObservable<EventPattern<EventArgs>> canExecuteChanged = Signal.FromEventPattern(handler => command.CanExecuteChanged += handler, handler => command.CanExecuteChanged -= handler);

        return new(
            commandParameter.Subscribe(parameter.OnNext),
            clicks.Where(_ => command.CanExecute(parameter.Value)).Subscribe(_ => command.Execute(parameter.Value)),
            parameter.Merge(canExecuteChanged.Select(_ => parameter.Value)).Select(command.CanExecute).Subscribe(canRun => button.IsEnabled = canRun),
            new ActionDisposable(() => button.IsEnabled = wasEnabled),
            parameter);
    }
}
