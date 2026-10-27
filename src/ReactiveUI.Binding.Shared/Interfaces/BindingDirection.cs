// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Specifies the direction of a property binding.</summary>
public enum BindingDirection
{
    /// <summary>One-way binding from source to target.</summary>
    OneWay = 0,

    /// <summary>Two-way binding between source and target.</summary>
    TwoWay = 1,

    /// <summary>One-way asynchronous binding from source to target.</summary>
    AsyncOneWay = 2,
}
