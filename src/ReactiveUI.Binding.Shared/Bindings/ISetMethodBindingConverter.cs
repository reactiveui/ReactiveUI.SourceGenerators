// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Replaces how a binding writes a value to its target, for example to fill a collection instead of assigning a property.</summary>
public interface ISetMethodBindingConverter : IEnableLogger
{
    /// <summary>Returns this converter's priority for writing a value of one type to a target of another.</summary>
    /// <param name="fromType">The type of the value being written; may be null.</param>
    /// <param name="toType">The type of the target being written to; may be null.</param>
    /// <returns>
    /// A positive value when <see cref="PerformSet"/> applies; zero or less excludes the converter.
    /// The registry picks the highest value and, on a tie, the earliest registered converter.
    /// </returns>
    int GetAffinityForObjects(Type? fromType, Type? toType);

    /// <summary>Writes a value to the target.</summary>
    /// <param name="toTarget">The object being written to.</param>
    /// <param name="newValue">The value to write.</param>
    /// <param name="arguments">The index arguments for an indexer target; a generated collection write passes null.</param>
    /// <returns>The result of the write; a generated collection write casts it to the target type and reports it as the new value.</returns>
    object? PerformSet(object? toTarget, object? newValue, object?[]? arguments);
}
