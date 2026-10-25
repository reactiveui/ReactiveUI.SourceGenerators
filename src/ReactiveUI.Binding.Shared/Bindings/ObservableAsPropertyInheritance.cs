// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The modifier the <see cref="ObservableAsPropertyAttribute"/> code fix gives the property it writes for a field.</summary>
public enum ObservableAsPropertyInheritance
{
    /// <summary>No modifier.</summary>
    None = 0,

    /// <summary>The property is <see langword="virtual"/>.</summary>
    Virtual = 1,

    /// <summary>The property is an <see langword="override"/>.</summary>
    Override = 2,

    /// <summary>The property hides an inherited member with <see langword="new"/>.</summary>
    New = 3,
}
