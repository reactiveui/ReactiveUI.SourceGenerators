// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>
/// The view model behind the to-do screen. It loads items from an <see cref="ITodoStore"/>, filters them by text
/// and keeps a count of the items still to do. Each command starts the matching method and returns; await the
/// method itself to wait for its work.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TodoListViewModel: Items = {Items.Count}, RemainingCount = {RemainingCount}")]
public sealed class TodoListViewModel : ObservableObject
{
    /// <summary>The database the view model reads and writes.</summary>
    private readonly ITodoStore _store;

    /// <summary>Every loaded item, before the filter is applied.</summary>
    private List<TodoItem> _all = [];

    /// <summary>Initializes a new instance of the <see cref="TodoListViewModel"/> class.</summary>
    /// <param name="store">The database to read and write.</param>
    public TodoListViewModel(ITodoStore store)
    {
        _store = store;
        LoadCommand = new(() => _ = LoadAsync());
        AddCommand = new(() => _ = AddAsync(), () => !string.IsNullOrWhiteSpace(NewTitle));
        CompleteCommand = new(() => _ = CompleteAsync(), () => SelectedItem is { IsDone: false });
    }

    /// <summary>Gets the command that runs <see cref="LoadAsync"/>.</summary>
    public Command LoadCommand { get; }

    /// <summary>Gets the command that runs <see cref="AddAsync"/>; it runs only while <see cref="NewTitle"/> is not blank.</summary>
    public Command AddCommand { get; }

    /// <summary>Gets the command that runs <see cref="CompleteAsync"/>; it runs only while an unfinished item is selected.</summary>
    public Command CompleteCommand { get; }

    /// <summary>Gets the items that match <see cref="FilterText"/>.</summary>
    public IReadOnlyList<TodoItem> Items
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the selected item, or <see langword="null"/> when nothing is selected.</summary>
    public TodoItem? SelectedItem
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CompleteCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>Gets or sets the title of the item the user is about to add.</summary>
    public string NewTitle
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                AddCommand.ChangeCanExecute();
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the text that <see cref="Items"/> is narrowed to; blank shows every item.</summary>
    public string FilterText
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ApplyFilter();
            }
        }
    } = string.Empty;

    /// <summary>Gets the number of loaded items that are not finished, whatever the filter.</summary>
    public int RemainingCount
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last failed database call, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Reads every item and replaces the list.</summary>
    /// <returns>A task that completes when the list is replaced.</returns>
    public async Task LoadAsync()
    {
        try
        {
            IReadOnlyList<TodoItem> rows = await _store.QueryAsync();
            ErrorMessage = string.Empty;
            ReplaceAll(rows);
        }
        catch (TodoStoreException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Stores <see cref="NewTitle"/> as a new item and selects it.</summary>
    /// <returns>A task that completes when the item is stored.</returns>
    public async Task AddAsync()
    {
        try
        {
            TodoItem added = await _store.AddAsync(new TodoItem { Title = NewTitle.Trim() });
            ErrorMessage = string.Empty;
            NewTitle = string.Empty;
            ReplaceAll(_all.Append(added));
            SelectedItem = Find(added.Id);
        }
        catch (TodoStoreException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Stores the selected item as finished.</summary>
    /// <returns>A task that completes when the item is stored.</returns>
    public async Task CompleteAsync()
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        try
        {
            TodoItem update = item.Clone();
            update.IsDone = true;
            TodoItem saved = await _store.UpdateAsync(update);
            ErrorMessage = string.Empty;
            item.IsDone = saved.IsDone;
            CompleteCommand.ChangeCanExecute();
        }
        catch (TodoStoreException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Replaces every loaded item and follows the new items for changes to <see cref="TodoItem.IsDone"/>.</summary>
    /// <param name="rows">The new set of items.</param>
    private void ReplaceAll(IEnumerable<TodoItem> rows)
    {
        foreach (TodoItem old in _all)
        {
            old.PropertyChanged -= OnItemChanged;
        }

        _all = [.. rows];

        foreach (TodoItem item in _all)
        {
            item.PropertyChanged += OnItemChanged;
        }

        ApplyFilter();
    }

    /// <summary>Rebuilds <see cref="Items"/> from the loaded items and the filter, keeping the selection when it survives.</summary>
    private void ApplyFilter()
    {
        int? selectedId = SelectedItem?.Id;

        Items = _all.Where(item => item.Matches(FilterText)).ToList();
        SelectedItem = selectedId is { } id ? Find(id) : null;
        RemainingCount = CountRemaining();
    }

    /// <summary>Finds a visible item.</summary>
    /// <param name="id">The identifier of the item.</param>
    /// <returns>The item, or <see langword="null"/> when the filter hides it or it does not exist.</returns>
    private TodoItem? Find(int id) => Items.FirstOrDefault(item => item.Id == id);

    /// <summary>Counts the loaded items that are not finished.</summary>
    /// <returns>The number of unfinished items.</returns>
    private int CountRemaining() => _all.Count(static item => !item.IsDone);

    /// <summary>Recounts the remaining items when one is finished or reopened.</summary>
    /// <param name="sender">The item that changed.</param>
    /// <param name="e">The event data.</param>
    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TodoItem.IsDone))
        {
            return;
        }

        RemainingCount = CountRemaining();
        CompleteCommand.ChangeCanExecute();
    }
}
