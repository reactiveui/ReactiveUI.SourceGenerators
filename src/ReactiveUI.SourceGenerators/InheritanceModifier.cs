// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The inheritance modifier of a generated property.</summary>
public enum InheritanceModifier
{
    /// <summary>No modifier.</summary>
    None,

    /// <summary>A <c>virtual</c> property.</summary>
    Virtual,

    /// <summary>An <c>override</c> property.</summary>
    Override,

    /// <summary>A <c>new</c> property.</summary>
    New,
}
