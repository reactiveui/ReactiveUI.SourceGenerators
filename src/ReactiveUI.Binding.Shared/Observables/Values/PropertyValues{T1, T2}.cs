// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The values of 2 observed properties, as one emission.</summary>
/// <typeparam name="T1">The type of the first observed property.</typeparam>
/// <typeparam name="T2">The type of the second observed property.</typeparam>
/// <param name="Property1">The value of the first observed property.</param>
/// <param name="Property2">The value of the second observed property.</param>
[DebuggerDisplay("PropertyValues: {Property1}, {Property2}")]
public readonly record struct PropertyValues<T1, T2>(
    T1 Property1,
    T2 Property2);
