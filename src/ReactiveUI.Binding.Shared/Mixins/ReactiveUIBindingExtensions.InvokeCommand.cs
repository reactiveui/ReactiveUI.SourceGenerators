// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension methods that execute a command with each value an observable produces (InvokeCommand).</summary>
/// <remarks>
/// The command is offered every value the sequence produces, as the command parameter, and a value the command
/// refuses is dropped. A <c>ReactiveCommand</c> is reached through these overloads like any other
/// <see cref="ICommand"/>: the parameter is passed as <see cref="object"/> rather than the command's input type.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed an InvokeCommand call site.</summary>
    private const string NoInvokeCommandDispatchMessage =
        "No generated InvokeCommand dispatch matched this call site. Use InvokeCommandUnsafe to resolve the expression at run time.";

    /// <summary>Executes a command with each value the sequence produces as its parameter, skipping a value the command cannot execute.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>A disposable that, when disposed, stops executing the command.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// The command is the caller's own object rather than a property to observe, so there is nothing here for the
    /// generator to resolve and no dispatch is emitted for this overload. An error from <paramref name="source"/>
    /// is rethrown on the thread that raised it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable InvokeCommand<T>(this IObservable<T> source, ICommand command) =>
        CommandBinding.CommandInvoker.Invoke(source, command);

    /// <summary>Executes the command a property holds with each value as its parameter, skipping a value while there is no command or the command cannot execute it.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <typeparam name="TTarget">The type declaring the command property.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="target">The object declaring the command property; null executes nothing.</param>
    /// <param name="commandProperty">An expression that selects the command property to execute.</param>
    /// <param name="commandPropertyExpression">The caller argument expression for <paramref name="commandProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, stops executing the command and stops observing the property.</returns>
    /// <exception cref="InvalidOperationException">No generated InvokeCommand dispatch matched this call site. Use InvokeCommandUnsafe to resolve the expression at run time.</exception>
    public static IDisposable InvokeCommand<T, TTarget>(
        this IObservable<T> source,
        TTarget? target,
        Expression<Func<TTarget, ICommand?>> commandProperty,
        [CallerArgumentExpression("commandProperty")] string commandPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
        => throw new InvalidOperationException(NoInvokeCommandDispatchMessage);
}
