// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Writes down every binding that is created and lets each one through.</summary>
/// <param name="name">The name that starts each line the hook writes.</param>
[System.Diagnostics.DebuggerDisplay("LoggingBindingHook: Logging binding hook")]
public sealed class LoggingBindingHook(string name) : IPropertyBindingHook
{
    /// <inheritdoc/>
    public bool ExecuteHook(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
        Func<IObservedChange<object, object>[]> getCurrentViewProperties,
        BindingDirection direction)
    {
        string sourceType = getCurrentViewModelProperties()[0].Sender.GetType().Name;
        string targetType = getCurrentViewProperties()[0].Sender.GetType().Name;

        Console.WriteLine($"{name}: {direction}: {sourceType} -> {targetType}");
        return true;
    }
}
