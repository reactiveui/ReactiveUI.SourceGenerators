// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The inheritance modifier of a generated property.</summary>
public enum InheritanceModifier
{
    /// <summary>No modifier.</summary>
    None = 0,

    /// <summary>A <c>virtual</c> property.</summary>
    Virtual = 1,

    /// <summary>An <c>override</c> property.</summary>
    Override = 2,

    /// <summary>A <c>new</c> property.</summary>
    New = 3,
}
