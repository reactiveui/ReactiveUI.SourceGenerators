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

/// <summary>Resolves a BindOneWay expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>BindOneWay</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Binds a source property to a target property one way, writing the current value and each later change on the target's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindOneWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            null,
            bindingExpression);
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
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindOneWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc)
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
            null,
            bindingExpression);
    }

    /// <summary>Binds a source and a target property to each other, seeding the target from the source and mirroring each change on the other side's owning thread.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindTwoWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(sourceProperty.Body));

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            null,
            bindingExpression);
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
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindTwoWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv)
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
            null,
            bindingExpression);
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
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, TVProp> OneWayBindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(value, null, null, out var converted);
                    return converted;
                },
            null,
            bindingExpression);
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
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, TOut> OneWayBindUnsafe<TViewModel, TView, TProp, TOut>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            selector,
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a view model property and a view property, resolving the property chains by reflection.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <returns>The binding; its change stream reports each value with the side it came from, and disposing it disconnects both directions.</returns>
    /// <remarks>
    /// The view model value is written to the view first, and the view's own current value is not written back.
    /// Writes to the view land on its owning thread when a registered view-thread invoker claims it.
    /// Values are converted with the registered converters, and a value they cannot convert is written as the default of the destination type.
    /// </remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create<TVMProp, TVProp>(
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(value, null, null, out var converted);
                    return converted;
                },
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVProp, TVMProp>(value, null, null, out var converted);
                    return converted;
                }),
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a view model property and a view property with conversion functions, resolving the property chains by reflection.</summary>
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
    /// <returns>The binding; its change stream reports each value with the side it came from, and disposing it disconnects both directions.</returns>
    /// <remarks>
    /// The view model value is written to the view first, and the view's own current value is not written back.
    /// Writes to the view land on its owning thread when a registered view-thread invoker claims it.
    /// </remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> viewModelToViewConverter,
        Func<TVProp, TVMProp> viewToViewModelConverter)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body));

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create(viewModelToViewConverter, viewToViewModelConverter),
            null,
            bindingExpression);
    }

    /// <summary>Binds a view model property and a view property in both directions, with an update stream driving one direction.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TVMProp">The view model property type.</typeparam>
    /// <typeparam name="TVProp">The view property type.</typeparam>
    /// <typeparam name="TDontCare">The ignored signal payload.</typeparam>
    /// <param name="view">The view to bind.</param>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="viewModelProperty">The view model property path.</param>
    /// <param name="viewProperty">The view property path.</param>
    /// <param name="signalViewUpdate">The update stream, or null to observe both properties' own notifications.</param>
    /// <param name="triggerUpdate">The direction the update stream drives; see <see cref="TriggerUpdate"/>.</param>
    /// <returns>The binding; its change stream reports each write with the side it came from, and disposing it disconnects both directions.</returns>
    /// <remarks>
    /// The view model value is written to the view first, once both sides are wired.
    /// Each signal then reads both sides at delivery on the view's owning thread, converts with the registered converters,
    /// and drops the write when the converted value equals the destination or cannot be converted.
    /// </remarks>
    [SuppressMessage("Design", "SST2309", Justification = "The compatibility overload requires ViewToViewModel as its optional enum default.")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IObservable<TDontCare>? signalViewUpdate,
        TriggerUpdate triggerUpdate = TriggerUpdate.ViewToViewModel)
        where TViewModel : class
        where TView : class, IViewFor =>
        RuntimeBindingFallback.Bind(view, viewModel, viewModelProperty, viewProperty, signalViewUpdate, triggerUpdate);

    /// <summary>Binds a view model property and a view property in both directions with explicit conversions, with an update stream driving one direction.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TVMProp">The view model property type.</typeparam>
    /// <typeparam name="TVProp">The view property type.</typeparam>
    /// <typeparam name="TDontCare">The ignored signal payload.</typeparam>
    /// <param name="view">The view to bind.</param>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="viewModelProperty">The view model property path.</param>
    /// <param name="viewProperty">The view property path.</param>
    /// <param name="viewModelToViewConverter">Converts a value written to the view.</param>
    /// <param name="viewToViewModelConverter">Converts a value written to the view model.</param>
    /// <param name="signalViewUpdate">The update stream, or null to observe both properties' own notifications.</param>
    /// <param name="triggerUpdate">The direction the update stream drives; see <see cref="TriggerUpdate"/>.</param>
    /// <returns>The binding; its change stream reports each write with the side it came from, and disposing it disconnects both directions.</returns>
    /// <remarks>
    /// The view model value is written to the view first, once both sides are wired.
    /// Each signal then reads both sides at delivery on the view's owning thread and drops the write when the converted value equals the destination.
    /// </remarks>
    [SuppressMessage("Design", "SST2309", Justification = "The compatibility overload requires ViewToViewModel as its optional enum default.")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> viewModelToViewConverter,
        Func<TVProp, TVMProp> viewToViewModelConverter,
        IObservable<TDontCare>? signalViewUpdate,
        TriggerUpdate triggerUpdate = TriggerUpdate.ViewToViewModel)
        where TViewModel : class
        where TView : class, IViewFor =>
        RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create(viewModelToViewConverter, viewToViewModelConverter),
            signalViewUpdate,
            triggerUpdate);
}
