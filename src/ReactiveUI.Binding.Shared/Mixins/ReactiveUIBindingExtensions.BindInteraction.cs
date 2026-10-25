// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Extension methods for binding interactions between a view and a view model.</summary>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Reported when no generated dispatch claimed a BindInteraction call site.</summary>
    private const string NoBindInteractionDispatchMessage =
        "No generated BindInteraction dispatch matched this call site. Use BindInteractionUnsafe to resolve the expression at run time.";

    /// <summary>Registers a task-based handler on the interaction a view model property holds, moving it to the new interaction when the property changes.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TInput">The type of the interaction's input.</typeparam>
    /// <typeparam name="TOutput">The type of the interaction's output.</typeparam>
    /// <param name="view">The view that provides the handler.</param>
    /// <param name="viewModel">The view model that exposes the interaction; null registers nothing.</param>
    /// <param name="propertyName">An expression that selects the interaction property on the view model.</param>
    /// <param name="handler">A task-based handler for the interaction.</param>
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, unregisters the handler and stops observing the property.</returns>
    /// <exception cref="InvalidOperationException">No generated BindInteraction dispatch matched this call site. Use BindInteractionUnsafe to resolve the expression at run time.</exception>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, Task> handler,
        [CallerArgumentExpression("propertyName")]
        string propertyNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoBindInteractionDispatchMessage);
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
    /// <param name="propertyNameExpression">The caller argument expression for <paramref name="propertyName"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, unregisters the handler and stops observing the property.</returns>
    /// <exception cref="InvalidOperationException">No generated BindInteraction dispatch matched this call site. Use BindInteractionUnsafe to resolve the expression at run time.</exception>
    public static IDisposable BindInteraction<TViewModel, TView, TInput, TOutput, TDontCare>(
        this TView view,
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> propertyName,
        Func<IInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler,
        [CallerArgumentExpression("propertyName")]
        string propertyNameExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
    {
        throw new InvalidOperationException(NoBindInteractionDispatchMessage);
    }
}
