// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The accessibility of a generated property.</summary>
public enum PropertyAccessModifier
{
    /// <summary>A <c>public</c> property.</summary>
    Public = 0,

    /// <summary>A <c>protected</c> property.</summary>
    Protected = 1,

    /// <summary>An <c>internal</c> property.</summary>
    Internal = 2,

    /// <summary>A <c>private</c> property.</summary>
    Private = 3,

    /// <summary>A <c>protected internal</c> property.</summary>
    InternalProtected = 4,

    /// <summary>A <c>private protected</c> property.</summary>
    PrivateProtected = 5,
}
