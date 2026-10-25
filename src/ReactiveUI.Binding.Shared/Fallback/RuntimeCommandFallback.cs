// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Resolves the command an <c>InvokeCommand</c> executes through the runtime expression engine.</summary>
/// <remarks>
/// Reached only where the generator could not serve the call site - a receiver it cannot name, a selector that is
/// not an inline lambda, or a build whose compiler predates the dispatch this package emits. The executions
/// themselves are the generated path's, because both hand the resolved command to
/// <see cref="CommandBinding.CommandInvoker"/>; only how the command is found differs.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeCommandFallback
{
    /// <summary>Executes the command an observed property holds with each value the sequence produces.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <typeparam name="TTarget">The type declaring the observed command property.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="target">The object declaring the command property.</param>
    /// <param name="commandProperty">The property holding the command to execute.</param>
    /// <returns>A disposable that, when disposed, stops executing the command.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="commandProperty"/> is null.</exception>
    /// <remarks>
    /// A null target holds no property to observe, so the values are dropped rather than faulting the sequence -
    /// the same outcome as a target whose command property is null, which is the ordinary case before a view
    /// model is assigned.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime command fallback resolves the property chain by reflection.")]
    public static IDisposable InvokeCommand<T, TTarget>(
        IObservable<T> source,
        TTarget? target,
        Expression<Func<TTarget, ICommand?>> commandProperty)
        where TTarget : class
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(commandProperty);

        return target is null
            ? EmptyDisposable.Instance
            : CommandBinding.CommandInvoker.Invoke(
                source,
                RuntimeObservationFallback.WhenAnyValue(target, commandProperty));
    }
}
