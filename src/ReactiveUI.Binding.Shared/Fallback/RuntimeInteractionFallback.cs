// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>Registers an interaction handler against a property the runtime expression engine resolves.</summary>
/// <remarks>
/// Reached where the generator could not serve the call site - a view model it cannot name, or a selector that is
/// not an inline lambda. How the handler runs is the generated path's: both hand it to the interaction itself,
/// and only how the interaction is found differs.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeInteractionFallback
{
    /// <summary>Keeps a handler registered against whichever interaction the observed property holds.</summary>
    /// <typeparam name="TViewModel">The type declaring the interaction property.</typeparam>
    /// <typeparam name="TInput">The type the interaction takes.</typeparam>
    /// <typeparam name="TOutput">The type the interaction produces.</typeparam>
    /// <param name="viewModel">The object declaring the interaction property.</param>
    /// <param name="interactionProperty">The property holding the interaction to handle.</param>
    /// <param name="register">Registers the caller's handler against one interaction.</param>
    /// <param name="bindingExpression">The bound expression, named when the observation faults.</param>
    /// <returns>A disposable that, when disposed, unregisters the handler and stops observing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="interactionProperty"/> or <paramref name="register"/> is null.</exception>
    /// <remarks>
    /// The registration follows the property: when the property holds a different interaction, the handler is
    /// unregistered from the interaction it replaced and registered on the new one, and a null interaction leaves
    /// nothing registered. A null view model registers nothing rather than faulting the call.
    /// </remarks>
    [RequiresUnreferencedCode("Runtime interaction fallback resolves the property chain by reflection.")]
    public static IDisposable BindInteraction<TViewModel, TInput, TOutput>(
        TViewModel? viewModel,
        Expression<Func<TViewModel, IInteraction<TInput, TOutput>>> interactionProperty,
        Func<IInteraction<TInput, TOutput>, IDisposable> register,
        string bindingExpression)
        where TViewModel : class
    {
        ArgumentExceptionHelper.ThrowIfNull(interactionProperty);
        ArgumentExceptionHelper.ThrowIfNull(register);

        if (viewModel is null)
        {
            return EmptyDisposable.Instance;
        }

        var registration = new SwapDisposable();
        var observation = BindingErrors.Subscribe(
            RuntimeObservationFallback.WhenAnyValue(viewModel, interactionProperty),
            interaction => registration.Disposable = interaction is null
                ? EmptyDisposable.Instance
                : register(interaction),
            bindingExpression);

        return new MultipleDisposable(observation, registration);
    }
}
