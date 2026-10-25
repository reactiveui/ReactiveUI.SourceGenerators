// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension methods for property binding (BindOneWay, BindTwoWay, OneWayBind, Bind).</summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a Bind call site.</summary>
    private const string NoBindDispatchMessage =
        "No generated Bind dispatch matched this call site. Use BindUnsafe to resolve the expression at run time.";

    /// <summary>Reported when no generated dispatch claimed a BindOneWay call site.</summary>
    private const string NoBindOneWayDispatchMessage =
        "No generated BindOneWay dispatch matched this call site. Use BindOneWayUnsafe to resolve the expression at run time.";

    /// <summary>Reported when no generated dispatch claimed a BindTwoWay call site.</summary>
    private const string NoBindTwoWayDispatchMessage =
        "No generated BindTwoWay dispatch matched this call site. Use BindTwoWayUnsafe to resolve the expression at run time.";

    /// <summary>Reported when no generated dispatch claimed a OneWayBind call site.</summary>
    private const string NoOneWayBindDispatchMessage =
        "No generated OneWayBind dispatch matched this call site. Use OneWayBindUnsafe to resolve the expression at run time.";

    /// <summary>Binds a source property to a target property one way, writing the current value and each later change on the target's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding; a binding hook that vetoes the binding leaves nothing bound.</returns>
    /// <exception cref="InvalidOperationException">No generated BindOneWay dispatch matched this call site. Use BindOneWayUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IDisposable BindOneWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        [CallerArgumentExpression("sourceProperty")]
        string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")]
        string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindOneWayDispatchMessage);
    }

    /// <summary>Binds a source property to a target property one way through a conversion function, writing on the target's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding; a binding hook that vetoes the binding leaves nothing bound.</returns>
    /// <exception cref="InvalidOperationException">No generated BindOneWay dispatch matched this call site. Use BindOneWayUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IDisposable BindOneWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc,
        [CallerArgumentExpression("sourceProperty")]
        string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")]
        string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindOneWayDispatchMessage);
    }

    /// <summary>Binds a source and a target property to each other, seeding the target from the source and mirroring each change on the other side's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding; a binding hook that vetoes the binding leaves nothing bound.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTwoWay dispatch matched this call site. Use BindTwoWayUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IDisposable BindTwoWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        [CallerArgumentExpression("sourceProperty")]
        string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")]
        string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindTwoWayDispatchMessage);
    }

    /// <summary>Binds a source and a target property to each other through conversion functions, seeding the target and mirroring each change on the other side's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
    /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding; a binding hook that vetoes the binding leaves nothing bound.</returns>
    /// <exception cref="InvalidOperationException">No generated BindTwoWay dispatch matched this call site. Use BindTwoWayUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IDisposable BindTwoWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv,
        [CallerArgumentExpression("sourceProperty")]
        string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")]
        string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
    {
        throw new InvalidOperationException(NoBindTwoWayDispatchMessage);
    }

    /// <summary>Binds a view model property to a view property one way, writing the current value and each later change on the view's owning thread.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding, or null when a binding hook vetoes it.</returns>
    /// <exception cref="InvalidOperationException">No generated OneWayBind dispatch matched this call site. Use OneWayBindUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IReactiveBinding<TView, TVProp> OneWayBind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        [CallerArgumentExpression("viewModelProperty")]
        string viewModelPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")]
        string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoOneWayBindDispatchMessage);
    }

    /// <summary>Binds a view model property to a view property one way through a selector, writing on the view's owning thread.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TProp">The type of the view model property.</typeparam>
    /// <typeparam name="TOut">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="selector">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding, or null when a binding hook vetoes it.</returns>
    /// <exception cref="InvalidOperationException">No generated OneWayBind dispatch matched this call site. Use OneWayBindUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IReactiveBinding<TView, TOut> OneWayBind<TViewModel, TView, TProp, TOut>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector,
        [CallerArgumentExpression("viewModelProperty")]
        string viewModelPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")]
        string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoOneWayBindDispatchMessage);
    }

    /// <summary>Binds a view model property and a view property to each other, seeding the view and mirroring each change on the other side's owning thread.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding, or null when a binding hook vetoes it.</returns>
    /// <exception cref="InvalidOperationException">No generated Bind dispatch matched this call site. Use BindUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        [CallerArgumentExpression("viewModelProperty")]
        string viewModelPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")]
        string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoBindDispatchMessage);
    }

    /// <summary>Binds a view model property and a view property to each other through conversion functions, seeding the view and mirroring each change on the other side's owning thread.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelToViewConverter">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewToViewModelConverter">A function that converts the view property value back to the view model property type.</param>
    /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding, or null when a binding hook vetoes it.</returns>
    /// <exception cref="InvalidOperationException">No generated Bind dispatch matched this call site. Use BindUnsafe to resolve the expression at run time.</exception>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> viewModelToViewConverter,
        Func<TVProp, TVMProp> viewToViewModelConverter,
        [CallerArgumentExpression("viewModelProperty")]
        string viewModelPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")]
        string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoBindDispatchMessage);
    }
}
