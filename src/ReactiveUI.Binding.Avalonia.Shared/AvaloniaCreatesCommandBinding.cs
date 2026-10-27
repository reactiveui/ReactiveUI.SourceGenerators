// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia;
#else
namespace ReactiveUI.Binding.Avalonia;
#endif

/// <summary>Binds commands to Avalonia input elements, through a command source's <c>Command</c> property or through a routed event.</summary>
/// <remarks>
/// A control that implements <c>ICommandSource</c>, such as a button or a menu item, is bound through its
/// <c>Command</c> and <c>CommandParameter</c> properties. Any other input element is bound through a routed event
/// that executes the command and keeps <c>IsEnabled</c> in step with <c>CanExecute</c>.
/// </remarks>
public sealed class AvaloniaCreatesCommandBinding : ICreatesCommandBinding
{
    /// <summary>The affinity of a binding through a named routed event.</summary>
    private const int EventTargetAffinity = 6;

    /// <summary>The affinity of a binding through an <c>ICommandSource</c>'s <c>Command</c> property.</summary>
    private const int CommandSourceAffinity = 10;

    /// <summary>Returns how well this binder handles controls of type <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <param name="hasEventTarget"><see langword="true"/> when the binding names an event.</param>
    /// <returns>
    /// Zero for a type that is not an <c>InputElement</c>; 6 for an input element bound through an event; 10 for an
    /// input element that implements <c>ICommandSource</c> and is bound without an event; otherwise zero.
    /// </returns>
    public int GetAffinityForObject<T>(bool hasEventTarget)
    {
        if (!typeof(InputElement).IsAssignableFrom(typeof(T)))
        {
            return 0;
        }

        if (hasEventTarget)
        {
            return EventTargetAffinity;
        }

        return typeof(ICommandSource).IsAssignableFrom(typeof(T)) ? CommandSourceAffinity : 0;
    }

    /// <summary>Sets <paramref name="command"/> as the control's <c>Command</c> and binds its <c>CommandParameter</c> to <paramref name="commandParameter"/>.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <param name="command">The command, or null to clear the control's command.</param>
    /// <param name="target">The control, or null when there is nothing to bind.</param>
    /// <param name="commandParameter">The values the control's <c>CommandParameter</c> follows.</param>
    /// <returns>
    /// A binding that clears the command and parameter when disposed; an empty disposable after clearing the command;
    /// or null when <paramref name="target"/> is null, or when the command is null and the target is not a command source.
    /// </returns>
    /// <exception cref="InvalidOperationException"><paramref name="target"/> is not an <c>InputElement</c> that implements <c>ICommandSource</c>.</exception>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class
    {
        if (target is null)
        {
            return null;
        }

        if (target is not (InputElement element and ICommandSource))
        {
            if (command is null)
            {
                return null;
            }

            throw new InvalidOperationException("Target must be an InputElement and implement ICommandSource.");
        }

        if (command is null)
        {
            element.SetCurrentValue(Button.CommandProperty, null);
            return EmptyDisposable.Instance;
        }

        // Button.CommandProperty is the property every button-like control and menu item shares.
        element.SetCurrentValue(Button.CommandProperty, command);
        var parameterBinding = element.Bind(Button.CommandParameterProperty, commandParameter);
        return Scope.Create<CommandPropertyBinding>(
            new(element, command, parameterBinding),
            static state =>
            {
                state.ParameterBinding.Dispose();
                if (!ReferenceEquals(state.Element.GetValue(Button.CommandProperty), state.Command))
                {
                    return;
                }

                state.Element.ClearValue(Button.CommandProperty);
            });
    }

    /// <summary>Executes <paramref name="command"/> whenever the routed event named <paramref name="eventName"/> is raised on the control.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <typeparam name="TEventArgs">The event's argument type; the routed event's arguments must be assignable to it.</typeparam>
    /// <param name="command">The command, or null when there is nothing to bind.</param>
    /// <param name="target">The control, or null when there is nothing to bind.</param>
    /// <param name="commandParameter">The values passed to the command.</param>
    /// <param name="eventName">The name of a routed event the control or one of its base types registers.</param>
    /// <returns>A binding that removes the handler when disposed, or null when the command or target is null.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="target"/> is not an <c>InputElement</c>, or registers no routed event named <paramref name="eventName"/>
    /// whose arguments are a <typeparamref name="TEventArgs"/>.
    /// </exception>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        string eventName)
        where T : class
    {
        if (command is null || target is null)
        {
            return null;
        }

        if (target is not InputElement element)
        {
            throw new InvalidOperationException("Target must be an InputElement.");
        }

        var routedEvent = FindRoutedEvent(element.GetType(), eventName, typeof(TEventArgs))
            ?? throw new InvalidOperationException($"Routed Event {eventName} not found on {element.GetType().Name} element.");
        return new RoutedEventCommandBinding(element, routedEvent, command, commandParameter);
    }

    /// <summary>Executes <paramref name="command"/> whenever the event attached through <paramref name="addHandler"/> is raised.</summary>
    /// <typeparam name="T">The control type.</typeparam>
    /// <typeparam name="TEventArgs">The event's argument type.</typeparam>
    /// <param name="command">The command, or null when there is nothing to bind.</param>
    /// <param name="target">The control, or null when there is nothing to bind.</param>
    /// <param name="commandParameter">The values passed to the command.</param>
    /// <param name="addHandler">Attaches a handler to the event.</param>
    /// <param name="removeHandler">Detaches the handler from the event.</param>
    /// <returns>A binding that detaches the handler when disposed, or null when the command or target is null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="addHandler"/> or <paramref name="removeHandler"/> is null.</exception>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs
    {
        ArgumentExceptionHelper.ThrowIfNull(addHandler);
        ArgumentExceptionHelper.ThrowIfNull(removeHandler);

        return command is null || target is null
            ? null
            : new HandlerCommandBinding<TEventArgs>(command, commandParameter, addHandler, removeHandler);
    }

