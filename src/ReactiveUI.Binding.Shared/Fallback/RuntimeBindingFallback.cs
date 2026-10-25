// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Drives a binding through the runtime observation engine instead of the generated one.</summary>
/// <remarks>
/// Unsafe entry points resolve property paths through the runtime observation engine and cached expression
/// chains. Their trimming annotations make member discovery explicit at the call site.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class RuntimeBindingFallback
{
    /// <summary>Binds a source property one way onto a target property of the same type.</summary>
    /// <typeparam name="TSource">The type declaring the observed property.</typeparam>
    /// <typeparam name="TTarget">The type declaring the written property.</typeparam>
    /// <typeparam name="TProp">The type of the value carried across the binding.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="target">The object to write to.</param>
    /// <param name="sourceProperty">The property to observe.</param>
    /// <param name="targetProperty">The property to write.</param>
    /// <param name="scheduler">The sequencer the write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects the binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindOneWay<TSource, TTarget, TProp>(
        TSource source,
        TTarget target,
        Expression<Func<TSource, TProp>> sourceProperty,
        Expression<Func<TTarget, TProp>> targetProperty,
        ISequencer? scheduler,
        string bindingExpression)
        where TSource : class =>
        Write(
            Schedule(RuntimeObservationFallback.WhenChanged(source, sourceProperty), scheduler, target),
            target,
            targetProperty,
            bindingExpression);

    /// <summary>Binds a source property one way onto a target property of another type, applying a conversion.</summary>
    /// <typeparam name="TSource">The type declaring the observed property.</typeparam>
    /// <typeparam name="TTarget">The type declaring the written property.</typeparam>
    /// <typeparam name="TSourceProp">The type of the observed property.</typeparam>
    /// <typeparam name="TTargetProp">The type of the written property.</typeparam>
    /// <param name="source">The object to observe.</param>
    /// <param name="target">The object to write to.</param>
    /// <param name="sourceProperty">The property to observe.</param>
    /// <param name="targetProperty">The property to write.</param>
    /// <param name="conversion">Converts the observed value to the written one.</param>
    /// <param name="scheduler">The sequencer the write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects the binding.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="conversion"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindOneWay<TSource, TTarget, TSourceProp, TTargetProp>(
        TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversion,
        ISequencer? scheduler,
        string bindingExpression)
        where TSource : class
    {
        ArgumentExceptionHelper.ThrowIfNull(conversion);

        return Write(
            Schedule(
                new MapSignal<TSourceProp, TTargetProp>(
                    RuntimeObservationFallback.WhenChanged(source, sourceProperty),
                    conversion),
                scheduler,
                target),
            target,
            targetProperty,
            bindingExpression);
    }

    /// <summary>Binds a source and a target property of the same type to each other.</summary>
    /// <typeparam name="TSource">The type declaring the source property.</typeparam>
    /// <typeparam name="TTarget">The type declaring the target property.</typeparam>
    /// <typeparam name="TProp">The type of the value carried across the binding.</typeparam>
    /// <param name="source">The first object of the binding.</param>
    /// <param name="target">The second object of the binding.</param>
    /// <param name="sourceProperty">The property on <paramref name="source"/>.</param>
    /// <param name="targetProperty">The property on <paramref name="target"/>.</param>
    /// <param name="scheduler">The sequencer the target write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects both directions.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable BindTwoWay<TSource, TTarget, TProp>(
        TSource source,
        TTarget target,
        Expression<Func<TSource, TProp>> sourceProperty,
        Expression<Func<TTarget, TProp>> targetProperty,
        ISequencer? scheduler,
        string bindingExpression)
        where TSource : class
        where TTarget : class =>
        JoinBothDirections(
            ScheduleLatest(RuntimeObservationFallback.WhenChanged(source, sourceProperty), scheduler, target),
            RuntimeObservationFallback.WhenChanged(target, targetProperty),
            source,
            target,
            sourceProperty,
            targetProperty,
            bindingExpression);

    /// <summary>Binds a source and a target property of differing types to each other, converting in both directions.</summary>
    /// <typeparam name="TSource">The type declaring the source property.</typeparam>
    /// <typeparam name="TTarget">The type declaring the target property.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The first object of the binding.</param>
    /// <param name="target">The second object of the binding.</param>
    /// <param name="sourceProperty">The property on <paramref name="source"/>.</param>
    /// <param name="targetProperty">The property on <paramref name="target"/>.</param>
    /// <param name="converters">Converts source to target, and target back to source.</param>
    /// <param name="scheduler">The sequencer the target write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects both directions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="converters"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindTwoWay<TSource, TTarget, TSourceProp, TTargetProp>(
        TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        TwoWayConverterPair<TSourceProp, TTargetProp> converters,
        ISequencer? scheduler,
        string bindingExpression)
        where TSource : class
        where TTarget : class
    {
        ArgumentExceptionHelper.ThrowIfNull(converters);

        return JoinBothDirections(
            ScheduleLatest(
                new MapSignal<TSourceProp, TTargetProp>(
                    RuntimeObservationFallback.WhenChanged(source, sourceProperty),
                    converters.Forward),
                scheduler,
                target),
            new MapSignal<TTargetProp, TSourceProp>(
                RuntimeObservationFallback.WhenChanged(target, targetProperty),
                converters.Reverse),
            source,
            target,
            sourceProperty,
            targetProperty,
            bindingExpression);
    }

    /// <summary>Binds a view-model property one way onto a view property of the same type.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TView">The type declaring the view property.</typeparam>
    /// <typeparam name="TProp">The type of the value carried across the binding.</typeparam>
    /// <param name="view">The view being written to.</param>
    /// <param name="viewModel">The view model being observed.</param>
    /// <param name="viewModelProperty">The property to observe.</param>
    /// <param name="viewProperty">The property to write.</param>
    /// <param name="scheduler">The sequencer the write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, which disconnects when disposed.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TView, TProp> OneWayBind<TViewModel, TView, TProp>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TProp>> viewProperty,
        ISequencer? scheduler,
        string bindingExpression)
        where TViewModel : class
        where TView : IViewFor =>
        OneWayBinding(
            view,
            viewProperty,
            Schedule(ObserveViewModel(view, viewModel, viewModelProperty), scheduler, view),
            bindingExpression);

    /// <summary>Binds a view-model property one way onto a view property of another type, applying a conversion.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TView">The type declaring the view property.</typeparam>
    /// <typeparam name="TViewModelProp">The type of the view-model property.</typeparam>
    /// <typeparam name="TViewProp">The type of the view property.</typeparam>
    /// <param name="view">The view being written to.</param>
    /// <param name="viewModel">The view model being observed.</param>
    /// <param name="viewModelProperty">The property to observe.</param>
    /// <param name="viewProperty">The property to write.</param>
    /// <param name="conversion">Converts the observed value to the view's type.</param>
    /// <param name="scheduler">The sequencer the write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, which disconnects when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="conversion"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, TViewProp> OneWayBind<TViewModel, TView, TViewModelProp, TViewProp>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TViewModelProp>> viewModelProperty,
        Expression<Func<TView, TViewProp>> viewProperty,
        Func<TViewModelProp, TViewProp> conversion,
        ISequencer? scheduler,
        string bindingExpression)
        where TViewModel : class
        where TView : IViewFor
    {
        ArgumentExceptionHelper.ThrowIfNull(conversion);

        return OneWayBinding(
            view,
            viewProperty,
            Schedule(
                new MapSignal<TViewModelProp, TViewProp>(
                    ObserveViewModel(view, viewModel, viewModelProperty),
                    conversion),
                scheduler,
                view),
            bindingExpression);
    }

    /// <summary>Binds a view-model property and a view property of the same type to each other.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TView">The type declaring the view property.</typeparam>
    /// <typeparam name="TProp">The type of the value carried across the binding.</typeparam>
    /// <param name="view">The view side of the binding.</param>
    /// <param name="viewModel">The view-model side of the binding.</param>
    /// <param name="viewModelProperty">The property on <paramref name="viewModel"/>.</param>
    /// <param name="viewProperty">The property on <paramref name="view"/>.</param>
    /// <param name="scheduler">The sequencer the view write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, which disconnects both directions when disposed.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TProp>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TProp>> viewProperty,
        ISequencer? scheduler,
        string bindingExpression)
        where TViewModel : class
        where TView : class, IViewFor =>
        TwoWayBinding(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            ScheduleLatest(ObserveViewModel(view, viewModel, viewModelProperty), scheduler, view),
            RuntimeObservationFallback.WhenChanged(view, viewProperty).Skip(1),
            bindingExpression);

    /// <summary>Binds a view-model property and a view property of differing types, converting in both directions.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TView">The type declaring the view property.</typeparam>
    /// <typeparam name="TViewModelProp">The type of the view-model property.</typeparam>
    /// <typeparam name="TViewProp">The type of the view property.</typeparam>
    /// <param name="view">The view side of the binding.</param>
    /// <param name="viewModel">The view-model side of the binding.</param>
    /// <param name="viewModelProperty">The property on <paramref name="viewModel"/>.</param>
    /// <param name="viewProperty">The property on <paramref name="view"/>.</param>
    /// <param name="converters">Converts view model to view, and view back to view model.</param>
    /// <param name="scheduler">The sequencer the view write is delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, which disconnects both directions when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="converters"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TViewModelProp, TViewProp>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TViewModelProp>> viewModelProperty,
        Expression<Func<TView, TViewProp>> viewProperty,
        TwoWayConverterPair<TViewModelProp, TViewProp> converters,
        ISequencer? scheduler,
        string bindingExpression)
        where TViewModel : class
        where TView : class, IViewFor
    {
        ArgumentExceptionHelper.ThrowIfNull(converters);

        return TwoWayBinding(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            ScheduleLatest(
                new MapSignal<TViewModelProp, TViewProp>(
                    ObserveViewModel(view, viewModel, viewModelProperty),
                    converters.Forward),
                scheduler,
                view),
            new MapSignal<TViewProp, TViewModelProp>(
                RuntimeObservationFallback.WhenChanged(view, viewProperty).Skip(1),
                converters.Reverse),
            bindingExpression);
    }

    /// <summary>Writes every value a sequence produces into a property, converting it on the way.</summary>
    /// <typeparam name="TValue">The type the sequence produces.</typeparam>
    /// <typeparam name="TTarget">The type declaring the written property.</typeparam>
    /// <typeparam name="TTargetValue">The type of the written property.</typeparam>
    /// <param name="source">The sequence driving the writes.</param>
    /// <param name="target">The object declaring the written property.</param>
    /// <param name="targetProperty">The property to write.</param>
    /// <param name="conversionHint">An optional hint handed to the converter.</param>
    /// <param name="converterOverride">A converter that takes precedence over the registered ones.</param>
    /// <param name="scheduler">The scheduler the writes are delivered on, or null to write on the thread that owns it.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that, when disposed, stops writing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="targetProperty"/> is null.</exception>
    /// <remarks>
    /// A value the converter refuses is written as the target type's default, which is what the generated path
    /// does with the same converter, so a call site behaves the same whether or not it was claimed.
    /// A null target holds no property to write, so the values are dropped rather than faulting the sequence.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue>> targetProperty,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        ISequencer? scheduler,
        string bindingExpression)
        where TTarget : class
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(targetProperty);

        if (target is null)
        {
            return EmptyDisposable.Instance;
        }

        var converted = new MapSignal<TValue, TTargetValue>(
            source,
            value =>
            {
                _ = RuntimeBindingConverter.TryConvert<TValue, TTargetValue>(
                    value,
                    conversionHint,
                    converterOverride,
                    out var result);
                return result;
            });

        return Write(Schedule(converted, scheduler, target), target, targetProperty, bindingExpression);
    }

    /// <summary>Roots a view-model property path at the view model the view currently holds.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TProp">The type of the view-model property.</typeparam>
    /// <param name="viewModelProperty">The path from the view model.</param>
    /// <returns>The same path, read from the view through <see cref="IViewFor{T}.ViewModel"/>.</returns>
    /// <remarks>
    /// A view-first binding follows the view's view model rather than the instance handed to it, the way the
    /// generated path does: replacing the view model rebinds, and a view without one yet waits for it. Rooting the
    /// path at the view lets the runtime chain do that, for observation and for the write back alike. The typed
    /// property is used so the chain carries the view model's type without a cast the chain would strip.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    internal static Expression<Func<IViewFor<TViewModel>, TProp>> RootAtViewModel<TViewModel, TProp>(Expression<Func<TViewModel, TProp>> viewModelProperty)
        where TViewModel : class
    {
        ArgumentExceptionHelper.ThrowIfNull(viewModelProperty);

        var view = Expression.Parameter(typeof(IViewFor<TViewModel>), "view");
        var viewModel = Expression.Property(view, typeof(IViewFor<TViewModel>).GetProperty(nameof(IViewFor<>.ViewModel))!);
        var body = new ParameterReplacer(viewModelProperty.Parameters[0], viewModel).Visit(viewModelProperty.Body);
        return Expression.Lambda<Func<IViewFor<TViewModel>, TProp>>(body, view);
    }

    /// <summary>Observes a view-model property through the view model the view holds, or the one handed in.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TProp">The type of the view-model property.</typeparam>
    /// <param name="view">The view holding the view model.</param>
    /// <param name="viewModel">The view model handed to the binding, used when the view does not hold one of this type.</param>
    /// <param name="viewModelProperty">The path from the view model.</param>
    /// <returns>The property's values, following the view's view model as it is replaced.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static IObservable<TProp> ObserveViewModel<TView, TViewModel, TProp>(TView view, TViewModel? viewModel, Expression<Func<TViewModel, TProp>> viewModelProperty)
        where TView : IViewFor
        where TViewModel : class =>
        view is IViewFor<TViewModel> typedView
            ? RuntimeObservationFallback.WhenChanged(typedView, RootAtViewModel(viewModelProperty))
            : RuntimeObservationFallback.WhenChanged(viewModel!, viewModelProperty);

    /// <summary>Writes values back to the view model the view holds, or the one handed in.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TProp">The type of the view-model property.</typeparam>
    /// <param name="values">The values to write.</param>
    /// <param name="view">The view holding the view model.</param>
    /// <param name="viewModel">The view model handed to the binding, used when the view does not hold one of this type.</param>
    /// <param name="viewModelProperty">The path from the view model.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that stops writing.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static IDisposable WriteViewModel<TView, TViewModel, TProp>(
        IObservable<TProp> values,
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        string bindingExpression)
        where TView : IViewFor
        where TViewModel : class =>
        view is IViewFor<TViewModel> typedView
            ? Write(values, typedView, RootAtViewModel(viewModelProperty), bindingExpression)
            : Write(values, viewModel!, viewModelProperty, bindingExpression);

    /// <summary>Wraps a two-way pair of writes as the binding value the view-first APIs hand back.</summary>
    /// <typeparam name="TViewModel">The type declaring the view-model property.</typeparam>
    /// <typeparam name="TView">The type declaring the view property.</typeparam>
    /// <typeparam name="TViewModelProp">The type of the view-model property.</typeparam>
    /// <typeparam name="TViewProp">The type of the view property.</typeparam>
    /// <param name="view">The view side of the binding.</param>
    /// <param name="viewModel">The view-model side of the binding.</param>
    /// <param name="viewModelProperty">The property on <paramref name="viewModel"/>.</param>
    /// <param name="viewProperty">The property on <paramref name="view"/>.</param>
    /// <param name="toView">The values written to the view, already converted and scheduled.</param>
    /// <param name="fromView">The values written back to the view model, already converted and skipped.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, whose change stream tags which side each value came from.</returns>
    /// <remarks>
    /// The change stream a two-way binding exposes is not either property's values but a merge of both,
    /// tagged with which side moved, so a consumer can tell an echo from an edit.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static ReactiveBinding<TView, BindingChange> TwoWayBinding<TViewModel, TView, TViewModelProp, TViewProp>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TViewModelProp>> viewModelProperty,
        Expression<Func<TView, TViewProp>> viewProperty,
        IObservable<TViewProp> toView,
        IObservable<TViewModelProp> fromView,
        string bindingExpression)
        where TViewModel : class
        where TView : IViewFor
    {
        var toViewWrite = Write(toView, view, viewProperty, bindingExpression);
        var toViewModelWrite = WriteViewModel(fromView, view, viewModel, viewModelProperty, bindingExpression);

        var changed = new MergeSignal<BindingChange>(
            new MapSignal<TViewProp, BindingChange>(toView, static v => new BindingChange(v, true)),
            new MapSignal<TViewModelProp, BindingChange>(fromView, static v => new BindingChange(v, false)));

        return new(
            view,
            changed,
            BindingDirection.TwoWay,
            new MultipleDisposable(toViewWrite, toViewModelWrite));
    }

    /// <summary>Wraps a one-way write as the binding value the view-first APIs hand back.</summary>
    /// <typeparam name="TView">The type declaring the written property.</typeparam>
    /// <typeparam name="TProp">The type of the value written.</typeparam>
    /// <param name="view">The view being written to.</param>
    /// <param name="viewProperty">The property to write.</param>
    /// <param name="values">The values to write, already scheduled.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>The binding, which disconnects when disposed.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReactiveBinding<TView, TProp> OneWayBinding<TView, TProp>(
        TView view,
        Expression<Func<TView, TProp>> viewProperty,
        IObservable<TProp> values,
        string bindingExpression)
        where TView : IViewFor =>
        new(
            view,
            values,
            BindingDirection.OneWay,
            Write(values, view, viewProperty, bindingExpression));

    /// <summary>Routes a sequence onto the sequencer a binding writes on.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="values">The values feeding a write.</param>
    /// <param name="scheduler">The sequencer to use, or null to ask which thread owns the written object.</param>
    /// <param name="target">The object the write lands on, which is what owns the thread it lands from.</param>
    /// <returns>The sequence, observed on the chosen sequencer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<T> Schedule<T>(IObservable<T> values, ISequencer? scheduler, object? target)
    {
        if (scheduler is null)
        {
            return BindingSchedulers.ObserveOnViewThread(values, target);
        }

        return IsImmediate(scheduler) ? values : new WitnessOnSignal<T>(values, scheduler);
    }

    /// <summary>Routes the values a two-way binding writes onto a sequencer, delivering only the latest one that waits.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="values">The values feeding a write.</param>
    /// <param name="scheduler">The sequencer to use, or null to ask which thread owns the written object.</param>
    /// <param name="target">The object the write lands on, which is what owns the thread it lands from.</param>
    /// <returns>The sequence, observed on the chosen sequencer.</returns>
    /// <remarks>
    /// Each write raises the other side's change. A queue that replays every value writes an older value back
    /// over a newer one, and the two sides then hand each other stale values without end.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<T> ScheduleLatest<T>(IObservable<T> values, ISequencer? scheduler, object? target)
    {
        if (scheduler is null)
        {
            return BindingSchedulers.ObserveOnViewThread(values, target);
        }

        return IsImmediate(scheduler) ? values : new ViewThreadObservable<T>(values, scheduler);
    }

    /// <summary>Determines whether a sequencer runs work on the calling thread at once.</summary>
    /// <param name="scheduler">The sequencer to test.</param>
    /// <returns><see langword="true"/> for the immediate sequencer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsImmediate(ISequencer scheduler) =>
