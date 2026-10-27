// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>A view locator for an application with one screen: it answers for the to-do list and for nothing else.</summary>
[System.Diagnostics.DebuggerDisplay("TodoViewLocator: Screen = TodoView")]
public sealed class TodoViewLocator : IViewLocator
{
    /// <inheritdoc/>
    public IViewFor? ResolveView(object? viewModel, string? contract) =>
        viewModel is TodoListViewModel todoList ? new TodoView { ViewModel = todoList } : null;

    /// <inheritdoc/>
    public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class =>
        viewModel is TodoListViewModel todoList ? new TodoView { ViewModel = todoList } : null;

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Part of IViewLocator. This locator builds no type at run time, so it answers as ResolveView does.")]
    public IViewFor? ResolveViewUnsafe(object? viewModel, string? contract) => ResolveView(viewModel, contract);
}
