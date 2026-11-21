// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.CommandBinding;
#else
namespace ReactiveUI.Binding.CommandBinding;
#endif

/// <summary>
/// Service that resolves the best <see cref="ICreatesCommandBinding"/> for a given
/// control type using affinity scoring. Follows the same resolution pattern as
/// property observation via <see cref="ICreatesObservableForProperty"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandBinderService
{
    /// <summary>Gets the highest-affinity <see cref="ICreatesCommandBinding"/> registered for the specified control type.</summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns>The best binder; the first registered wins a tie. Null when every registered binder returns an affinity of zero or less.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    public static ICreatesCommandBinding? GetBinder<T>(bool hasEventTarget)
    {
        var binders = Locator.Current.GetServices<ICreatesCommandBinding>();

        ICreatesCommandBinding? bestBinder = null;
        var bestAffinity = 0;

        foreach (var binder in binders)
        {
            var affinity = binder.GetAffinityForObject<T>(hasEventTarget);
            if (affinity <= bestAffinity)
            {
                continue;
            }

            bestAffinity = affinity;
            bestBinder = binder;
        }

        return bestBinder;
    }

    /// <summary>Binds a control's default event through a registered binder that outranks the generated binding.</summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    /// <param name="control">The control to bind.</param>
    /// <param name="commands">The command to bind, and each command that replaces it.</param>
    /// <param name="parameter">The command parameter values, or <see langword="null"/> when the binding has none.</param>
    /// <param name="generatedAffinity">The affinity of the generated binding, or -1 when there is none.</param>
    /// <returns>The binding, or <see langword="null"/> when no registered binder outranks the generated one.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commands"/> is <see langword="null"/>.</exception>
    public static IDisposable? BindCustom<TControl, TParameter>(
        TControl control,
        IObservable<ICommand?> commands,
        IObservable<TParameter>? parameter,
        int generatedAffinity)
        where TControl : class
    {
        ArgumentExceptionHelper.ThrowIfNull(commands);

        if (FindCustomBinder<TControl>(generatedAffinity, false) is not { } binder)
        {
            return null;
        }

        // A binder that takes the parameter in its own type is handed it as it is, so no value is boxed.
        if (binder is ICreatesTypedCommandBinding typed)
        {
            var typedValues = parameter ?? ImmutableEmptySignal<TParameter>.Instance;
            return Rebind(commands, command => typed.BindTypedCommandToObject(command, control, typedValues));
        }

        var values = ParameterValues(parameter);
        return Rebind(commands, command => binder.BindCommandToObject(command, control, values));
    }

    /// <summary>Binds a named event of a control through a registered binder that outranks the generated binding.</summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    /// <typeparam name="TEventArgs">The event's argument type.</typeparam>
    /// <param name="control">The control to bind.</param>
    /// <param name="commands">The command to bind, and each command that replaces it.</param>
    /// <param name="parameter">The command parameter values, or <see langword="null"/> when the binding has none.</param>
    /// <param name="generatedAffinity">The affinity of the generated binding, or -1 when there is none.</param>
    /// <param name="eventName">The event to bind to.</param>
    /// <returns>The binding, or <see langword="null"/> when no registered binder outranks the generated one.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commands"/> is <see langword="null"/>.</exception>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    public static IDisposable? BindCustom<TControl, TParameter, TEventArgs>(
        TControl control,
        IObservable<ICommand?> commands,
        IObservable<TParameter>? parameter,
        int generatedAffinity,
        string eventName)
        where TControl : class
    {
        ArgumentExceptionHelper.ThrowIfNull(commands);

        if (FindCustomBinder<TControl>(generatedAffinity, true) is not { } binder)
        {
            return null;
        }

        if (binder is ICreatesTypedCommandBinding typed)
        {
            var typedValues = parameter ?? ImmutableEmptySignal<TParameter>.Instance;
            return Rebind(commands, command => typed.BindTypedCommandToObject<TControl, TParameter, TEventArgs>(command, control, typedValues, eventName));
        }

        var values = ParameterValues(parameter);
        return Rebind(commands, command => binder.BindCommandToObject<TControl, TEventArgs>(command, control, values, eventName));
    }

    /// <summary>Finds the registered binder that outranks a generated binding.</summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="generatedAffinity">The affinity of the generated binding.</param>
    /// <param name="hasEventTarget">Whether the binding names its event.</param>
    /// <returns>The binder, or <see langword="null"/> when the generated binding ranks at least as high.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    private static ICreatesCommandBinding? FindCustomBinder<TControl>(int generatedAffinity, bool hasEventTarget) =>
        CommandBindingAffinityChecker.HasHigherAffinityPlugin<TControl>(generatedAffinity, hasEventTarget)
            ? GetBinder<TControl>(hasEventTarget)
            : null;

    /// <summary>Gets a binding's parameter values in the untyped form a registered binder takes.</summary>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    /// <param name="parameter">The typed parameter values, or <see langword="null"/> for none.</param>
    /// <returns>The parameter values, empty when the binding has none.</returns>
    private static IObservable<object?> ParameterValues<TParameter>(IObservable<TParameter>? parameter) =>
        parameter is null
            ? ImmutableEmptySignal<object?>.Instance
            : new MapSignal<TParameter, object?>(parameter, static value => value);

    /// <summary>Binds each command as it arrives, releasing the binding of the command it replaces.</summary>
    /// <param name="commands">The command to bind, and each command that replaces it.</param>
    /// <param name="bind">Binds one command, returning what releases it.</param>
    /// <returns>A disposable that stops following the commands and releases the current binding.</returns>
    private static MultipleDisposable Rebind(IObservable<ICommand?> commands, Func<ICommand?, IDisposable?> bind)
    {
        var current = new SwapDisposable();
        var subscription = commands.Subscribe(command =>
        {
            current.Disposable = EmptyDisposable.Instance;
            current.Disposable = bind(command) ?? EmptyDisposable.Instance;
        });

        return new(subscription, current);
    }
}