    /// <summary>Finds a routed event by name and argument type on <paramref name="type"/> or one of its base types.</summary>
    /// <param name="type">The input element type to start from.</param>
    /// <param name="eventName">The event name; the comparison is case-sensitive.</param>
    /// <param name="argumentsType">The type the event's arguments must be assignable to.</param>
    /// <returns>The routed event, or null when no type in the hierarchy registers a matching one.</returns>
    private static RoutedEvent? FindRoutedEvent(Type type, string eventName, Type argumentsType)
    {
        for (Type? current = type; typeof(InputElement).IsAssignableFrom(current); current = current.BaseType)
        {
            var registered = RoutedEventRegistry.Instance.GetRegistered(current);
            for (var i = 0; i < registered.Count; i++)
            {
                if (string.Equals(registered[i].Name, eventName, StringComparison.Ordinal)
                    && argumentsType.IsAssignableFrom(registered[i].EventArgsType))
                {
                    return registered[i];
                }
            }
        }

        return null;
    }

    /// <summary>The control, command and parameter binding a command-property binding releases when disposed.</summary>
    /// <param name="Element">The control whose <c>Command</c> was set.</param>
    /// <param name="Command">The command that was set.</param>
    /// <param name="ParameterBinding">The binding of the control's <c>CommandParameter</c>.</param>
    private readonly record struct CommandPropertyBinding(AvaloniaObject Element, ICommand Command, IDisposable ParameterBinding);

    /// <summary>Executes a command when a routed event is raised and keeps the element's <c>IsEnabled</c> in step with <c>CanExecute</c>.</summary>
    private sealed class RoutedEventCommandBinding : IDisposable, IObserver<object?>
    {
        /// <summary>The element the handler is attached to.</summary>
        private readonly InputElement _element;

        /// <summary>The routed event the handler is attached to.</summary>
        private readonly RoutedEvent _routedEvent;

        /// <summary>The command to execute.</summary>
        private readonly ICommand _command;

        /// <summary>The handler attached to the routed event.</summary>
        private readonly EventHandler<RoutedEventArgs> _handler;

        /// <summary>The subscription to the command parameter.</summary>
        private readonly IDisposable _parameterSubscription;

        /// <summary>The latest command parameter.</summary>
        private object? _parameter;

        /// <summary>Initializes a new instance of the <see cref="RoutedEventCommandBinding"/> class and attaches its handler.</summary>
        /// <param name="element">The element to attach the handler to.</param>
        /// <param name="routedEvent">The routed event to handle.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandParameter">The values passed to the command.</param>
        public RoutedEventCommandBinding(InputElement element, RoutedEvent routedEvent, ICommand command, IObservable<object?> commandParameter)
        {
            _element = element;
            _routedEvent = routedEvent;
            _command = command;
            _handler = OnEvent;
            _parameterSubscription = commandParameter.Subscribe(this);
            element.AddHandler(routedEvent, _handler, RoutingStrategies.Bubble);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _parameterSubscription.Dispose();
            _element.RemoveHandler(_routedEvent, _handler);
            _element.ClearValue(InputElement.IsEnabledProperty);
        }

        /// <inheritdoc/>
        public void OnNext(object? value)
        {
            _parameter = value;
            _element.SetCurrentValue(InputElement.IsEnabledProperty, _command.CanExecute(value));
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <summary>Executes the command with the latest parameter when it can execute.</summary>
        /// <param name="sender">The element that raised the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnEvent(object? sender, RoutedEventArgs args)
        {
            if (!_command.CanExecute(_parameter))
            {
                return;
            }

            _command.Execute(_parameter);
        }
    }

    /// <summary>Executes a command when a CLR event is raised.</summary>
    /// <typeparam name="TEventArgs">The event's argument type.</typeparam>
    private sealed class HandlerCommandBinding<TEventArgs> : IDisposable, IObserver<object?>
        where TEventArgs : EventArgs
    {
        /// <summary>The command to execute.</summary>
        private readonly ICommand _command;

        /// <summary>Detaches the handler from the event.</summary>
        private readonly Action<EventHandler<TEventArgs>> _removeHandler;

        /// <summary>The handler attached to the event.</summary>
        private readonly EventHandler<TEventArgs> _handler;

        /// <summary>The subscription to the command parameter.</summary>
        private readonly IDisposable _parameterSubscription;

        /// <summary>The latest command parameter.</summary>
        private object? _parameter;

        /// <summary>Initializes a new instance of the <see cref="HandlerCommandBinding{TEventArgs}"/> class and attaches its handler.</summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandParameter">The values passed to the command.</param>
        /// <param name="addHandler">Attaches the handler to the event.</param>
        /// <param name="removeHandler">Detaches the handler from the event.</param>
        public HandlerCommandBinding(
            ICommand command,
            IObservable<object?> commandParameter,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
        {
            _command = command;
            _removeHandler = removeHandler;
            _handler = OnEvent;
            _parameterSubscription = commandParameter.Subscribe(this);
            addHandler(_handler);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _parameterSubscription.Dispose();
            _removeHandler(_handler);
        }

        /// <inheritdoc/>
        public void OnNext(object? value) => _parameter = value;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <summary>Executes the command with the latest parameter when it can execute.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnEvent(object? sender, TEventArgs args)
        {
            if (!_command.CanExecute(_parameter))
            {
                return;
            }

            _command.Execute(_parameter);
        }
    }
}
