// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Receives each property of a bound path with its owner and value in their own types.</summary>
/// <remarks>A <see cref="BindingHookPath"/> calls the visitor once per property it reaches, in path order.</remarks>
public interface IBindingHookPropertyVisitor
{
    /// <summary>Receives one property of the path.</summary>
    /// <typeparam name="TOwner">The type that declares the property.</typeparam>
    /// <typeparam name="TValue">The property's type.</typeparam>
    /// <param name="owner">The object the property was read from.</param>
    /// <param name="property">The lambda that names the property.</param>
    /// <param name="value">The property's current value.</param>
    void Visit<TOwner, TValue>(TOwner owner, Expression<Func<TOwner, TValue>> property, TValue value);
}
