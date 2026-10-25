// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Shows a to-do item as its title. The hint <see cref="UpperCaseHint"/> asks for capital letters.</summary>
public sealed class TodoItemTitleConverter : BindingTypeConverter<TodoItem, string>
{
    /// <summary>Gets the conversion hint that asks for the title in capital letters.</summary>
    public static string UpperCaseHint { get; } = "upper";

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 1;

    /// <inheritdoc/>
    public override bool TryConvert(TodoItem? from, object? conversionHint, out string? result)
    {
        string title = from?.Title ?? string.Empty;
        result = Equals(conversionHint, UpperCaseHint) ? title.ToUpperInvariant() : title;
        return true;
    }
}
