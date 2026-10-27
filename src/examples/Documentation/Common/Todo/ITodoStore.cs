// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>The to-do database. Every call is asynchronous, as a call to a real database is.</summary>
public interface ITodoStore
{
    /// <summary>Reads every item.</summary>
    /// <returns>Copies of the items, oldest first.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable.</exception>
    Task<IReadOnlyList<TodoItem>> QueryAsync();

    /// <summary>Reads the items whose title or notes contain a text.</summary>
    /// <param name="text">The text to look for.</param>
    /// <returns>Copies of the matching items, oldest first.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable.</exception>
    Task<IReadOnlyList<TodoItem>> QueryAsync(string text);

    /// <summary>Reads one item.</summary>
    /// <param name="id">The identifier of the item.</param>
    /// <returns>A copy of the item, or <see langword="null"/> when no item has that identifier.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable.</exception>
    Task<TodoItem?> GetAsync(int id);

    /// <summary>Stores a new item.</summary>
    /// <param name="item">The item to store; its identifier is ignored.</param>
    /// <returns>A copy of the stored item with its identifier assigned.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable or the title is blank.</exception>
    Task<TodoItem> AddAsync(TodoItem item);

    /// <summary>Replaces the stored values of an item.</summary>
    /// <param name="item">The item, identified by <see cref="TodoItem.Id"/>.</param>
    /// <returns>A copy of the stored item.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable or no item has that identifier.</exception>
    Task<TodoItem> UpdateAsync(TodoItem item);

    /// <summary>Removes an item.</summary>
    /// <param name="id">The identifier of the item.</param>
    /// <returns>A task that completes when the item is removed.</returns>
    /// <exception cref="TodoStoreException">The database is unreachable or no item has that identifier.</exception>
    Task DeleteAsync(int id);
}
