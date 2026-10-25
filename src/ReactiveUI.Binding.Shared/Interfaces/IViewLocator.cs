// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves views for view models.</summary>
/// <remarks>
/// A locator has an ahead-of-time safe way to resolve a view model held as an <see cref="object"/>,
/// <see cref="ResolveView(object?, string?)"/>, and a reflective one, <see cref="ResolveViewUnsafe(object?, string?)"/>.
/// An implementation keeps <see cref="ResolveView(object?, string?)"/> free of reflection over the view model's
/// runtime type, and puts any such step in <see cref="ResolveViewUnsafe(object?, string?)"/> only.
/// </remarks>
public interface IViewLocator : IEnableLogger
{
    /// <summary>Resolves a view for the view model using its compile-time type, without reflection over the view model's runtime type.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">The contract to resolve under, or null for the default view.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class;

    /// <summary>Resolves a view for the view model using its runtime type, without building any type at run time.</summary>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">The contract to resolve under, or null for the default view.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    /// <remarks>
    /// This member is safe to call from a trimmed or native AOT application. <see cref="DefaultViewLocator"/> asks the
    /// source-generated lookups and then its explicit mappings, keyed by the view model's runtime type. It does not ask
    /// the service locator for <c>IViewFor&lt;T&gt;</c> closed over the runtime type. Call
    /// <see cref="ResolveViewUnsafe(object?, string?)"/> for that step.
    /// </remarks>
    /// <example>
    /// <code>
    /// object viewModel = navigationStack.Peek();
    /// var view = ViewLocator.GetCurrent().ResolveView(viewModel, contract: null);
    /// </code>
    /// </example>
    IViewFor? ResolveView(object? viewModel, string? contract);

    /// <summary>Resolves a view for the view model using its runtime type, and falls back to steps that build a type at run time.</summary>
    /// <param name="viewModel">The view model instance to resolve a view for.</param>
    /// <param name="contract">The contract to resolve under, or null for the default view.</param>
    /// <returns>The resolved view, or <see langword="null"/> if no view is found.</returns>
    /// <remarks>
    /// <see cref="DefaultViewLocator"/> runs the steps of <see cref="ResolveView(object?, string?)"/> first. When they
    /// find nothing, it closes <c>IViewFor&lt;&gt;</c> over the view model's runtime type and asks the service locator
    /// for it. A native AOT application cannot build that type, so prefer <see cref="ResolveView(object?, string?)"/>
    /// with a generated view or a <c>Map</c> registration. An implementation carries the same
    /// <see cref="RequiresDynamicCodeAttribute"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// // The view is registered only as IViewFor&lt;TodoViewModel&gt; in the service locator.
    /// object viewModel = new TodoViewModel();
    /// var view = ViewLocator.GetCurrent().ResolveViewUnsafe(viewModel, contract: null);
    /// </code>
    /// </example>
    [RequiresDynamicCode(ViewLocatorMixins.ResolveViewUnsafeMessage)]
    IViewFor? ResolveViewUnsafe(object? viewModel, string? contract);
}
