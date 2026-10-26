// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators;

/// <summary>The accessibility of a generated property's set accessor.</summary>
public enum AccessModifier
{
    /// <summary>A <c>public</c> accessor.</summary>
    Public = 0,

    /// <summary>A <c>protected</c> accessor.</summary>
    Protected = 1,

    /// <summary>An <c>internal</c> accessor.</summary>
    Internal = 2,

    /// <summary>A <c>private</c> accessor.</summary>
    Private = 3,

    /// <summary>A <c>protected internal</c> accessor.</summary>
    InternalProtected = 4,

    /// <summary>A <c>private protected</c> accessor.</summary>
    PrivateProtected = 5,

    /// <summary>An <c>init</c> accessor.</summary>
    Init = 6,
}
