// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Fluent builder for registering view-to-view-model mappings on a <see cref="DefaultViewLocator"/>.</summary>
[DebuggerDisplay("ViewMappingBuilder: Locator = {_locator}")]
public sealed class ViewMappingBuilder
{
    /// <summary>The view locator to register mappings on.</summary>
    private readonly DefaultViewLocator _locator;

    /// <summary>Initializes a new instance of the <see cref="ViewMappingBuilder"/> class.</summary>
    /// <param name="locator">The view locator to register mappings on.</param>
    internal ViewMappingBuilder(DefaultViewLocator locator) => _locator = locator;

    /// <summary>Maps a view model type to a view type constructed with its parameterless constructor, replacing an existing mapping for the same view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must have a parameterless constructor.</typeparam>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ViewMappingBuilder Map<TViewModel, TView>()
        where TViewModel : class
        where TView : IViewFor, new() => Map<TViewModel, TView>(null);

    /// <summary>Maps a view model type and contract to a view type constructed with its parameterless constructor, replacing an existing mapping for the same pair.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must have a parameterless constructor.</typeparam>
    /// <param name="contract">The contract the mapping is registered under; null registers the default mapping.</param>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel, TView>(string? contract)
        where TViewModel : class
        where TView : IViewFor, new()
    {
        _locator.Map<TViewModel, TView>(contract);
        return this;
    }

    /// <summary>Maps a view model type to a view created by a factory, replacing an existing mapping for the same view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <returns>This builder for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is null.</exception>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ViewMappingBuilder Map<TViewModel>(Func<IViewFor> factory)
        where TViewModel : class => Map<TViewModel>(factory, null);

    /// <summary>Maps a view model type and contract to a view created by a factory, replacing an existing mapping for the same pair.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <param name="contract">The contract the mapping is registered under; null registers the default mapping.</param>
    /// <returns>This builder for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is null.</exception>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder Map<TViewModel>(Func<IViewFor> factory, string? contract)
        where TViewModel : class
    {
        _locator.Map<TViewModel>(factory, contract);
        return this;
    }

    /// <summary>Maps a view model type to a view the service locator creates, replacing an existing mapping for the same view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type, as registered in the service locator.</typeparam>
    /// <returns>This builder for chaining.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ViewMappingBuilder MapFromServiceLocator<TViewModel, TView>()
        where TViewModel : class
        where TView : class, IViewFor => MapFromServiceLocator<TViewModel, TView>(null);

    /// <summary>Maps a view model type and contract to a view the service locator creates, replacing an existing mapping for the same pair.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type, as registered in the service locator.</typeparam>
    /// <param name="contract">The contract the mapping is registered under; null registers the default mapping.</param>
    /// <returns>This builder for chaining.</returns>
    /// <remarks>
    /// The service locator is asked for <typeparamref name="TView"/> with no contract. To tell apart views registered
    /// under one type, such as <c>IViewFor&lt;TViewModel&gt;</c>, pass the service locator contract to
    /// <see cref="MapFromServiceLocator{TViewModel, TView}(string?, string?)"/>. The view is asked for each time the
    /// mapping resolves, so the service locator's lifetime for it applies. A view that is not registered throws
    /// <see cref="InvalidOperationException"/> when the mapping resolves, rather than resolving to nothing.
    /// </remarks>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ViewMappingBuilder MapFromServiceLocator<TViewModel, TView>(string? contract)
        where TViewModel : class
        where TView : class, IViewFor => MapFromServiceLocator<TViewModel, TView>(contract, null);

    /// <summary>
    /// Maps a view model type and contract to a view the service locator creates under its own contract, replacing an
    /// existing mapping for the same pair.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type, as registered in the service locator.</typeparam>
    /// <param name="contract">The contract the mapping is registered under; null registers the default mapping.</param>
    /// <param name="serviceContract">The contract the view is registered under in the service locator; null asks for the registration with no contract.</param>
    /// <returns>This builder for chaining.</returns>
    /// <remarks>
    /// The view is asked for each time the mapping resolves, so the service locator's lifetime for it applies. A view
    /// that is not registered under <paramref name="serviceContract"/> throws <see cref="InvalidOperationException"/>
    /// when the mapping resolves, rather than resolving to nothing or to a view registered under another contract.
    /// </remarks>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public ViewMappingBuilder MapFromServiceLocator<TViewModel, TView>(string? contract, string? serviceContract)
        where TViewModel : class
        where TView : class, IViewFor
    {
        _locator.Map<TViewModel>(
            () => AppLocator.Current.GetService<TView>(serviceContract)
                ?? throw new InvalidOperationException(serviceContract is null
                    ? $"View {typeof(TView).Name} is not registered in the service locator."
                    : $"View {typeof(TView).Name} is not registered in the service locator under contract '{serviceContract}'."),
            contract);
        return this;
    }
}
