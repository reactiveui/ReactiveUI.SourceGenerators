// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Extension methods that turn an observable into the backing store of a read-only property (<c>ToProperty</c>).
/// These generic stubs throw when no concrete generated overload is found; the source generator emits
/// concrete typed overloads that C# overload resolution prefers over these generic stubs.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a ToProperty call site.</summary>
    private const string NoToPropertyDispatchMessage =
        "No generated ToProperty dispatch matched this call site. Name the property with a lambda of the form x => x.Property, "
        + "or with a constant such as nameof(Property), on a type whose change notifications generated code can raise; "
        + "otherwise construct ObservableAsPropertyHelper directly.";

    /// <summary>Backs the selected read-only property with the observable's latest value.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with optionally deferred subscription and a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        TRet initialValue,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value and a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        TRet initialValue,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value and optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        TRet initialValue,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value factory.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        Func<TRet> getInitialValue,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value factory and a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        Func<TRet> getInitialValue,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value factory and optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with optionally deferred subscription and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with optionally deferred
    /// subscription and a scheduler for its change notifications and also returns it through an out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value and optionally
    /// deferred subscription and also returns it through an out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications and also returns it through an out
    /// parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    [OverloadResolutionPriority(1)]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value factory and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription and also returns it through an out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications and also returns it through an
    /// out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A lambda of the form <c>x =&gt; x.Property</c> that names the property; the generator reads it when you build, and nothing calls it.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the API/overload under test")]
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Func<TObj, TRet> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value and a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        ISequencer? scheduler)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value and optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        bool deferSubscription)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        bool deferSubscription)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        ISequencer? scheduler)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with optionally deferred subscription and a scheduler for its change notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value factory.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value factory and optionally deferred subscription.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with optionally deferred subscription and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with optionally deferred subscription
    /// and a scheduler for its change notifications and also returns it through an out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value factory and also returns it through an out parameter.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription and also returns it through an out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications and also returns it through an
    /// out parameter.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property, as a constant such as <c>nameof(Property)</c>.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">No generated ToProperty dispatch matched this call site.</exception>
    public static ObservableAsPropertyHelper<TRet> ToProperty<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = null!;
        throw new InvalidOperationException(NoToPropertyDispatchMessage);
    }
}
