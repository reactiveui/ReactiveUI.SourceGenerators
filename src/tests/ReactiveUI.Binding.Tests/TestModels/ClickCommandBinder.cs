// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>Binds a command to <see cref="DispatchStubControl.Click"/>, tracking the latest parameter.</summary>
/// <remarks>
/// The runtime library ships no command binder of its own, so a binding built through the runtime path has
/// nothing to subscribe its parameter stream with. Registering this one is what lets a test drive a command
/// execution and see the parameter that reached it.
/// </remarks>
public class ClickCommandBinder : ICreatesCommandBinding
{
    /// <summary>The affinity returned for a control this binder knows how to reach.</summary>
    private const int SupportedAffinity = 100;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObject<T>(bool hasEventTarget) =>
        typeof(T) == typeof(DispatchStubControl) ? SupportedAffinity : 0;

    /// <inheritdoc/>
    public IDisposable? BindCommandToObject<T>(ICommand? command, T? target, IObservable<object?> commandParameter)
        where T : class
    {
        if (command is null || target is not DispatchStubControl control)
        {
            return null;
        }

        object? latest = null;
        var parameters = commandParameter.Subscribe(new LatestParameter(value => latest = value));

        EventHandler onClick = (_, _) =>
        {
            if (!command.CanExecute(latest))
            {
                return;
            }

            command.Execute(latest);
        };

        control.Click += onClick;

        return new UnbindOnDispose(() =>
        {
            control.Click -= onClick;
            parameters.Dispose();
        });
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "type parameter is dictated by the interface / specified explicitly by the caller")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        string eventName)
        where T : class => BindCommandToObject(command, target, commandParameter);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs => BindCommandToObject(command, target, commandParameter);

    /// <summary>Records the most recent value a parameter stream produced.</summary>
    /// <param name="onValue">Receives each value.</param>
    private sealed class LatestParameter(Action<object?> onValue) : IObserver<object?>
    {
        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(object? value) => onValue(value);
    }

    /// <summary>Runs an action when disposed.</summary>
    /// <param name="unbind">The action that undoes the binding.</param>
    private sealed class UnbindOnDispose(Action unbind) : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => unbind();
    }
}
