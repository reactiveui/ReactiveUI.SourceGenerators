// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The accessibility of a generated property.</summary>
public enum PropertyAccessModifier
{
    /// <summary>A <c>public</c> property.</summary>
    Public,

    /// <summary>A <c>protected</c> property.</summary>
    Protected,

    /// <summary>An <c>internal</c> property.</summary>
    Internal,

    /// <summary>A <c>private</c> property.</summary>
    Private,

    /// <summary>A <c>protected internal</c> property.</summary>
    InternalProtected,

    /// <summary>A <c>private protected</c> property.</summary>
    PrivateProtected,
}
