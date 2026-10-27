// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension methods for observing property changes with IObservedChange context (WhenAny).</summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a WhenAny call site.</summary>
    private const string NoWhenAnyDispatchMessage =
        "No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.";

    /// <summary>Observes 1 property and emits the selector applied to its observed change, on subscription and after each change.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    public static IObservable<TRet> WhenAny<TSender, TRet, T1>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Func<IObservedChange<TSender, T1>, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 2 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Func<IObservedChange<TSender, T1>, IObservedChange<TSender, T2>, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 3 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Func<IObservedChange<TSender, T1>, IObservedChange<TSender, T2>, IObservedChange<TSender, T3>, TRet> selector,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 4 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 5 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 6 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 7 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 8 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
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
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 9 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
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
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 10 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
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
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 11 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <typeparam name="T11">The type of property 11 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="property11">An expression that selects property 11 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
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
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            IObservedChange<TSender, T11>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }

    /// <summary>Observes 12 properties and emits the selector applied to their observed changes, on subscription and whenever any of them changes.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <typeparam name="T11">The type of property 11 value.</typeparam>
    /// <typeparam name="T12">The type of property 12 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="property11">An expression that selects property 11 to observe.</param>
    /// <param name="property12">An expression that selects property 12 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
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
    /// <returns>An observable of selector results, emitted on subscription and whenever an observed property changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenAny dispatch matched this call site. Use WhenAnyUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<TRet> WhenAny<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            IObservedChange<TSender, T11>,
            IObservedChange<TSender, T12>,
            TRet> selector,
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
    {
        throw new InvalidOperationException(NoWhenAnyDispatchMessage);
    }
}
