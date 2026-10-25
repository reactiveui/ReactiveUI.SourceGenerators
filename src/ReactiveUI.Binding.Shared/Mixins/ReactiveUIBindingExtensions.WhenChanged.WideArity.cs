// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension methods for observing property changes after they occur (WhenChanged).</summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a WhenChanged call site.</summary>
    private const string NoWhenChangedDispatchMessage =
        "No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.";

    /// <summary>Observes a property and emits its current value when subscribed, then its new value after each change that alters it.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits the current value on subscription and the new value after each change that alters it.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    public static IObservable<T1> WhenChanged<TObj, T1>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 2 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    public static IObservable<PropertyValues<T1, T2>> WhenChanged<TObj, T1, T2>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 3 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IObservable<PropertyValues<T1, T2, T3>> WhenChanged<TObj, T1, T2, T3>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        [CallerArgumentExpression("property1")]
        string property1Expression = "",
        [CallerArgumentExpression("property2")]
        string property2Expression = "",
        [CallerArgumentExpression("property3")]
        string property3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 4 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<PropertyValues<T1, T2, T3, T4>> WhenChanged<
        TObj,
        T1,
        T2,
        T3,
        T4>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 5 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5>> WhenChanged<
        TObj,
        T1,
        T2,
        T3,
        T4,
        T5>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 6 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6>>
        WhenChanged<TObj, T1, T2, T3, T4, T5, T6>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 7 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7>>
        WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 8 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8>> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 9 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 10 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 11 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> WhenChanged<
                TObj,
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
                T11>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 12 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> WhenChanged<
                TObj,
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
                T12>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
            Expression<Func<TObj, T12>> property12,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 13 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> WhenChanged<
                TObj,
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
                T13>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
            Expression<Func<TObj, T12>> property12,
            Expression<Func<TObj, T13>> property13,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 14 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
        WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
            Expression<Func<TObj, T12>> property12,
            Expression<Func<TObj, T13>> property13,
            Expression<Func<TObj, T14>> property14,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 15 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
            Expression<Func<TObj, T12>> property12,
            Expression<Func<TObj, T13>> property13,
            Expression<Func<TObj, T14>> property14,
            Expression<Func<TObj, T15>> property15,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }

    /// <summary>Observes 16 properties and emits a PropertyValues of their current values when subscribed and whenever any of them changes.</summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <returns>An observable that emits a PropertyValues holding the latest value of each property, on subscription and whenever any of them changes.</returns>
    /// <exception cref="InvalidOperationException">No generated WhenChanged dispatch matched this call site. Use WhenChangedUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    public static
        IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> WhenChanged<
                TObj,
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
                T16>(
            this TObj objectToMonitor,
            Expression<Func<TObj, T1>> property1,
            Expression<Func<TObj, T2>> property2,
            Expression<Func<TObj, T3>> property3,
            Expression<Func<TObj, T4>> property4,
            Expression<Func<TObj, T5>> property5,
            Expression<Func<TObj, T6>> property6,
            Expression<Func<TObj, T7>> property7,
            Expression<Func<TObj, T8>> property8,
            Expression<Func<TObj, T9>> property9,
            Expression<Func<TObj, T10>> property10,
            Expression<Func<TObj, T11>> property11,
            Expression<Func<TObj, T12>> property12,
            Expression<Func<TObj, T13>> property13,
            Expression<Func<TObj, T14>> property14,
            Expression<Func<TObj, T15>> property15,
            Expression<Func<TObj, T16>> property16,
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
        where TObj : class
    {
        throw new InvalidOperationException(NoWhenChangedDispatchMessage);
    }
}
