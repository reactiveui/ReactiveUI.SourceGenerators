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

/// <summary>Resolves a BindInteraction expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>BindInteraction</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Registers a task-based handler on the interaction a view model property holds, moving it to the new interaction when the property changes.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction; null registers nothing.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">A task-based handler for the interaction.</param>
    /// <returns>A disposable that, when disposed, unregisters the handler and stops observing the property.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindInteractionUnsafe<TViewModel, TView, TInput, TOutput>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, Task> handler)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(propertyName.Body));

        return RuntimeInteractionFallback.BindInteraction(
            viewModel,
            propertyName,
            interaction => interaction.RegisterHandler(handler),
            bindingExpression);
    }

    /// <summary>Registers an observable-based handler on the interaction a view model property holds, moving it to the new interaction when the property changes.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <typeparam name="TDontCare">The signal type of the observable handler.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction; null registers nothing.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">An observable-based handler for the interaction.</param>
    /// <returns>A disposable that, when disposed, unregisters the handler and stops observing the property.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindInteractionUnsafe<TViewModel, TView, TInput, TOutput, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(Reflection.Rewrite(propertyName.Body));

        return RuntimeInteractionFallback.BindInteraction(
            viewModel,
            propertyName,
            interaction => interaction.RegisterHandler(handler),
            bindingExpression);
    }
}
