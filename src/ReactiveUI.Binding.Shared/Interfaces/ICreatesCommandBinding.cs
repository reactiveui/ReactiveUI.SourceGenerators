// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Plugin interface for types that can bind an <see cref="ICommand"/> to a control.
/// Implementations register with Splat and are resolved by affinity scoring.
/// </summary>
/// <remarks>
/// <para>
/// This interface follows the same affinity-based resolution pattern as
/// <see cref="ICreatesObservableForProperty"/>. Multiple implementations may be
/// registered; the one with the highest affinity score for a given control type wins.
/// </para>
/// <para>
/// Platform-specific modules (WPF, WinForms) register their own implementations
/// to handle platform-specific binding semantics (e.g., WPF's Command/CommandParameter
/// properties, WinForms event-based binding).
/// </para>
/// </remarks>
public interface ICreatesCommandBinding
{
    /// <summary>
    /// Returns a positive integer when this implementation supports binding a command
    /// to an object of the specified type. If the binding is not supported,
    /// the method returns a non-positive integer. In cases where multiple
    /// implementations return positive values, the one with the highest value wins.
    /// </summary>
    /// <typeparam name="T">The type of the control to bind to.</typeparam>
    /// <param name="hasEventTarget">Whether the caller specifies a custom event target.</param>
    /// <returns>A positive integer if binding is supported, or zero/negative if not.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; the interface shape dictates it.")]
    [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Names the control type; implementations resolve it via typeof(T).")]
    int GetAffinityForObject<T>(bool hasEventTarget);

    /// <summary>
    /// Binds an <see cref="ICommand"/> to a UI object using the default event.
    /// The default event is determined by the implementation (e.g., Click, TouchUpInside).
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="command">The command to bind. If <see langword="null"/>, no binding is created.</param>
    /// <param name="target">The target object, usually a UI control.</param>
    /// <param name="commandParameter">An observable that provides the command parameter value.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that disconnects the binding when disposed, or
    /// <see langword="null"/> if no binding was created.
    /// </returns>
    IDisposable? BindCommandToObject<T>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter)
        where T : class;

    /// <summary>Binds an <see cref="ICommand"/> to a UI object to a specific named event.</summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="TEventArgs">The event argument type.</typeparam>
    /// <param name="command">The command to bind. If <see langword="null"/>, no binding is created.</param>
    /// <param name="target">The target object, usually a UI control.</param>
    /// <param name="commandParameter">An observable that provides the command parameter value.</param>
    /// <param name="eventName">The event to bind to.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that disconnects the binding when disposed, or
    /// <see langword="null"/> if no binding was created.
    /// </returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the bound command shape.")]
    [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Types the event handler this overload binds; it mirrors the add/remove handler overload.")]
    IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        string eventName)
        where T : class;

    /// <summary>Binds a command to a specific event on a target object using explicit add/remove handler delegates.</summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="TEventArgs">The event arguments type.</typeparam>
    /// <param name="command">The command to bind. If <see langword="null"/>, no binding is created.</param>
    /// <param name="target">The target object.</param>
    /// <param name="commandParameter">An observable that supplies command parameter values.</param>
    /// <param name="addHandler">Adds the handler to the target event.</param>
    /// <param name="removeHandler">Removes the handler from the target event.</param>
    /// <returns>A disposable that unbinds the command.</returns>
    IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs;
}
