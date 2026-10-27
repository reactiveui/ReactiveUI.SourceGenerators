// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Extension methods for observing properties that are themselves observables (WhenAnyObservable).
/// Automatically switches to the latest observable when the property value changes.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a WhenAnyObservable call site.</summary>
    private const string NoWhenAnyObservableDispatchMessage =
        "No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.";

    /// <summary>Observes 1 observable property and emits the values of the observable it holds, switching when the property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable the property held.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 2 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 3 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 4 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 5 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 6 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 7 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 8 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 9 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 10 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 11 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="obs11Expression">The caller argument expression for <paramref name="obs11"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerArgumentExpression("obs11")] string obs11Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>Observes 12 observable properties and merges the values of the observables they hold, switching each when its property changes; a null value emits nothing.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <param name="obs12">An expression that selects observable property 12 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="obs11Expression">The caller argument expression for <paramref name="obs11"/>. Auto-populated by the compiler.</param>
    /// <param name="obs12Expression">The caller argument expression for <paramref name="obs12"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the values of the latest observable each property held, merged into one sequence.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11,
        Expression<Func<TSender, IObservable<TRet>?>> obs12,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerArgumentExpression("obs11")] string obs11Expression = "",
        [CallerArgumentExpression("obs12")] string obs12Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>
    /// Observes 2 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Func<T1?, T2?, TRet> selector,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }
}
