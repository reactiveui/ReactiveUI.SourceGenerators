// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The opt-in half of the scheduler overloads, which resolves a binding by reflection.</summary>
/// <remarks>
/// The unsuffixed members resolve at compile time and throw when no generated dispatch claimed the call site;
/// these walk the chain by reflection instead, and are annotated so a consumer publishing trimmed or
/// ahead-of-time sees the requirement at their own call site. They are declared as classic static extension
/// methods rather than extension-block members, because nothing has to lose extension lookup to a generated
/// overload here.
/// </remarks>
public static partial class ReactiveSchedulerExtensions
{
    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a specified scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindOneWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        ISequencer? scheduler)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a conversion function and a specified scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindOneWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc,
        ISequencer? scheduler)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            conversionFunc,
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a one-way binding from a source property to a target property using an explicit <see cref="IBindingTypeConverter"/>, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline.</param>
    /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>A value the converter cannot convert is written as the default of the destination type.</remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindOneWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        IBindingTypeConverter converter,
        ISequencer? scheduler,
        object? conversionHint)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TSourceProp, TTargetProp>(
                        value,
                        conversionHint,
                        converter,
                        out var converted);
                    return converted;
                },
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a two-way binding between a source property and a target property with a specified scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline. Writes back are not scheduled.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindTwoWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        ISequencer? scheduler)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a two-way binding between a source property and a target property with conversion functions and a specified scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
    /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline. Writes back are not scheduled.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindTwoWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv,
        ISequencer? scheduler)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            TwoWayConverters.Create(sourceToTargetConv, targetToSourceConv),
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a two-way binding between a source property and a target property using explicit <see cref="IBindingTypeConverter"/> instances, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source the binding is rooted on.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConverter">The converter for source-to-target conversion.</param>
    /// <param name="targetToSourceConverter">The converter for target-to-source conversion.</param>
    /// <param name="scheduler">The scheduler the write to the target is delivered on, or null for its owning thread; an immediate scheduler writes inline. Writes back are not scheduled.</param>
    /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>A value the converter cannot convert is written as the default of the destination type.</remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindTwoWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        IBindingTypeConverter sourceToTargetConverter,
        IBindingTypeConverter targetToSourceConverter,
        ISequencer? scheduler,
        object? conversionHint)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            TwoWayConverters.Create<TSourceProp, TTargetProp>(
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TSourceProp, TTargetProp>(
                        value,
                        conversionHint,
                        sourceToTargetConverter,
                        out var converted);
                    return converted;
                },
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TTargetProp, TSourceProp>(
                        value,
                        conversionHint,
                        targetToSourceConverter,
                        out var converted);
                    return converted;
                }),
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a one-way binding from a view model property to a view property with a specified selector and scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TProp">The type of the view model property.</typeparam>
    /// <typeparam name="TOut">The type of the view property.</typeparam>
    /// <param name="view">The view the binding is rooted on.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="selector">A function that converts the view model property value to the view property type.</param>
    /// <param name="scheduler">The scheduler the write to the view is delivered on, or null for its owning thread; an immediate scheduler writes inline.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, TOut> OneWayBindUnsafe<TView, TViewModel, TProp, TOut>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector,
        ISequencer? scheduler)
        where TView : class, IViewFor
        where TViewModel : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            selector,
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a one-way binding from a view model property to a view property using an explicit <see cref="IBindingTypeConverter"/>, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view the binding is rooted on.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
    /// <param name="scheduler">The scheduler the write to the view is delivered on, or null for its owning thread; an immediate scheduler writes inline.</param>
    /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>A value the converter cannot convert is written as the default of the destination type.</remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, TVProp> OneWayBindUnsafe<TView, TViewModel, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IBindingTypeConverter converter,
        ISequencer? scheduler,
        object? conversionHint)
        where TView : class, IViewFor
        where TViewModel : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(
                        value,
                        conversionHint,
                        converter,
                        out var converted);
                    return converted;
                },
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a two-way binding between a view model property and a view property with conversion functions and a specified scheduler, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view the binding is rooted on.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelToViewConverter">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewToViewModelConverter">A function that converts the view property value back to the view model property type.</param>
    /// <param name="scheduler">The scheduler the write to the view is delivered on, or null for its owning thread; an immediate scheduler writes inline. Writes back are not scheduled.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TView, TViewModel, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> viewModelToViewConverter,
        Func<TVProp, TVMProp> viewToViewModelConverter,
        ISequencer? scheduler)
        where TView : class, IViewFor
        where TViewModel : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create(viewModelToViewConverter, viewToViewModelConverter),
            scheduler,
            bindingExpression);
    }

    /// <summary>
    /// Creates a two-way binding between a view model property and a view property using explicit <see cref="IBindingTypeConverter"/> instances, resolving the property chains by reflection.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view the binding is rooted on.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelToViewConverter">The converter for view model-to-view conversion.</param>
    /// <param name="viewToViewModelConverter">The converter for view-to-view model conversion.</param>
    /// <param name="scheduler">The scheduler the write to the view is delivered on, or null for its owning thread; an immediate scheduler writes inline. Writes back are not scheduled.</param>
    /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>A value the converter cannot convert is written as the default of the destination type.</remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TView, TViewModel, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IBindingTypeConverter viewModelToViewConverter,
        IBindingTypeConverter viewToViewModelConverter,
        ISequencer? scheduler,
        object? conversionHint)
        where TView : class, IViewFor
        where TViewModel : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create<TVMProp, TVProp>(
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(
                        value,
                        conversionHint,
                        viewModelToViewConverter,
                        out var converted);
                    return converted;
                },
                value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVProp, TVMProp>(
                        value,
                        conversionHint,
                        viewToViewModelConverter,
                        out var converted);
                    return converted;
                }),
            scheduler,
            bindingExpression);
    }
}
