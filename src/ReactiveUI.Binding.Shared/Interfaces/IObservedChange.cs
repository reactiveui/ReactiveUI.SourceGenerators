// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Describes one property change: the object that raised it, the expression naming the property, and its value.</summary>
/// <typeparam name="TSender">The type of the object that raised the change.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public interface IObservedChange<out TSender, out TValue>
{
    /// <summary>Gets the object that raised the change.</summary>
    TSender Sender { get; }

    /// <summary>Gets the expression of the member that changed.</summary>
    Expression? Expression { get; }

    /// <summary>Gets the value the change carries; a source that raises a notification without reading the property leaves it at the default.</summary>
    TValue Value { get; }
}