#if REACTIVE_SHIM
        scheduler == Scheduler.Immediate;
#else
        scheduler == Sequencer.Immediate;
#endif

    /// <summary>Wires a forward and a reverse write, dropping the target's initial value so it does not echo back.</summary>
    /// <typeparam name="TSource">The type declaring the source property.</typeparam>
    /// <typeparam name="TTarget">The type declaring the target property.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="forward">The values to write onto the target, already scheduled.</param>
    /// <param name="reverse">The values to write back onto the source.</param>
    /// <param name="source">The first object of the binding.</param>
    /// <param name="target">The second object of the binding.</param>
    /// <param name="sourceProperty">The property on <paramref name="source"/>.</param>
    /// <param name="targetProperty">The property on <paramref name="target"/>.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects both directions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="sourceProperty"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static MultipleDisposable JoinBothDirections<TSource, TTarget, TSourceProp, TTargetProp>(
        IObservable<TTargetProp> forward,
        IObservable<TSourceProp> reverse,
        TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        string bindingExpression)
    {
        var toTarget = Write(forward, target, targetProperty, bindingExpression);

        // The target's own current value is not a change the source asked for, so it is dropped rather
        // than written straight back over the value that just seeded it.
        var toSource = Write(reverse.Skip(1), source, sourceProperty, bindingExpression);

        return new(toTarget, toSource);
    }

    /// <summary>Delivers a sequence of values onto a property of the written object.</summary>
    /// <typeparam name="TWritten">The type declaring the written property.</typeparam>
    /// <typeparam name="TValue">The type of the value written.</typeparam>
    /// <param name="values">The values to write, already scheduled.</param>
    /// <param name="written">The object to write to.</param>
    /// <param name="property">The property to write.</param>
    /// <param name="bindingExpression">The bound expression, named when a write faults.</param>
    /// <returns>A disposable that disconnects the write.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="written"/> or <paramref name="property"/> is null.</exception>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static IDisposable Write<TWritten, TValue>(
        IObservable<TValue> values,
        TWritten written,
        Expression<Func<TWritten, TValue>> property,
        string bindingExpression)
    {
        ArgumentExceptionHelper.ThrowIfNull(written);
        ArgumentExceptionHelper.ThrowIfNull(property);

        // The chain is a property of the expression, not of any one value, so it is resolved once here
        // rather than on every write.
        var chain = new List<Expression>(Reflection.Rewrite(property.Body).GetExpressionChain());

        return BindingErrors.Subscribe(
            values,
            value => _ = Reflection.TrySetValueToPropertyChain(written, chain, value),
            bindingExpression);
    }

    /// <summary>Replaces one parameter of an expression with another expression.</summary>
    /// <param name="parameter">The parameter to replace.</param>
    /// <param name="replacement">The expression that stands in for it.</param>
    private sealed class ParameterReplacer(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
    {
        /// <inheritdoc/>
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == parameter ? replacement : base.VisitParameter(node);
    }
}
