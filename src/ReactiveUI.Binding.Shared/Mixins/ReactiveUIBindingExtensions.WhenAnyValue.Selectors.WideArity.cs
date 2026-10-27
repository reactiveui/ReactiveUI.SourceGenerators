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
/// Extension methods for observing property changes with a selector function using ReactiveUI conventions (WhenAnyValue with selector).
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes 1 property and emits the result of a selector applied to its current value when subscribed and after each change. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="selector">A function that converts the observed property value to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and after each change of the observed property.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    public static IObservable<TRet> WhenAnyValue<TSender, T1, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Func<T1, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 2 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Func<T1, T2, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 3 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Func<T1, T2, T3, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 4 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Func<T1, T2, T3, T4, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 5 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Func<T1, T2, T3, T4, T5, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 6 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Func<T1, T2, T3, T4, T5, T6, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 7 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Func<T1, T2, T3, T4, T5, T6, T7, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 8 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 9 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 10 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 11 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 12 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerArgumentExpression("property12")]
        string property12Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 13 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerArgumentExpression("property12")]
        string property12Expression = "",
        [CallerArgumentExpression("property13")]
        string property13Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 14 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerArgumentExpression("property12")]
        string property12Expression = "",
        [CallerArgumentExpression("property13")]
        string property13Expression = "",
        [CallerArgumentExpression("property14")]
        string property14Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 15 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="property15Expression">The caller argument expression for <paramref name="property15"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Expression<Func<TSender, T15>> property15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerArgumentExpression("property12")]
        string property12Expression = "",
        [CallerArgumentExpression("property13")]
        string property13Expression = "",
        [CallerArgumentExpression("property14")]
        string property14Expression = "",
        [CallerArgumentExpression("property15")]
        string property15Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);

    /// <summary>
    /// Observes 16 properties and emits the result of a selector applied to their current values when subscribed and whenever any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="T16">The type of the sixteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="property16">An expression that selects the sixteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="property15Expression">The caller argument expression for <paramref name="property15"/>. Auto-populated by the compiler.</param>
    /// <param name="property16Expression">The caller argument expression for <paramref name="property16"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the selector result on subscription and whenever any of the observed properties changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAnyValue dispatch matched this call site. Use WhenAnyValueUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAnyValue<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        T16,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Expression<Func<TSender, T15>> property15,
        Expression<Func<TSender, T16>> property16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerArgumentExpression("property4")]
        string property4Expression = "",
        [CallerArgumentExpression("property5")]
        string property5Expression = "",
        [CallerArgumentExpression("property6")]
        string property6Expression = "",
        [CallerArgumentExpression("property7")]
        string property7Expression = "",
        [CallerArgumentExpression("property8")]
        string property8Expression = "",
        [CallerArgumentExpression("property9")]
        string property9Expression = "",
        [CallerArgumentExpression("property10")]
        string property10Expression = "",
        [CallerArgumentExpression("property11")]
        string property11Expression = "",
        [CallerArgumentExpression("property12")]
        string property12Expression = "",
        [CallerArgumentExpression("property13")]
        string property13Expression = "",
        [CallerArgumentExpression("property14")]
        string property14Expression = "",
        [CallerArgumentExpression("property15")]
        string property15Expression = "",
        [CallerArgumentExpression("property16")]
        string property16Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
        => throw new InvalidOperationException(NoWhenAnyValueDispatchMessage);
}
