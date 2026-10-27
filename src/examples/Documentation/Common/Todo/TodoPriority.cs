// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>How urgent a to-do item is.</summary>
public enum TodoPriority
{
    /// <summary>The item can wait.</summary>
    Low = 0,

    /// <summary>The item has no particular urgency.</summary>
    Normal = 1,

    /// <summary>The item needs attention soon.</summary>
    High = 2,
}
