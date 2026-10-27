// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Extension methods for binding an observable stream directly to a target property (<c>BindTo</c>).
/// These generic stubs throw when no concrete generated overload is found; the source generator emits
/// concrete typed overloads that C# overload resolution prefers over these generic stubs.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a BindTo call site.</summary>
    private const string NoBindToDispatchMessage =
        "No generated BindTo dispatch matched this call site. Use BindToUnsafe to resolve the expression at run time.";

    /// <summary>Writes each value the source produces to a target property, on the target's owning thread when its platform has one.</summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTo dispatch matched this call site. Use BindToUnsafe to resolve the expression at run time.</exception>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindToDispatchMessage);
    }

    /// <summary>
    /// Writes each value the source produces to a target property, passing the conversion hint to the
    /// converter that coerces the value to the property type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="conversionHint">An object that provides a hint to the converter. The semantics are defined by the converter.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTo dispatch matched this call site. Use BindToUnsafe to resolve the expression at run time.</exception>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindToDispatchMessage);
    }

    /// <summary>
    /// Writes each value the source produces to a target property, coercing it with the supplied converter
    /// instead of a registered one.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="converterOverride">An explicit converter to use when converting the source value to the target property type.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTo dispatch matched this call site. Use BindToUnsafe to resolve the expression at run time.</exception>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        IBindingTypeConverter? converterOverride,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindToDispatchMessage);
    }

    /// <summary>
    /// Writes each value the source produces to a target property, coercing it with the supplied converter
    /// instead of a registered one and passing the conversion hint to it.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="conversionHint">An object that provides a hint to the converter. The semantics are defined by the converter.</param>
    /// <param name="converterOverride">An explicit converter to use when converting the source value to the target property type.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTo dispatch matched this call site. Use BindToUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindToDispatchMessage);
    }
}
