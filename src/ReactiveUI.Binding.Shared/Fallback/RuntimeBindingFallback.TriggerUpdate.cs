// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Runtime bindings driven by direction signals and current property values.</summary>
public static partial class RuntimeBindingFallback
{
    /// <summary>Binds properties using converter registration and direction signals.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TVMProp">The view model property type.</typeparam>
    /// <typeparam name="TVProp">The view property type.</typeparam>
    /// <typeparam name="TDontCare">The ignored signal payload.</typeparam>
    /// <param name="view">The view to bind.</param>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="viewModelProperty">The view model property path.</param>
    /// <param name="viewProperty">The view property path.</param>
    /// <param name="signalViewUpdate">The update stream, or null to observe both properties.</param>
    /// <param name="triggerUpdate">The direction driven by the stream.</param>
    /// <returns>The connected two-way binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TVMProp, TVProp, TDontCare>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IObservable<TDontCare>? signalViewUpdate,
        TriggerUpdate triggerUpdate)
        where TViewModel : class
        where TView : class, IViewFor =>
        CreateTriggeredBinding(view, viewModel, viewModelProperty, viewProperty, null, signalViewUpdate, triggerUpdate);

    /// <summary>Binds properties using explicit conversions and direction signals.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TVMProp">The view model property type.</typeparam>
    /// <typeparam name="TVProp">The view property type.</typeparam>
    /// <typeparam name="TDontCare">The ignored signal payload.</typeparam>
    /// <param name="view">The view to bind.</param>
    /// <param name="viewModel">The view model to bind.</param>
    /// <param name="viewModelProperty">The view model property path.</param>
    /// <param name="viewProperty">The view property path.</param>
    /// <param name="conversions">The conversions applied before comparison and writing.</param>
    /// <param name="signalViewUpdate">The update stream, or null to observe both properties.</param>
    /// <param name="triggerUpdate">The direction driven by the stream.</param>
    /// <returns>The connected two-way binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    public static IReactiveBinding<TView, BindingChange> Bind<TViewModel, TView, TVMProp, TVProp, TDontCare>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        TwoWayConverterPair<TVMProp, TVProp> conversions,
        IObservable<TDontCare>? signalViewUpdate,
        TriggerUpdate triggerUpdate)
        where TViewModel : class
        where TView : class, IViewFor
    {
        ArgumentExceptionHelper.ThrowIfNull(conversions);
        ArgumentExceptionHelper.ThrowIfNull(conversions.Forward);
        ArgumentExceptionHelper.ThrowIfNull(conversions.Reverse);
        return CreateTriggeredBinding(view, viewModel, viewModelProperty, viewProperty, conversions, signalViewUpdate, triggerUpdate);
    }

    /// <summary>Connects one direction stream to the read, compare and write pipeline.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TVMProp">The view model property type.</typeparam>
    /// <typeparam name="TVProp">The view property type.</typeparam>
    /// <typeparam name="TDontCare">The ignored signal payload.</typeparam>
    /// <param name="view">The view root.</param>
    /// <param name="viewModel">The view model root.</param>
    /// <param name="viewModelProperty">The view model property path.</param>
    /// <param name="viewProperty">The view property path.</param>
    /// <param name="conversions">Explicit conversions, or null for converter registration.</param>
    /// <param name="signalViewUpdate">The optional update stream.</param>
    /// <param name="triggerUpdate">The direction driven by the stream.</param>
    /// <returns>The connected binding.</returns>
    [RequiresUnreferencedCode("Runtime binding fallback resolves the property chain by reflection.")]
    private static ReactiveBinding<TView, BindingChange> CreateTriggeredBinding<TViewModel, TView, TVMProp, TVProp, TDontCare>(
        TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        TwoWayConverterPair<TVMProp, TVProp>? conversions,
        IObservable<TDontCare>? signalViewUpdate,
        TriggerUpdate triggerUpdate)
        where TViewModel : class
        where TView : class, IViewFor
    {
        ArgumentExceptionHelper.ThrowIfNull(view);
        ArgumentExceptionHelper.ThrowIfNull(viewModelProperty);
        ArgumentExceptionHelper.ThrowIfNull(viewProperty);

        var modelSignals = new MapSignal<TVMProp, bool>(ObserveViewModel(view, viewModel, viewModelProperty), static _ => true);
        var viewSignals = new MapSignal<TVProp, bool>(RuntimeObservationFallback.WhenChanged(view, viewProperty), static _ => false);
        var source = CreateTriggerSource(modelSignals, viewSignals, signalViewUpdate, triggerUpdate);

        // The model side is read and written through the view's own view model when the view holds one of this
        // type, the same root the observation above follows.
        var (modelRoot, modelPath) = view is IViewFor<TViewModel>
            ? ((object)view, RootAtViewModel(viewModelProperty).Body)
            : ((object)viewModel!, viewModelProperty.Body);
        var plan = new TriggeredBindingPlan<TVMProp, TVProp>(
            modelRoot,
            view,
            new List<Expression>(Reflection.Rewrite(modelPath).GetExpressionChain()),
            new List<Expression>(Reflection.Rewrite(viewProperty.Body).GetExpressionChain()),
            conversions);
        var values = BindingSchedulers.ObserveOnViewThread(new InitialBindingSignal(source), view).Choose(plan.Project);
        var changes = new Signal<BindingChange>();
        var subscription = BindingErrors.Subscribe(
            values,
            change =>
            {
                plan.Write(change);
                changes.OnNext(change);
            },
            Reflection.ExpressionToPropertyNames(Reflection.Rewrite(viewModelProperty.Body)));
        return new(view, changes, BindingDirection.TwoWay, new MultipleDisposable(subscription, new ActionDisposable(changes.OnCompleted)));
    }

    /// <summary>Selects the notifications belonging to each direction.</summary>
    /// <typeparam name="T">The ignored update payload.</typeparam>
    /// <param name="modelSignals">View model notifications, including the initial snapshot.</param>
    /// <param name="viewSignals">View notifications, including the initial snapshot.</param>
    /// <param name="updates">The optional update stream.</param>
    /// <param name="direction">The direction driven by the update stream.</param>
    /// <returns>The combined direction signals.</returns>
    private static MergeSignal<bool> CreateTriggerSource<T>(
        IObservable<bool> modelSignals,
        IObservable<bool> viewSignals,
        IObservable<T>? updates,
        TriggerUpdate direction)
    {
        if (updates is null)
        {
            return new(modelSignals, viewSignals);
        }

        return direction == TriggerUpdate.ViewToViewModel
            ? new MergeSignal<bool>(modelSignals, new MapSignal<T, bool>(updates, static _ => false))
            : new MergeSignal<bool>(
                new MergeSignal<bool>(modelSignals.Take(1), new MapSignal<T, bool>(updates, static _ => true)),
                viewSignals);
    }
}
