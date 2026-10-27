// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Binds a command to a control, resolving the command and control properties at runtime.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeCommandBindingFallback
{
    /// <summary>Keeps the command an observed property holds bound to the control another one holds.</summary>
    /// <typeparam name="TView">The type declaring the control property.</typeparam>
    /// <typeparam name="TViewModel">The type declaring the command property.</typeparam>
    /// <typeparam name="TProp">The type of the command property.</typeparam>
    /// <typeparam name="TControl">The type of the control the command is bound to.</typeparam>
    /// <param name="view">The object declaring the control property.</param>
    /// <param name="viewModel">The object declaring the command property.</param>
    /// <param name="commandProperty">The property holding the command to bind.</param>
    /// <param name="controlProperty">The property holding the control to bind it to.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <param name="toEvent">The control event that executes the command, or null for the control's default.</param>
    /// <param name="bindingExpression">The bound expression, named when the observation faults.</param>
    /// <returns>A disposable that, when disposed, unbinds the command and stops observing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commandProperty"/> or <paramref name="controlProperty"/> is null.</exception>
    /// <remarks>
    /// The command is rebound whenever either property changes, on the view's owning thread. Nothing is bound while
    /// the view model, the command or the control is null, or when no registered binder supports the control.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime command binding resolves the property chain by reflection.")]
    public static IDisposable BindCommand<
        TView,
        TViewModel,
        TProp,
        TControl>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp?>> commandProperty,
        Expression<Func<TView, TControl>> controlProperty,
        IObservable<object?> commandParameter,
        string? toEvent,
        string bindingExpression)
        where TView : class
        where TViewModel : class
        where TProp : ICommand
        where TControl : class
    {
        ArgumentExceptionHelper.ThrowIfNull(commandProperty);
        ArgumentExceptionHelper.ThrowIfNull(controlProperty);

        // No view model is the ordinary state before one is assigned, so nothing is bound.
        if (viewModel is null)
        {
            return EmptyDisposable.Instance;
        }

        var binding = new SwapDisposable();
        var commands = RuntimeObservationFallback.WhenAnyValue(viewModel, commandProperty);
        var controls = RuntimeObservationFallback.WhenAnyValue(view, controlProperty);

        // Each rebind touches the control, so it is delivered on the view's thread.
        var observation = BindingErrors.Subscribe(
            BindingSchedulers.ObserveOnViewThread(
                new CombineLatestSignal<TProp?, TControl, (TProp? Command, TControl Control)>(commands, controls, static (command, control) => (command, control)),
                view),
            pair => binding.Disposable = Bind(pair.Command, pair.Control, commandParameter, toEvent),
            bindingExpression);

        return new MultipleDisposable(observation, binding);
    }

    /// <summary>Hands one command and one control to the binder with the highest affinity for that control.</summary>
    /// <typeparam name="TProp">The type of the command.</typeparam>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="command">The command to execute, or null while the property holds none.</param>
    /// <param name="control">The control that executes it, or null while the property holds none.</param>
    /// <param name="commandParameter">The values offered as the command parameter.</param>
    /// <param name="toEvent">The control event that executes the command, or null for the control's default.</param>
    /// <returns>The binding, or an empty disposable when there is nothing to bind.</returns>
    private static IDisposable Bind<TProp, TControl>(
        TProp? command,
        TControl? control,
        IObservable<object?> commandParameter,
        string? toEvent)
        where TProp : ICommand
        where TControl : class
    {
        if (command is null || control is null)
        {
            return EmptyDisposable.Instance;
        }

        var binder = CommandBinding.CommandBinderService.GetBinder<TControl>(toEvent is not null);

        var bound = toEvent is null
            ? binder?.BindCommandToObject(command, control, commandParameter)
            : binder?.BindCommandToObject<TControl, EventArgs>(command, control, commandParameter, toEvent);

        return bound ?? EmptyDisposable.Instance;
    }
}
