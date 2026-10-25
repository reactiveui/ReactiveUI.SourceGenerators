// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Concrete implementation of <see cref="IObservedChange{TSender, TValue}"/>.</summary>
/// <typeparam name="TSender">The type of the object that raised the change.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
[DebuggerDisplay("ObservedChange: Expression = {Expression}, Sender = {Sender}, Value = {Value}")]
public class ObservedChange<TSender, TValue> : IObservedChange<TSender, TValue>
{
    /// <summary>Initializes a new instance of the <see cref="ObservedChange{TSender, TValue}"/> class.</summary>
    /// <param name="sender">The object that raised the change.</param>
    /// <param name="expression">The expression of the member that changed.</param>
    /// <param name="value">The value of the property; the default when the reader is to fetch it from <paramref name="sender"/>.</param>
    public ObservedChange(TSender sender, Expression? expression, TValue value)
    {
        Sender = sender;
        Expression = expression;
        Value = value;
    }

    /// <inheritdoc/>
    public TSender Sender { get; }

    /// <inheritdoc/>
    public Expression? Expression { get; }

    /// <inheritdoc/>
    public TValue Value { get; }
}
