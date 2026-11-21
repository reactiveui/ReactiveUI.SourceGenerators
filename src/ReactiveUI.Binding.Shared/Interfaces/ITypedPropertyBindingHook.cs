// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>A binding hook that reads the bound properties in their own types, so no value is boxed.</summary>
/// <remarks>
/// A generated binding calls this member when a registered hook implements it. A binding made through the
/// reflection-based <c>Unsafe</c> methods still calls the <see cref="IPropertyBindingHook"/> member.
/// </remarks>
public interface ITypedPropertyBindingHook : IPropertyBindingHook
{
    /// <summary>Decides whether a binding may be created.</summary>
    /// <param name="source">The source object, typically the view model; may be null.</param>
    /// <param name="target">The target object, typically the view.</param>
    /// <param name="sourcePath">The properties walked from the source.</param>
    /// <param name="targetPath">The properties walked from the target.</param>
    /// <param name="direction">Which way the binding runs.</param>
    /// <returns><see langword="true"/> to let the binding proceed; <see langword="false"/> to cancel it.</returns>
    bool ExecuteHook(
        object? source,
        object target,
        BindingHookPath sourcePath,
        BindingHookPath targetPath,
        BindingDirection direction);
}
