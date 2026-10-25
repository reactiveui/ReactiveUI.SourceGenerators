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
/// The higher-arity WhenAnyObservable overloads, which combine the latest value of each observable property with a selector.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes 3 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Func<T1?, T2?, T3?, TRet> selector,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyObservableDispatchMessage);
    }

    /// <summary>
    /// Observes 4 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Func<T1?, T2?, T3?, T4?, TRet> selector,
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

    /// <summary>
    /// Observes 5 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Func<T1?, T2?, T3?, T4?, T5?, TRet> selector,
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

    /// <summary>
    /// Observes 6 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, TRet> selector,
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

    /// <summary>
    /// Observes 7 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRet> selector,
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

    /// <summary>
    /// Observes 8 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
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
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, TRet> selector,
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

    /// <summary>
    /// Observes 9 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
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
    /// <param name="selector">A function that combines the latest values from all observables.</param>
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
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, TRet> selector,
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

    /// <summary>
    /// Observes 10 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
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
    /// <param name="selector">A function that combines the latest values from all observables.</param>
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
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, TRet> selector,
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

    /// <summary>
    /// Observes 11 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
    /// <typeparam name="T11">The element type of observable property 11.</typeparam>
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
    /// <param name="selector">A function that combines the latest values from all observables.</param>
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
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Expression<Func<TSender, IObservable<T11>?>> obs11,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, TRet> selector,
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

    /// <summary>
    /// Observes 12 observable properties of different element types and emits the selector applied to the latest value of each, once every observable has produced a value.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
    /// <typeparam name="T11">The element type of observable property 11.</typeparam>
    /// <typeparam name="T12">The element type of observable property 12.</typeparam>
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
    /// <param name="selector">A function that combines the latest values from all observables.</param>
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
    /// <returns>An observable that emits the selector result each time one of the observables produces a value, once every observable has produced one.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyObservable dispatch matched this call site. Use WhenAnyObservableUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Expression<Func<TSender, IObservable<T11>?>> obs11,
        Expression<Func<TSender, IObservable<T12>?>> obs12,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, TRet> selector,
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
}
