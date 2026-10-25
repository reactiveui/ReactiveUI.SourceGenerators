// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Refuses every two-way binding on the accounts screen, so a view can show an account but never write to it.</summary>
[System.Diagnostics.DebuggerDisplay("ReadOnlyAccountHook: Read-only account hook")]
public sealed class ReadOnlyAccountHook : IPropertyBindingHook
{
    /// <inheritdoc/>
    public bool ExecuteHook(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
        Func<IObservedChange<object, object>[]> getCurrentViewProperties,
        BindingDirection direction)
    {
        if (source is not AccountsViewModel || direction != BindingDirection.TwoWay)
        {
            return true;
        }

        Console.WriteLine("read-only: refused a two-way binding");
        return false;
    }
}
