// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>A command binder that takes the command parameter in its own type, so no value is boxed.</summary>
/// <remarks>
/// A generated <c>BindCommand</c> calls these members when the registered binder that wins implements them, and
/// the <see cref="ICreatesCommandBinding"/> members otherwise.
/// </remarks>
public interface ICreatesTypedCommandBinding : ICreatesCommandBinding
{
    /// <summary>Binds a command to a control's default event.</summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    /// <param name="command">The command to bind. If <see langword="null"/>, no binding is created.</param>
    /// <param name="target">The target object, usually a UI control.</param>
    /// <param name="commandParameter">The command parameter values; empty when the binding has no parameter.</param>
    /// <returns>A disposable that disconnects the binding, or <see langword="null"/> if no binding was created.</returns>
    IDisposable? BindTypedCommandToObject<T, TParameter>(
        ICommand? command,
        T? target,
        IObservable<TParameter> commandParameter)
        where T : class;

    /// <summary>Binds a command to a named event of a control.</summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    /// <typeparam name="TEventArgs">The event argument type.</typeparam>
    /// <param name="command">The command to bind. If <see langword="null"/>, no binding is created.</param>
    /// <param name="target">The target object, usually a UI control.</param>
    /// <param name="commandParameter">The command parameter values; empty when the binding has no parameter.</param>
    /// <param name="eventName">The event to bind to.</param>
    /// <returns>A disposable that disconnects the binding, or <see langword="null"/> if no binding was created.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the bound command shape.")]
    [SuppressMessage("Design", "SST1452:Unused type parameter", Justification = "Types the event handler this overload binds; it mirrors ICreatesCommandBinding.")]
    IDisposable? BindTypedCommandToObject<T, TParameter, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<TParameter> commandParameter,
        string eventName)
        where T : class;
}
