// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves an InvokeCommand expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>InvokeCommand</c>. The unsuffixed overload resolves at compile time and throws when no
/// generated dispatch claimed the call site; this one walks the chain by reflection instead, and is annotated so a
/// consumer publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Executes the command a property holds with each value as its parameter, skipping a value while there is no command or the command cannot execute it.</summary>
    /// <typeparam name="T">The type of the value offered as the command parameter.</typeparam>
    /// <typeparam name="TTarget">The type declaring the command property.</typeparam>
    /// <param name="source">The sequence driving the executions.</param>
    /// <param name="target">The object declaring the command property; null executes nothing.</param>
    /// <param name="commandProperty">An expression that selects the command property to execute.</param>
    /// <returns>A disposable that, when disposed, stops executing the command and stops observing the property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="commandProperty"/> is null.</exception>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("Runtime command fallback resolves the property chain by reflection.")]
    public static IDisposable InvokeCommandUnsafe<T, TTarget>(
        this IObservable<T> source,
        TTarget? target,
        Expression<Func<TTarget, ICommand?>> commandProperty)
        where TTarget : class
        => RuntimeCommandFallback.InvokeCommand(source, target, commandProperty);
}
