// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Implement this as a way to intercept bindings at the time that they are
/// created and execute an additional action (or to cancel the binding).
/// </summary>
public interface IPropertyBindingHook
{
    /// <summary>Called as a binding is set up, before it is wired, and can refuse the binding.</summary>
    /// <param name="source">The source ViewModel; may be null.</param>
    /// <param name="target">The target View (not the actual control).</param>
    /// <param name="getCurrentViewModelProperties">Reads the current values along the view model property path when called.</param>
    /// <param name="getCurrentViewProperties">Reads the current values along the view property path when called.</param>
    /// <param name="direction">The Binding direction.</param>
    /// <returns><see langword="true"/> to let the binding proceed; <see langword="false"/> to cancel it.</returns>
    bool ExecuteHook(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
        Func<IObservedChange<object, object>[]> getCurrentViewProperties,
        BindingDirection direction);
}
