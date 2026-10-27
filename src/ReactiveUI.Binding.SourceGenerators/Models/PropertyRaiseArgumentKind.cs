// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>What a raise call passes to name the property.</summary>
internal enum PropertyRaiseArgumentKind
{
    /// <summary>The property name as a string.</summary>
    PropertyName = 0,

    /// <summary>A cached event-args instance for the property, so no event-args object is allocated per change.</summary>
    EventArgs = 1,
}
