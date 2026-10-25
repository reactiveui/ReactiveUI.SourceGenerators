// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Convenience overloads for <see cref="IViewLocator"/> that supply the default (null) contract.
/// Provided as overloads rather than optional parameters so the interface stays free of optional parameters.
/// </summary>
public static class ViewLocatorMixins
{
    /// <summary>The reason <c>ResolveViewUnsafe</c> needs dynamic code, reported at each call site.</summary>
    internal const string ResolveViewUnsafeMessage =
        "ResolveViewUnsafe closes IViewFor<> over the view model's runtime type to ask the service locator. Use ResolveView with a generated view or a Map registration to stay ahead-of-time safe.";

    /// <summary>Provides ResolveView extension members for <paramref name="locator"/>.</summary>
    /// <param name="locator">The view locator.</param>
    extension(IViewLocator locator)
    {
        /// <summary>Resolves a view for the specified view model type using the default contract.</summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="viewModel">The view model instance to resolve a view for.</param>
        /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
        public IViewFor? ResolveView<TViewModel>(TViewModel viewModel)
            where TViewModel : class
        {
            ArgumentExceptionHelper.ThrowIfNull(locator);
            return locator.ResolveView(viewModel, null);
        }

        /// <summary>Resolves a view for a view model type under the default contract, without a view model instance.</summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the view model type.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IViewFor<TViewModel>? ResolveView<TViewModel>()
            where TViewModel : class => locator.ResolveView<TViewModel>(contract: null);

        /// <summary>Resolves a view for a view model type under a contract, without a view model instance.</summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="contract">The contract to resolve under, or null for the default view.</param>
        /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
        /// <remarks>
        /// The default locator asks its explicit mappings and then the service locator. Any other locator has no
        /// type-only lookup of its own, so this asks the service locator for <see cref="IViewFor{T}"/> directly.
        /// Pass the contract by name, <c>contract: null</c>, when it is a literal null: a bare <c>null</c> could
        /// also be the view model of <c>ResolveView&lt;TViewModel&gt;(TViewModel)</c>.
        /// </remarks>
        [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the view model type.")]
        public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract)
            where TViewModel : class
        {
            ArgumentExceptionHelper.ThrowIfNull(locator);
            return locator is DefaultViewLocator defaultLocator
                ? defaultLocator.ResolveView<TViewModel>(contract)
                : AppLocator.Current.GetService<IViewFor<TViewModel>>(string.IsNullOrEmpty(contract) ? null : contract);
        }

        /// <summary>Resolves a view for a view model instance under the default contract, without building any type at run time.</summary>
        /// <param name="viewModel">The view model instance to resolve a view for.</param>
        /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
        /// <remarks>
        /// This calls <see cref="IViewLocator.ResolveView(object?, string?)"/> with a null contract. It is safe to call
        /// from a trimmed or native AOT application.
        /// </remarks>
        /// <example>
        /// <code>
        /// object viewModel = navigationStack.Peek();
        /// var view = ViewLocator.GetCurrent().ResolveView(viewModel);
        /// </code>
        /// </example>
        public IViewFor? ResolveView(object? viewModel)
        {
            ArgumentExceptionHelper.ThrowIfNull(locator);
            return locator.ResolveView(viewModel, null);
        }

        /// <summary>Resolves a view for a view model instance under the default contract, and falls back to steps that build a type at run time.</summary>
        /// <param name="viewModel">The view model instance to resolve a view for.</param>
        /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
        /// <remarks>
        /// This calls <see cref="IViewLocator.ResolveViewUnsafe(object?, string?)"/> with a null contract. Prefer
        /// <c>ResolveView(object?)</c> with a generated view or a <c>Map</c> registration.
        /// </remarks>
        /// <example>
        /// <code>
        /// // The view is registered only as IViewFor&lt;TodoViewModel&gt; in the service locator.
        /// object viewModel = new TodoViewModel();
        /// var view = ViewLocator.GetCurrent().ResolveViewUnsafe(viewModel);
        /// </code>
        /// </example>
        [RequiresDynamicCode(ResolveViewUnsafeMessage)]
        public IViewFor? ResolveViewUnsafe(object? viewModel)
        {
            ArgumentExceptionHelper.ThrowIfNull(locator);
            return locator.ResolveViewUnsafe(viewModel, null);
        }
    }
}
